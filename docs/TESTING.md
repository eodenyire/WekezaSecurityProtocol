# Salama Security Protocol - Testing Guide

## Overview

This guide covers testing strategies for the Salama Security Protocol, including unit tests, integration tests, and security testing.

## Test Strategy

### Test Pyramid

```
              /\
             /  \
            / E2E \
           /--------\
          /          \
         / Integration \
        /--------------\
       /                \
      /   Unit Tests     \
     /--------------------\
```

1. **Unit Tests (70%)** - Test individual components in isolation
2. **Integration Tests (20%)** - Test component interactions
3. **E2E Tests (10%)** - Test complete user flows

## Test Categories

### 1. Authentication Tests

#### Test Cases
- ✅ Standard PIN authentication succeeds
- ✅ Reversed PIN activates shadow mode
- ✅ Duress code activates shadow mode
- ✅ High accelerometer variance activates shadow mode
- ✅ Invalid PIN returns error
- ✅ Rate limiting enforced (5 attempts per 15 minutes)
- ✅ Account lockout after 10 failed attempts
- ✅ JWT token generated with correct scope
- ✅ Token expiration set correctly (6 hours)

#### Example Test
```csharp
[Fact]
public async Task Login_WithReversedPIN_ActivatesShadowMode()
{
    // Arrange
    var user = new User 
    { 
        AccountId = "254712345678",
        PinHash = HashPin("1234"),
        PlainPin = "1234" // Encrypted in production
    };
    await _userRepo.AddUserAsync(user);

    var request = new AuthenticationRequest
    {
        AccountId = "254712345678",
        PinHash = HashPin("4321"), // Reversed
        BehavioralData = null
    };

    // Act
    var response = await _authService.AuthenticateAsync(request);

    // Assert
    Assert.True(response.Success);
    Assert.Equal(SessionMode.Shadow, response.SessionMode);
    
    // Verify silent alert was sent
    _mockAlertService.Verify(
        x => x.SendAlertAsync(It.IsAny<SecurityAlert>()), 
        Times.Once
    );
}
```

### 2. Shadow Service Tests

#### Test Cases
- ✅ Shadow balance returns 100-500 KES
- ✅ Shadow transaction history contains realistic transactions
- ✅ Shadow transfer returns fake success
- ✅ Transfer quarantined in database with is_duress=true
- ✅ Mock transaction IDs look authentic
- ✅ No actual funds transferred

#### Example Test
```csharp
[Fact]
public async Task GetBalance_InShadowMode_ReturnsLowBalance()
{
    // Arrange
    var shadowService = new ShadowBankingService();
    var accountId = "254712345678";

    // Act
    var balance = await shadowService.GetBalanceAsync(accountId);

    // Assert
    Assert.InRange(balance.Balance, 100, 500);
    Assert.Equal("KES", balance.Currency);
}

[Fact]
public async Task ProcessTransfer_InShadowMode_QuarantinesTransaction()
{
    // Arrange
    var request = new TransferRequest
    {
        FromAccountId = "254712345678",
        RecipientAccount = "254722334455",
        Amount = 5000,
        Currency = "KES"
    };

    // Act
    var response = await _shadowService.ProcessTransferAsync(request);

    // Assert
    Assert.True(response.Success);
    Assert.NotEmpty(response.TransactionId);
    
    // Verify quarantine entry created
    var quarantined = await _db.DuressQuarantine
        .FirstOrDefaultAsync(q => q.MockTransactionId == response.TransactionId);
    
    Assert.NotNull(quarantined);
    Assert.True(quarantined.IsDuress);
    Assert.Equal("Quarantined", quarantined.Status);
}
```

### 3. Middleware Tests

#### Test Cases
- ✅ Standard mode token routes to real services
- ✅ Shadow mode token routes to shadow services
- ✅ Session mode added to HttpContext
- ✅ X-Session-Mode header set for shadow mode

#### Example Test
```csharp
[Fact]
public async Task Middleware_WithShadowToken_SetsContextCorrectly()
{
    // Arrange
    var context = new DefaultHttpContext();
    var shadowToken = GenerateJwtToken("254712345678", new[] { "shadow" });
    context.Request.Headers["Authorization"] = $"Bearer {shadowToken}";

    var middleware = new SalamaRoutingMiddleware(next: (innerContext) => Task.CompletedTask);

    // Act
    await middleware.InvokeAsync(context);

    // Assert
    Assert.True((bool)context.Items["IsShadowMode"]!);
    Assert.Equal("SHADOW", context.Request.Headers["X-Session-Mode"]);
}
```

### 4. Integration Tests

#### Test Cases
- ✅ Complete authentication flow (PIN → token → balance)
- ✅ Standard mode transfer executes on real API
- ✅ Shadow mode transfer quarantined
- ✅ Silent alert sent when shadow mode activated
- ✅ 6-hour lockout enforced

#### Example Test
```csharp
[Fact]
public async Task CompleteFlow_ShadowMode_TransferQuarantined()
{
    // Arrange - Create user
    var user = await CreateTestUser("254712345678", "1234");

    // Act 1 - Login with reversed PIN
    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
        new AuthenticationRequest
        {
            AccountId = "254712345678",
            PinHash = HashPin("4321") // Reversed
        });
    
    var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
    Assert.True(authResult.Success);

    // Act 2 - Make transfer
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", authResult.SessionToken);
    
    var transferResponse = await _client.PostAsJsonAsync("/api/v1/transfers/mobile-money",
        new TransferRequest
        {
            RecipientAccount = "254722334455",
            Amount = 10000,
            Currency = "KES"
        });

    var transferResult = await transferResponse.Content.ReadFromJsonAsync<TransferResponse>();

    // Assert
    Assert.True(transferResult.Success);
    
    // Verify transaction quarantined
    var quarantined = await _db.DuressQuarantine
        .Where(q => q.UserId == user.UserId)
        .FirstOrDefaultAsync();
    
    Assert.NotNull(quarantined);
    Assert.Equal(10000, quarantined.Amount);
    Assert.True(quarantined.IsDuress);
}
```

## Running Tests

### Unit Tests

```bash
# Run all unit tests
dotnet test --filter Category=Unit

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test class
dotnet test --filter FullyQualifiedName~AuthenticationServiceTests

# Run in watch mode (auto-run on file changes)
dotnet watch test
```

### Integration Tests

```bash
# Run all integration tests
dotnet test --filter Category=Integration

# Requires test database
export ConnectionStrings__DefaultConnection="Host=localhost;Database=salama_test;Username=test_user;Password=test_pass"
dotnet test --filter Category=Integration
```

### Security Tests

```bash
# Run security-specific tests
dotnet test --filter Category=Security
```

## Test Coverage Goals

- **Overall Coverage:** Minimum 80%
- **Critical Paths:** 100% (authentication, shadow routing, quarantine)
- **Edge Cases:** 90%

### Generate Coverage Report

```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report

# Open report
open coverage-report/index.html
```

## Security Testing

### 1. PIN Brute Force Testing

```bash
# Test rate limiting
for i in {1..10}; do
  curl -X POST https://localhost:5001/api/v1/auth/login \
    -H "Content-Type: application/json" \
    -d "{\"accountId\":\"254712345678\",\"pinHash\":\"wrong\"}"
done

# Should return 429 Too Many Requests after 5 attempts
```

### 2. Token Tampering Testing

```csharp
[Fact]
public async Task TamperedToken_ReturnsUnauthorized()
{
    // Arrange
    var validToken = GenerateJwtToken("254712345678", new[] { "standard" });
    var parts = validToken.Split('.');
    
    // Tamper with payload (change standard to shadow)
    var tamperedPayload = parts[1].Replace("standard", "shadow");
    var tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", tamperedToken);

    // Act
    var response = await _client.GetAsync("/api/v1/account/balance");

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

### 3. Shadow Mode Detection Testing

**Objective:** Ensure attacker cannot detect shadow mode from client

```csharp
[Theory]
[InlineData("standard")]
[InlineData("shadow")]
public async Task API_ResponsesIdentical_RegardlessOfMode(string mode)
{
    // Both modes should return identical response structure
    var token = GenerateJwtToken("254712345678", new[] { mode });
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);

    var response = await _client.GetAsync("/api/v1/account/balance");
    var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();

    // Assert identical structure
    Assert.NotNull(balance.AccountId);
    Assert.NotNull(balance.Currency);
    Assert.True(balance.Balance >= 0);
    
    // No mode indicator in response
    var json = await response.Content.ReadAsStringAsync();
    Assert.DoesNotContain("shadow", json, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("duress", json, StringComparison.OrdinalIgnoreCase);
}
```

## Performance Testing

### Load Testing with k6

```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests under 500ms
  },
};

export default function () {
  // Login
  const loginRes = http.post('https://api.wekeza.com/security/v1/api/v1/auth/login', 
    JSON.stringify({
      accountId: '254712345678',
      pinHash: 'hashed_pin_value'
    }), 
    { headers: { 'Content-Type': 'application/json' } }
  );
  
  check(loginRes, {
    'login successful': (r) => r.status === 200,
  });

  const token = JSON.parse(loginRes.body).sessionToken;

  // Get balance
  const balanceRes = http.get('https://api.wekeza.com/security/v1/api/v1/account/balance', {
    headers: { 'Authorization': `Bearer ${token}` },
  });
  
  check(balanceRes, {
    'balance retrieved': (r) => r.status === 200,
  });

  sleep(1);
}
```

Run with:
```bash
k6 run load-test.js
```

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: salama_test
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_pass
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true
      env:
        ConnectionStrings__DefaultConnection: "Host=localhost;Database=salama_test;Username=test_user;Password=test_pass"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3
      with:
        file: coverage.opencover.xml
```

## Test Data Management

### Test Users

```csharp
public class TestDataFactory
{
    public static User CreateStandardUser()
    {
        return new User
        {
            AccountId = "254712345678",
            AccountName = "John Doe",
            AccountType = "Savings",
            Currency = "KES",
            PinHash = HashPin("1234"),
            PlainPin = "1234",
            DuressCodeHash = HashPin("9999")
        };
    }

    public static User CreateShadowModeUser()
    {
        return new User
        {
            AccountId = "254722334455",
            AccountName = "Jane Smith",
            AccountType = "Current",
            Currency = "KES",
            PinHash = HashPin("5678"),
            PlainPin = "5678",
            IsInShadowMode = true,
            ShadowModeActivatedAt = DateTime.UtcNow.AddHours(-2)
        };
    }
}
```

## Debugging Tests

### Visual Studio
- Set breakpoints in test methods
- Right-click test → Debug Test(s)

### VS Code
- Install C# Dev Kit extension
- Use Test Explorer
- Click debug icon next to test

### Command Line
```bash
# Run single test with detailed output
dotnet test --filter FullyQualifiedName~Login_WithReversedPIN_ActivatesShadowMode --logger "console;verbosity=detailed"
```

## Best Practices

1. **Arrange-Act-Assert Pattern** - Structure all tests clearly
2. **Independent Tests** - No dependencies between tests
3. **Descriptive Names** - Test names describe what they test
4. **Fast Tests** - Unit tests run in milliseconds
5. **Isolated Tests** - Use mocks for external dependencies
6. **Test Edge Cases** - Test boundaries and error conditions
7. **Clean Up** - Reset database state after integration tests

## Support

For testing support:
- QA Team: qa@wekeza.com
- Test Documentation: https://docs.wekeza.com/testing

# USSD (*234#) Integration Example

## Overview
This example demonstrates how to integrate the Salama Security Protocol with USSD menu-based banking for feature phones.

## Features
- ✅ Reversed PIN detection
- ✅ Text-based shadow mode
- ✅ Feature phone support
- ✅ No internet required

## USSD Flow

```
User: *234#
System: Welcome to Wekeza Bank
        Enter PIN:

User enters: 4321 (reversed, normal is 1234)

System: Welcome to Wekeza Bank
        Balance: KES 287.30       ← Shadow mode (mock balance)
        1. Send Money
        2. Check Balance
        3. Mini Statement
        4. My Account
        0. Exit

User: 1 (Send Money)

System: Enter recipient phone:
User: 254722334455

System: Enter amount:
User: 5000

System: Transaction Successful    ← Fake success (quarantined)
        Sent KES 5,000.00
        To: 254722334455
        Ref: WKZ20260212103001234
        New Balance: KES 187.30
```

## Example Implementation

```csharp
[HttpPost("api/ussd")]
public async Task<IActionResult> HandleUssd([FromBody] UssdRequest request)
{
    var ussdAdapter = new UssdChannelAdapter();
    
    // Step 1: PIN Entry
    if (request.Step == 0)
    {
        return Ok(new UssdResponse
        {
            Message = "Welcome to Wekeza Bank\nEnter PIN:",
            ContinueSession = true
        });
    }
    
    // Step 2: Authenticate
    if (request.Step == 1)
    {
        var authRequest = ussdAdapter.CreateAuthRequest(
            phoneNumber: request.PhoneNumber,
            pinCode: request.Input,
            ussdSessionId: request.SessionId,
            ussdCode: $"*234*{request.Input}#"
        );
        
        var authResponse = await _authService.AuthenticateAsync(authRequest);
        
        string menu;
        if (authResponse.SessionMode == SessionMode.Shadow)
        {
            menu = ussdAdapter.GenerateShadowMenu(request.PhoneNumber);
        }
        else
        {
            var balance = await _bankingService.GetBalanceAsync(
                request.PhoneNumber, false);
            menu = ussdAdapter.GenerateStandardMenu(
                request.PhoneNumber, balance.Balance);
        }
        
        return Ok(new UssdResponse
        {
            Message = menu,
            ContinueSession = true
        });
    }
    
    // Handle menu navigation...
}
```

## Menu Examples

### Standard Mode
```
Welcome to Wekeza Bank
Balance: KES 15,234.50
1. Send Money
2. Check Balance
3. Mini Statement
4. My Account
0. Exit
```

### Shadow Mode (Identical Appearance)
```
Welcome to Wekeza Bank
Balance: KES 287.30
1. Send Money
2. Check Balance
3. Mini Statement
4. My Account
0. Exit
```

## Transaction Examples

### Standard Mode Transaction
```
Transaction Successful
Sent KES 5,000.00
To: 254722334455
Ref: WKZ20260212103001234
New Balance: KES 10,234.50
```

### Shadow Mode Transaction (Quarantined)
```
Transaction Successful         ← Fake success
Sent KES 5,000.00
To: 254722334455
Ref: WKZ20260212103005678     ← Mock transaction ID
New Balance: KES 187.30        ← Mock balance
```

## Telco Integration

### Safaricom
- USSD Code: *234#
- Session timeout: 180 seconds
- Max message length: 182 characters

### Airtel
- USSD Code: *234#
- Session timeout: 120 seconds
- Max message length: 160 characters

### Telkom
- USSD Code: *234#
- Session timeout: 120 seconds
- Max message length: 160 characters

## Limitations
- Text-only interface (no rich UI)
- No accelerometer data
- No GPS tracking
- Relies solely on reversed PIN
- Session timeout constraints

## Advantages
- Works on feature phones
- No smartphone required
- No internet required
- Wide accessibility

## See Also
- [Multi-Channel Guide](../../docs/MULTI_CHANNEL_GUIDE.md)
- [API Reference](../../docs/API_REFERENCE.md)

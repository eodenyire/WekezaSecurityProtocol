# Salama Security Protocol - Multi-Channel Integration Guide

## Overview

The Salama Security Protocol is designed to work seamlessly across **all banking channels**, providing duress protection regardless of how users access their accounts.

## Supported Channels

| Channel | Description | Duress Detection | Behavioral Signals |
|---------|-------------|------------------|-------------------|
| **Mobile Apps** | iOS & Android native apps | Reversed PIN + Accelerometer | ✅ Full Support |
| **Web Portal** | Browser-based banking | Reversed PIN + Mouse/Keyboard | ✅ Full Support |
| **STK Push** | M-Pesa mobile money | Reversed PIN via USSD | ⚠️ Limited |
| **USSD** | *234# menu-based | Reversed PIN | ❌ PIN-only |

---

## Channel Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ALL CHANNELS                              │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐      │
│  │  Mobile  │ │   Web    │ │STK Push  │ │  USSD    │      │
│  │   App    │ │  Portal  │ │ (M-Pesa) │ │  *234#   │      │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘      │
│       │            │            │            │              │
│       └────────────┴────────────┴────────────┘              │
│                         │                                    │
└─────────────────────────┼────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│         Channel Adapters (Translation Layer)                 │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │Mobile Adapter│ │  Web Adapter │ │ USSD Adapter │       │
│  └──────┬───────┘ └──────┬───────┘ └──────┬───────┘       │
│         └────────────────┴────────────────┘                 │
│                         │                                    │
│                         ▼                                    │
│              Common AuthenticationRequest                    │
│              (Channel-agnostic model)                        │
└─────────────────────────┼────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│           Salama Authentication Service                      │
│  • Reversed PIN detection                                   │
│  • Behavioral analysis (if available)                       │
│  • Shadow/Standard mode determination                       │
└─────────────────────────┼────────────────────────────────────┘
                          │
                ┌─────────┴─────────┐
                ▼                   ▼
         Standard Mode         Shadow Mode
         (Real Banking)      (Mock Data + Alert)
```

---

## Channel Integration Details

### 1. Mobile Apps (iOS/Android)

**✅ Full Support** - Complete duress detection with all features

#### Duress Detection Methods
1. **Reversed PIN** - User enters PIN backwards (e.g., 4321 instead of 1234)
2. **Accelerometer Variance** - Device tremor during PIN entry (threshold: 0.5)
3. **Entry Duration** - Unusually slow PIN entry
4. **Correction Count** - Multiple backspaces during entry

#### Integration Example

```swift
// iOS Swift
import WekezaSecuritySDK

class LoginViewController {
    let salamaAuth = SalamaAuthManager()
    
    func handleLogin(accountId: String, pin: String) async {
        // Start behavioral data collection
        salamaAuth.startPINEntry()
        
        // User enters PIN (collect accelerometer data)
        
        // Calculate behavioral data
        let behavioralData = salamaAuth.calculateBehavioralData()
        
        // Hash PIN client-side
        let pinHash = sha256(pin)
        
        // Create authentication request
        let request = AuthenticationRequest(
            accountId: accountId,
            pinHash: pinHash,
            deviceId: UIDevice.current.identifierForVendor?.uuidString,
            location: getCurrentLocation(),
            behavioralData: behavioralData,
            channelMetadata: ChannelMetadata(
                channel: .mobileApp,
                userAgent: "WekeziOS/1.0",
                ipAddress: getIPAddress()
            )
        )
        
        // Authenticate
        let response = try await api.authenticate(request)
        
        // Store token (client never knows if shadow mode)
        KeychainHelper.save(token: response.sessionToken)
        
        // Navigate to dashboard
        navigateToDashboard()
    }
}
```

#### Key Features
- ✅ Real-time accelerometer monitoring
- ✅ GPS tracking (with user permission)
- ✅ Silent SOC alerts
- ✅ Seamless shadow/standard mode switching
- ✅ Secure token storage (Keychain/EncryptedSharedPreferences)

---

### 2. Web Portal (Browser-Based)

**✅ Full Support** - Web-specific behavioral detection

#### Duress Detection Methods
1. **Reversed PIN** - User enters PIN backwards
2. **Mouse Movement Variance** - Erratic mouse movements (threshold: 0.6)
3. **Typing Patterns** - Long pauses between keystrokes (>2 seconds)
4. **Tab/Window Switches** - Context changes during PIN entry
5. **Unusual Time of Day** - Login at atypical hours for user

#### Integration Example

```javascript
// JavaScript (Browser)
class WekeziWebAuth {
    constructor() {
        this.behavioralData = {
            mouseMovements: [],
            typingDelays: [],
            tabSwitches: 0,
            windowBlurs: 0
        };
    }
    
    startPINEntry() {
        // Track mouse movements
        document.addEventListener('mousemove', (e) => {
            this.behavioralData.mouseMovements.push({
                x: e.clientX,
                y: e.clientY,
                timestamp: Date.now()
            });
        });
        
        // Track tab switches
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.behavioralData.tabSwitches++;
            }
        });
        
        // Track window blur
        window.addEventListener('blur', () => {
            this.behavioralData.windowBlurs++;
        });
    }
    
    calculateBehavioralData() {
        // Calculate mouse movement variance
        const mouseVariance = this.calculateMouseVariance();
        
        return {
            accelerometerVariance: mouseVariance, // Map to common model
            entryDurationMs: this.getTotalEntryDuration(),
            correctionCount: this.behavioralData.backspaceCount
        };
    }
    
    async login(accountId, pin) {
        const behavioralData = this.calculateBehavioralData();
        
        const request = {
            accountId: accountId,
            pinHash: await this.hashPin(pin),
            deviceId: this.getBrowserFingerprint(),
            behavioralData: behavioralData,
            channelMetadata: {
                channel: 'WebPortal',
                userAgent: navigator.userAgent,
                ipAddress: await this.getIPAddress(),
                browserFingerprint: this.getBrowserFingerprint(),
                additionalData: {
                    screenResolution: `${screen.width}x${screen.height}`,
                    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
                }
            }
        };
        
        const response = await fetch('/api/v1/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        
        const data = await response.json();
        
        // Store token securely
        sessionStorage.setItem('sessionToken', data.sessionToken);
        
        // Redirect to dashboard
        window.location.href = '/dashboard';
    }
}
```

#### Key Features
- ✅ Mouse tremor detection
- ✅ Typing pattern analysis
- ✅ Browser fingerprinting
- ✅ Session-based authentication
- ✅ Same UI for both modes

---

### 3. STK Push (M-Pesa Integration)

**⚠️ Limited Support** - Reversed PIN via USSD code

#### Duress Detection Methods
1. **Reversed PIN via USSD** - User dials *234*4321# instead of *234*1234#
2. **Rapid Retry Attempts** - Multiple failed attempts in short period

#### Integration Flow

```
User Flow:
1. User receives STK Push prompt on phone
2. User enters PIN via M-Pesa interface
3. M-Pesa sends callback to Wekeza API
4. Salama Protocol checks for reversed PIN
5. If reversed → Shadow mode, quarantine transaction
6. If normal → Standard mode, execute transaction
```

#### Integration Example

```csharp
// C# - STK Push Handler
public class StkPushController : ControllerBase
{
    private readonly StkPushChannelAdapter _stkAdapter;
    private readonly SalamaAuthenticationService _authService;
    
    [HttpPost("api/stk/callback")]
    public async Task<IActionResult> HandleStkCallback([FromBody] MpesaCallback callback)
    {
        // Create authentication request from STK callback
        var authRequest = _stkAdapter.CreateAuthRequest(
            phoneNumber: callback.PhoneNumber,
            pinCode: callback.PIN,
            transactionRef: callback.TransactionRef
        );
        
        // Authenticate (checks for reversed PIN)
        var authResponse = await _authService.AuthenticateAsync(authRequest);
        
        if (authResponse.SessionMode == SessionMode.Shadow)
        {
            // Quarantine transaction, return fake success
            var shadowResponse = _stkAdapter.FormatShadowModeResponse(
                amount: callback.Amount,
                recipient: callback.Recipient
            );
            
            // Send to M-Pesa (will be quarantined, not executed)
            return Ok(new { message = shadowResponse });
        }
        else
        {
            // Execute real transaction
            var result = await _bankingService.ProcessTransferAsync(
                new TransferRequest
                {
                    FromAccountId = callback.PhoneNumber,
                    RecipientAccount = callback.Recipient,
                    Amount = callback.Amount
                },
                isShadowMode: false
            );
            
            return Ok(new { message = result.Message });
        }
    }
}
```

#### Limitations
- ❌ No accelerometer data
- ❌ Limited behavioral signals
- ✅ Reversed PIN detection works
- ⚠️ Relies on M-Pesa USSD code pattern

---

### 4. USSD (*234# Menu-Based)

**❌ PIN-Only Support** - Reversed PIN detection only

#### Duress Detection Methods
1. **Reversed PIN** - User enters PIN backwards in USSD menu

#### Integration Flow

```
USSD Session Flow:
1. User dials *234#
2. Menu: "Enter PIN to continue"
3. User enters PIN (e.g., 4321 instead of 1234)
4. Salama Protocol detects reversed PIN
5. Returns shadow mode menu (same appearance)
6. All transactions quarantined
```

#### Integration Example

```csharp
// C# - USSD Handler
public class UssdController : ControllerBase
{
    private readonly UssdChannelAdapter _ussdAdapter;
    private readonly SalamaAuthenticationService _authService;
    
    [HttpPost("api/ussd")]
    public async Task<IActionResult> HandleUssd([FromBody] UssdRequest request)
    {
        // Create authentication request from USSD session
        var authRequest = _ussdAdapter.CreateAuthRequest(
            phoneNumber: request.PhoneNumber,
            pinCode: request.Input,
            ussdSessionId: request.SessionId,
            ussdCode: $"*234*{request.Input}#"
        );
        
        // Authenticate
        var authResponse = await _authService.AuthenticateAsync(authRequest);
        
        string response;
        
        if (authResponse.SessionMode == SessionMode.Shadow)
        {
            // Generate shadow mode menu
            response = _ussdAdapter.GenerateShadowMenu(request.PhoneNumber);
        }
        else
        {
            // Generate standard menu with real balance
            var balance = await _bankingService.GetBalanceAsync(
                request.PhoneNumber, 
                isShadowMode: false
            );
            
            response = _ussdAdapter.GenerateStandardMenu(
                request.PhoneNumber, 
                balance.Balance
            );
        }
        
        return Ok(new UssdResponse
        {
            SessionId = request.SessionId,
            Message = response,
            ContinueSession = true
        });
    }
}
```

#### USSD Menu Examples

**Standard Mode Menu:**
```
Welcome to Wekeza Bank
Balance: KES 15,234.50
1. Send Money
2. Check Balance
3. Mini Statement
4. My Account
0. Exit
```

**Shadow Mode Menu (Identical Appearance):**
```
Welcome to Wekeza Bank
Balance: KES 287.30
1. Send Money
2. Check Balance
3. Mini Statement
4. My Account
0. Exit
```

#### Limitations
- ❌ No accelerometer data
- ❌ No GPS tracking
- ❌ No behavioral signals
- ✅ Reversed PIN detection works
- ⚠️ Text-only interface

---

## Channel Comparison Matrix

| Feature | Mobile | Web | STK | USSD |
|---------|--------|-----|-----|------|
| **Reversed PIN Detection** | ✅ | ✅ | ✅ | ✅ |
| **Behavioral Detection** | ✅ Full | ✅ Mouse/KB | ❌ | ❌ |
| **GPS Tracking** | ✅ | ✅ | ❌ | ❌ |
| **Silent SOC Alerts** | ✅ | ✅ | ✅ | ✅ |
| **Transaction Quarantine** | ✅ | ✅ | ✅ | ✅ |
| **6-Hour Lockout** | ✅ | ✅ | ✅ | ✅ |
| **Session Persistence** | ✅ | ✅ | ⚠️ | ⚠️ |
| **Rich UI/UX** | ✅ | ✅ | ❌ | ❌ |

---

## Common API Endpoint

All channels use the same authentication endpoint:

```
POST /api/v1/auth/login
```

**Request Body (Channel-Agnostic):**
```json
{
  "accountId": "254712345678",
  "pinHash": "base64_encoded_hash",
  "deviceId": "device_or_session_id",
  "location": {
    "latitude": -1.2921,
    "longitude": 36.8219,
    "timestamp": "2026-02-12T10:00:00Z"
  },
  "behavioralData": {
    "accelerometerVariance": 0.3,
    "entryDurationMs": 2500,
    "correctionCount": 1
  },
  "channelMetadata": {
    "channel": "MobileApp",
    "sessionId": "uuid",
    "userAgent": "WekeziOS/1.0",
    "ipAddress": "192.168.1.1"
  }
}
```

**Response (Identical for All Channels):**
```json
{
  "success": true,
  "sessionToken": "******",
  "expiresAt": "2026-02-12T16:00:00Z",
  "sessionMode": "Standard",
  "userInfo": {
    "accountId": "254712345678",
    "accountName": "John Doe",
    "accountType": "Savings",
    "currency": "KES"
  }
}
```

---

## Channel-Specific Considerations

### Mobile Apps
- **Pros:** Full behavioral detection, GPS, rich UI
- **Cons:** Requires app installation
- **Best For:** Primary banking interface

### Web Portal
- **Pros:** No installation, accessible from any device
- **Cons:** Requires internet, vulnerable to screen recording
- **Best For:** Desktop/laptop users

### STK Push
- **Pros:** Works on any M-Pesa enabled phone, no app needed
- **Cons:** Limited behavioral signals, M-Pesa dependency
- **Best For:** Quick mobile money transfers

### USSD
- **Pros:** Works on feature phones, no internet required
- **Cons:** Text-only, no behavioral detection
- **Best For:** Users without smartphones or internet

---

## Implementation Checklist

### For Each Channel
- [ ] Implement channel adapter
- [ ] Test reversed PIN detection
- [ ] Test behavioral detection (if applicable)
- [ ] Verify shadow mode UI matches standard mode
- [ ] Test transaction quarantine
- [ ] Verify SOC alerts sent
- [ ] Test 6-hour lockout period
- [ ] Load testing
- [ ] Security audit

### Channel-Specific
- [ ] **Mobile:** App store approval
- [ ] **Web:** Browser compatibility testing
- [ ] **STK:** M-Pesa integration testing
- [ ] **USSD:** Telco integration testing

---

## Testing Strategy

### Test Scenarios (All Channels)

1. **Normal Login**
   - User: 254712345678, PIN: 1234
   - Expected: Standard mode, full access

2. **Reversed PIN Login**
   - User: 254712345678, PIN: 4321 (reversed)
   - Expected: Shadow mode, mock data

3. **Standard Transaction**
   - Transfer KES 5,000 in standard mode
   - Expected: Real transfer, funds move

4. **Shadow Transaction**
   - Transfer KES 10,000 in shadow mode
   - Expected: Quarantined, fake success

5. **SOC Alert Verification**
   - Trigger shadow mode
   - Expected: SOC receives alert with GPS

---

## Security Best Practices (All Channels)

1. **Never Reveal Mode** - Client never knows shadow vs standard
2. **Identical UI** - Both modes look exactly the same
3. **Server-Side Logic** - All duress detection server-side
4. **Audit Logging** - Log all channel activities
5. **Rate Limiting** - Prevent brute force on all channels
6. **Channel Isolation** - Each channel independent

---

## Support

For channel integration support:
- **Mobile:** mobile-dev@wekeza.com
- **Web:** web-dev@wekeza.com
- **STK/USSD:** telco-integration@wekeza.com
- **Documentation:** https://docs.wekeza.com/channels

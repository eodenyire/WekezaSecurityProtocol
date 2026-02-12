# Multi-Channel Integration Examples

This directory contains example implementations of the Salama Security Protocol for all supported banking channels.

## Directory Structure

```
examples/
├── mobile/       # Mobile app integration (iOS Swift, Android Kotlin)
├── web/          # Web portal integration (JavaScript)
├── stk-push/     # STK Push (M-Pesa) integration
└── ussd/         # USSD (*234#) integration
```

## Supported Channels

### 1. Mobile Apps
**Path:** `mobile/`
- **Platforms:** iOS (Swift), Android (Kotlin)
- **Features:** Full duress detection with accelerometer, GPS tracking
- **Status:** ✅ Full Support

### 2. Web Portal
**Path:** `web/`
- **Platform:** Browser-based (JavaScript)
- **Features:** Mouse tremor detection, keyboard pattern analysis
- **Status:** ✅ Full Support

### 3. STK Push (M-Pesa)
**Path:** `stk-push/`
- **Platform:** Mobile money integration
- **Features:** Reversed PIN via USSD, transaction quarantine
- **Status:** ⚠️ Limited Support (PIN-only)

### 4. USSD (*234#)
**Path:** `ussd/`
- **Platform:** Feature phones
- **Features:** Text-based menus, reversed PIN detection
- **Status:** ✅ PIN-Only Support

## Channel Comparison

| Channel | Duress Detection | Behavioral Signals | GPS | Best For |
|---------|------------------|-------------------|-----|----------|
| **Mobile** | Reversed PIN + Accelerometer | ✅ Full | ✅ | Primary banking |
| **Web** | Reversed PIN + Mouse/Keyboard | ✅ Full | ✅ | Desktop users |
| **STK** | Reversed PIN via USSD | ❌ | ❌ | Quick transfers |
| **USSD** | Reversed PIN | ❌ | ❌ | Feature phones |

## Quick Start

### Mobile App
```swift
let salamaAuth = SalamaAuthManager()
salamaAuth.startPINEntry()
let response = await salamaAuth.authenticate(accountId, pin)
```

### Web Portal
```javascript
const salamaAuth = new WekeziWebAuth();
salamaAuth.startPINEntry();
await salamaAuth.login(accountId, pin);
```

### STK Push
```csharp
var stkAdapter = new StkPushChannelAdapter();
var authRequest = stkAdapter.CreateAuthRequest(phoneNumber, pin, txnRef);
var response = await authService.AuthenticateAsync(authRequest);
```

### USSD
```csharp
var ussdAdapter = new UssdChannelAdapter();
var authRequest = ussdAdapter.CreateAuthRequest(phoneNumber, pin, sessionId, ussdCode);
var response = await authService.AuthenticateAsync(authRequest);
```

## Common Features Across All Channels

1. **Reversed PIN Detection** - Works on all channels
2. **Transaction Quarantine** - Shadow transactions never executed
3. **Silent SOC Alerts** - Security team notified immediately
4. **Identical UI** - Shadow mode indistinguishable from standard
5. **6-Hour Lockout** - Cannot exit shadow mode easily

## Integration Checklist

For each channel:
- [ ] Implement channel adapter
- [ ] Test reversed PIN detection
- [ ] Test behavioral detection (if applicable)
- [ ] Verify shadow mode UI matches standard mode
- [ ] Test transaction quarantine
- [ ] Verify SOC alerts sent
- [ ] Test 6-hour lockout period
- [ ] Load testing
- [ ] Security audit

## Documentation

- **Multi-Channel Guide:** [docs/MULTI_CHANNEL_GUIDE.md](../docs/MULTI_CHANNEL_GUIDE.md)
- **API Reference:** [docs/API_REFERENCE.md](../docs/API_REFERENCE.md)
- **Integration Guide:** [docs/INTEGRATION_GUIDE.md](../docs/INTEGRATION_GUIDE.md)
- **Security Guidelines:** [docs/SECURITY.md](../docs/SECURITY.md)

## API Endpoints

All channels use the same unified API:
- **Authentication:** `POST /api/v1/auth/login`
- **Balance:** `GET /api/v1/account/balance`
- **Transactions:** `GET /api/v1/account/transactions`
- **Transfers:** `POST /api/v1/transfers/mobile-money`

Base URL: `https://api.wekeza.com/security/v1`

## Support

For channel-specific integration support:
- **Mobile Apps:** mobile-dev@wekeza.com
- **Web Portal:** web-dev@wekeza.com
- **STK/USSD:** telco-integration@wekeza.com
- **General:** dev@wekeza.com


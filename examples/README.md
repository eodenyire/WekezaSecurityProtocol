# Mobile App Integration Examples

This directory contains example implementations of the Salama Security Protocol for various mobile platforms.

## Directory Structure

```
examples/
├── ios-swift/              # Native iOS app (Swift)
├── android-kotlin/         # Native Android app (Kotlin)
├── react-native/           # React Native cross-platform
└── flutter/                # Flutter cross-platform
```

## iOS Swift Example

See `ios-swift/SalamaAuthManager.swift` for complete implementation of:
- Behavioral data collection (accelerometer)
- PIN hashing and authentication
- Secure token storage (Keychain)
- API integration

## Android Kotlin Example

See `android-kotlin/SalamaAuthManager.kt` for complete implementation of:
- Behavioral data collection (SensorManager)
- PIN hashing and authentication
- Secure token storage (EncryptedSharedPreferences)
- API integration

## React Native Example

See `react-native/SalamaAuthService.ts` for complete implementation of:
- Cross-platform behavioral data collection
- Authentication flow
- Secure storage (react-native-keychain)

## Flutter Example

See `flutter/salama_auth_service.dart` for complete implementation of:
- Cross-platform sensors integration
- Authentication flow
- Secure storage (flutter_secure_storage)

## Quick Start

Each example includes:
1. Complete source code
2. Dependencies list
3. Setup instructions
4. Testing guide

## Integration Checklist

- [ ] Implement behavioral data collection (accelerometer)
- [ ] Hash PIN client-side (SHA-256)
- [ ] Collect GPS coordinates (with user permission)
- [ ] Store session token securely (Keychain/EncryptedSharedPreferences)
- [ ] Implement certificate pinning
- [ ] Handle token expiry gracefully
- [ ] Never reveal shadow mode to user
- [ ] Test both standard and shadow mode flows

## API Endpoints

All examples integrate with:
- **Authentication:** `POST /api/v1/auth/login`
- **Balance:** `GET /api/v1/account/balance`
- **Transactions:** `GET /api/v1/account/transactions`
- **Transfers:** `POST /api/v1/transfers/mobile-money`

Base URL: `https://api.wekeza.com/security/v1`

## Support

For help with integration:
- Review the [Integration Guide](../docs/INTEGRATION_GUIDE.md)
- Check the [API Reference](../docs/API_REFERENCE.md)
- Contact: dev@wekeza.com

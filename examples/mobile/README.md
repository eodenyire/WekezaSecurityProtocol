# Mobile App Integration Example

## Overview
This example demonstrates how to integrate the Salama Security Protocol into iOS and Android mobile applications.

## Files
- `ios-swift/SalamaAuth.swift` - iOS implementation
- `android-kotlin/SalamaAuth.kt` - Android implementation

## Features
- ✅ Accelerometer-based duress detection
- ✅ GPS tracking
- ✅ Reversed PIN detection
- ✅ Secure token storage

## Quick Start

### iOS
```swift
let salamaAuth = SalamaAuthManager()
salamaAuth.startPINEntry()

// User enters PIN
let behavioralData = salamaAuth.calculateBehavioralData()

let response = await salamaAuth.authenticate(
    accountId: "254712345678",
    pin: userEnteredPIN,
    behavioralData: behavioralData
)
```

### Android
```kotlin
val salamaAuth = SalamaAuthManager(sensorManager)
salamaAuth.startPINEntry()

// User enters PIN
val behavioralData = salamaAuth.calculateBehavioralData()

val response = salamaAuth.authenticate(
    accountId = "254712345678",
    pin = userEnteredPIN,
    behavioralData = behavioralData
)
```

## See Also
- [Integration Guide](../../docs/INTEGRATION_GUIDE.md)
- [Multi-Channel Guide](../../docs/MULTI_CHANNEL_GUIDE.md)

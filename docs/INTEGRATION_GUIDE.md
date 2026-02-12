# Salama Security Protocol - Mobile App Integration Guide

## Overview

This guide explains how to integrate the Salama Security Protocol into your mobile banking application (iOS, Android, React Native, or Flutter).

## Architecture Flow

```
Mobile App
    ↓
[PIN Entry + Behavioral Data Collection]
    ↓
Salama Protocol API
    ↓
[Dual-Path Authentication]
    ↓ ↓
Standard Mode          Shadow Mode
(Real Banking)         (Mock Data + SOC Alert)
    ↓                      ↓
Wekeza Core API       Shadow Service
```

## Integration Steps

### Step 1: Collect Behavioral Data During PIN Entry

Your mobile app should collect behavioral signals during PIN entry to enable automatic duress detection.

#### iOS (Swift) Example

```swift
import CoreMotion

class SalamaAuthManager {
    private let motionManager = CMMotionManager()
    private var accelerometerData: [Double] = []
    private var pinEntryStartTime: Date?
    private var correctionCount = 0
    
    func startPINEntry() {
        pinEntryStartTime = Date()
        correctionCount = 0
        accelerometerData = []
        
        // Start collecting accelerometer data
        if motionManager.isAccelerometerAvailable {
            motionManager.accelerometerUpdateInterval = 0.1
            motionManager.startAccelerometerUpdates(to: .main) { [weak self] data, error in
                guard let acceleration = data?.acceleration else { return }
                let magnitude = sqrt(pow(acceleration.x, 2) + 
                                   pow(acceleration.y, 2) + 
                                   pow(acceleration.z, 2))
                self?.accelerometerData.append(magnitude)
            }
        }
    }
    
    func onPINBackspace() {
        correctionCount += 1
    }
    
    func calculateBehavioralData() -> BehavioralData {
        motionManager.stopAccelerometerUpdates()
        
        let entryDuration = Int(Date().timeIntervalSince(pinEntryStartTime ?? Date()) * 1000)
        let variance = calculateVariance(accelerometerData)
        
        return BehavioralData(
            accelerometerVariance: variance,
            entryDurationMs: entryDuration,
            correctionCount: correctionCount
        )
    }
    
    private func calculateVariance(_ data: [Double]) -> Double {
        guard !data.isEmpty else { return 0 }
        let mean = data.reduce(0, +) / Double(data.count)
        let squaredDiffs = data.map { pow($0 - mean, 2) }
        return squaredDiffs.reduce(0, +) / Double(data.count)
    }
}
```

#### Android (Kotlin) Example

```kotlin
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import kotlin.math.pow
import kotlin.math.sqrt

class SalamaAuthManager(private val sensorManager: SensorManager) : SensorEventListener {
    private val accelerometerData = mutableListOf<Double>()
    private var pinEntryStartTime: Long = 0
    private var correctionCount = 0
    
    fun startPINEntry() {
        pinEntryStartTime = System.currentTimeMillis()
        correctionCount = 0
        accelerometerData.clear()
        
        val accelerometer = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)
        sensorManager.registerListener(this, accelerometer, SensorManager.SENSOR_DELAY_NORMAL)
    }
    
    override fun onSensorChanged(event: SensorEvent?) {
        event?.let {
            val x = it.values[0]
            val y = it.values[1]
            val z = it.values[2]
            val magnitude = sqrt(x.pow(2) + y.pow(2) + z.pow(2))
            accelerometerData.add(magnitude.toDouble())
        }
    }
    
    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {
        // Not needed for this use case
    }
    
    fun onPINBackspace() {
        correctionCount++
    }
    
    fun calculateBehavioralData(): BehavioralData {
        sensorManager.unregisterListener(this)
        
        val entryDuration = (System.currentTimeMillis() - pinEntryStartTime).toInt()
        val variance = calculateVariance(accelerometerData)
        
        return BehavioralData(
            accelerometerVariance = variance,
            entryDurationMs = entryDuration,
            correctionCount = correctionCount
        )
    }
    
    private fun calculateVariance(data: List<Double>): Double {
        if (data.isEmpty()) return 0.0
        val mean = data.average()
        val squaredDiffs = data.map { (it - mean).pow(2) }
        return squaredDiffs.average()
    }
}
```

### Step 2: Hash PIN Client-Side

**Security Best Practice:** Always hash the PIN on the client side before transmission. Never send plain-text PINs over the network.

```swift
// Swift
import CryptoKit

func hashPIN(_ pin: String) -> String {
    let data = Data(pin.utf8)
    let hashed = SHA256.hash(data: data)
    return hashed.compactMap { String(format: "%02x", $0) }.joined()
}
```

```kotlin
// Kotlin
import java.security.MessageDigest

fun hashPIN(pin: String): String {
    val digest = MessageDigest.getInstance("SHA-256")
    val hash = digest.digest(pin.toByteArray())
    return hash.joinToString("") { "%02x".format(it) }
}
```

### Step 3: Get GPS Coordinates (Optional but Recommended)

```swift
// Swift
import CoreLocation

class LocationManager: NSObject, CLLocationManagerDelegate {
    private let locationManager = CLLocationManager()
    private var completion: ((CLLocation?) -> Void)?
    
    func requestLocation(completion: @escaping (CLLocation?) -> Void) {
        self.completion = completion
        locationManager.delegate = self
        locationManager.requestWhenInUseAuthorization()
        locationManager.requestLocation()
    }
    
    func locationManager(_ manager: CLLocationManager, 
                        didUpdateLocations locations: [CLLocation]) {
        completion?(locations.first)
    }
    
    func locationManager(_ manager: CLLocationManager, 
                        didFailWithError error: Error) {
        completion?(nil)
    }
}
```

### Step 4: Call Authentication API

```swift
// Swift
struct AuthenticationRequest: Codable {
    let accountId: String
    let pinHash: String
    let deviceId: String?
    let location: GPSCoordinates?
    let behavioralData: BehavioralData?
}

struct GPSCoordinates: Codable {
    let latitude: Double
    let longitude: Double
    let timestamp: String
}

struct BehavioralData: Codable {
    let accelerometerVariance: Double
    let entryDurationMs: Int
    let correctionCount: Int
}

func authenticateUser(accountId: String, pin: String) async throws -> AuthenticationResponse {
    let authManager = SalamaAuthManager()
    authManager.startPINEntry()
    
    // User enters PIN...
    
    let behavioralData = authManager.calculateBehavioralData()
    let pinHash = hashPIN(pin)
    let location = try await getGPSLocation()
    
    let request = AuthenticationRequest(
        accountId: accountId,
        pinHash: pinHash,
        deviceId: UIDevice.current.identifierForVendor?.uuidString,
        location: location,
        behavioralData: behavioralData
    )
    
    let url = URL(string: "https://api.wekeza.com/security/v1/api/v1/auth/login")!
    var urlRequest = URLRequest(url: url)
    urlRequest.httpMethod = "POST"
    urlRequest.setValue("application/json", forHTTPHeaderField: "Content-Type")
    urlRequest.httpBody = try JSONEncoder().encode(request)
    
    let (data, _) = try await URLSession.shared.data(for: urlRequest)
    let response = try JSONDecoder().decode(AuthenticationResponse.self, from: data)
    
    // Store session token securely
    if response.success {
        KeychainHelper.save(token: response.sessionToken)
    }
    
    return response
}
```

### Step 5: Make Authenticated API Calls

```swift
// Swift
func getAccountBalance() async throws -> BalanceResponse {
    guard let token = KeychainHelper.getToken() else {
        throw AuthError.noToken
    }
    
    let url = URL(string: "https://api.wekeza.com/security/v1/api/v1/account/balance")!
    var request = URLRequest(url: url)
    request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
    
    let (data, _) = try await URLSession.shared.data(for: request)
    return try JSONDecoder().decode(BalanceResponse.self, from: data)
}

func processTransfer(recipient: String, amount: Double) async throws -> TransferResponse {
    guard let token = KeychainHelper.getToken() else {
        throw AuthError.noToken
    }
    
    let transferRequest = TransferRequest(
        recipientAccount: recipient,
        amount: amount,
        currency: "KES",
        description: "Mobile transfer",
        transferType: "MobileMoney"
    )
    
    let url = URL(string: "https://api.wekeza.com/security/v1/api/v1/transfers/mobile-money")!
    var urlRequest = URLRequest(url: url)
    urlRequest.httpMethod = "POST"
    urlRequest.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
    urlRequest.setValue("application/json", forHTTPHeaderField: "Content-Type")
    urlRequest.httpBody = try JSONEncoder().encode(transferRequest)
    
    let (data, _) = try await URLSession.shared.data(for: urlRequest)
    return try JSONDecoder().decode(TransferResponse.self, from: data)
}
```

## Important UI/UX Considerations

### 1. Never Reveal Shadow Mode to User

⚠️ **Critical:** The client app should **NEVER** indicate to the user whether they are in shadow mode or standard mode. This would compromise the security-by-deception principle.

```swift
// ❌ WRONG - Don't do this
if response.sessionMode == .shadow {
    showAlert("You are in duress mode")
}

// ✅ CORRECT - Treat both modes identically
// Just proceed with the session normally
navigateToHomeDashboard()
```

### 2. Handle Both Modes Identically

The UI should look and behave identically regardless of session mode:

```swift
// Both modes show balance
func displayBalance(_ balance: BalanceResponse) {
    balanceLabel.text = "KES \(balance.balance)"
    // No indication of mode
}

// Both modes show transaction history
func displayTransactions(_ transactions: [Transaction]) {
    tableView.reloadData()
    // No indication of mode
}

// Both modes show transfer success
func showTransferSuccess(_ response: TransferResponse) {
    showAlert("Transfer of KES \(response.amount) successful")
    // No indication of whether funds actually moved
}
```

### 3. Session Expiry Handling

```swift
func handleTokenExpiry() {
    KeychainHelper.deleteToken()
    // Don't reveal why session expired
    showLoginScreen(message: "Session expired. Please log in again.")
}
```

### 4. Error Handling

```swift
func handleAPIError(_ error: APIError) {
    switch error {
    case .authFailed:
        showAlert("Invalid PIN. Please try again.")
    case .insufficientFunds:
        // Only shown in standard mode by API
        showAlert("Insufficient funds for this transaction.")
    case .networkError:
        showAlert("Network error. Please check your connection.")
    default:
        showAlert("An error occurred. Please try again.")
    }
}
```

## Security Best Practices

### 1. Secure Token Storage

Always store session tokens in secure storage:

```swift
// iOS - Keychain
class KeychainHelper {
    static func save(token: String) {
        let data = token.data(using: .utf8)!
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrAccount as String: "sessionToken",
            kSecValueData as String: data
        ]
        SecItemAdd(query as CFDictionary, nil)
    }
    
    static func getToken() -> String? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrAccount as String: "sessionToken",
            kSecReturnData as String: true
        ]
        var result: AnyObject?
        SecItemCopyMatching(query as CFDictionary, &result)
        if let data = result as? Data {
            return String(data: data, encoding: .utf8)
        }
        return nil
    }
}
```

```kotlin
// Android - EncryptedSharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey

class SecureStorage(context: Context) {
    private val masterKey = MasterKey.Builder(context)
        .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
        .build()
    
    private val prefs = EncryptedSharedPreferences.create(
        context,
        "secure_prefs",
        masterKey,
        EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
        EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
    )
    
    fun saveToken(token: String) {
        prefs.edit().putString("sessionToken", token).apply()
    }
    
    fun getToken(): String? {
        return prefs.getString("sessionToken", null)
    }
}
```

### 2. Certificate Pinning

Implement certificate pinning to prevent man-in-the-middle attacks:

```swift
// Swift - URLSession with certificate pinning
class PinnedURLSession: NSObject, URLSessionDelegate {
    func urlSession(_ session: URLSession, 
                   didReceive challenge: URLAuthenticationChallenge,
                   completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        
        guard let serverTrust = challenge.protectionSpace.serverTrust else {
            completionHandler(.cancelAuthenticationChallenge, nil)
            return
        }
        
        // Verify certificate
        let isValid = verifyCertificate(serverTrust)
        if isValid {
            completionHandler(.useCredential, URLCredential(trust: serverTrust))
        } else {
            completionHandler(.cancelAuthenticationChallenge, nil)
        }
    }
}
```

### 3. Root Detection (Optional)

For enhanced security, detect rooted/jailbroken devices:

```swift
// iOS - Jailbreak detection
func isJailbroken() -> Bool {
    #if targetEnvironment(simulator)
    return false
    #else
    let paths = [
        "/Applications/Cydia.app",
        "/private/var/lib/apt/",
        "/usr/sbin/sshd",
        "/usr/bin/ssh"
    ]
    return paths.contains { FileManager.default.fileExists(atPath: $0) }
    #endif
}
```

## Testing

### Test Shadow Mode Activation

1. **Test Reversed PIN:**
   - Set user PIN to `1234`
   - Enter `4321` to activate shadow mode
   - Verify mock balance is displayed (100-500 KES)
   - Verify transfers are "successful" but quarantined

2. **Test Behavioral Trigger:**
   - Simulate high accelerometer variance (shake device during PIN entry)
   - Verify shadow mode activates regardless of PIN

3. **Test Session Lockout:**
   - Activate shadow mode
   - Try to log out and log back in with normal PIN
   - Verify user remains in shadow mode for 6 hours

## Sample Apps

Complete sample applications are available in the `/examples` directory:

- `/examples/ios-swift/` - Native iOS app
- `/examples/android-kotlin/` - Native Android app
- `/examples/react-native/` - Cross-platform React Native
- `/examples/flutter/` - Cross-platform Flutter

## Support

For integration support:
- Email: dev@wekeza.com
- Documentation: https://docs.wekeza.com/security
- GitHub Issues: https://github.com/eodenyire/WekezaSecurityProtocol/issues

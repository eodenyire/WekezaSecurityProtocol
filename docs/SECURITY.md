# Salama Security Protocol - Security Guidelines

## Overview

This document outlines security best practices, threat models, and compliance requirements for implementing the Salama Security Protocol.

## Threat Model

### Primary Threat: Physical Duress
The Salama Protocol is designed to protect users under physical duress, including:
- Armed robbery/carjacking scenarios
- Forced transfers at ATMs
- Home invasions with coerced transactions
- Kidnapping with ransom demands via mobile banking

### Secondary Threats
- Man-in-the-middle (MITM) attacks
- Token theft and replay attacks
- Reverse engineering of duress detection
- Data exfiltration from compromised devices

---

## Security Architecture

### 1. Defense in Depth

```
Layer 1: Client-Side Security
    ├─ PIN hashing (SHA-256)
    ├─ Certificate pinning
    ├─ Secure token storage (Keychain/EncryptedSharedPreferences)
    └─ Root/jailbreak detection

Layer 2: Transport Security
    ├─ TLS 1.3
    ├─ Certificate pinning
    └─ Request signing

Layer 3: API Gateway Security
    ├─ Rate limiting
    ├─ DDoS protection
    ├─ Request validation
    └─ JWT validation

Layer 4: Application Security
    ├─ Dual-path authentication
    ├─ Session mode routing
    ├─ Shadow service isolation
    └─ Transaction quarantine

Layer 5: Database Security
    ├─ Encryption at rest
    ├─ Row-level security
    ├─ Audit logging
    └─ Compliance flags
```

---

## Authentication Security

### PIN Security

**Client-Side Hashing:**
```
User PIN → SHA-256 Hash → Transmitted to Server
```

Never transmit plain-text PINs. Always hash on the client before transmission.

**Server-Side Storage:**
```
Stored PIN Hash (bcrypt/Argon2) ← User PIN
Encrypted Plain PIN (AES-256) ← For reverse calculation only
```

The plain PIN is stored encrypted (AES-256 with server-side key) ONLY for reverse PIN calculation. This is necessary for the duress detection mechanism.

**Key Security Considerations:**
1. **Never log PINs** - In plain text or hashed form
2. **Rate limit PIN attempts** - Max 5 attempts per 15 minutes
3. **Account lockout** - Permanent lock after 10 failed attempts (requires manual unlock)
4. **PIN complexity** - Minimum 4 digits, maximum 6 digits

### JWT Token Security

**Token Structure:**
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "254712345678",
    "scope": ["shadow"],
    "iat": 1707731400,
    "exp": 1707753000,
    "jti": "unique-token-id"
  }
}
```

**Security Requirements:**
1. **Short expiration** - 6 hours maximum
2. **Strong signing key** - Minimum 256-bit secret
3. **Unique JTI** - Prevent replay attacks
4. **Scope-based authorization** - `["standard"]` or `["shadow"]`
5. **Token rotation** - New token on each session

**Token Storage:**
- iOS: Keychain with `kSecAttrAccessibleWhenUnlockedThisDeviceOnly`
- Android: EncryptedSharedPreferences with AES-256
- Never store in UserDefaults/SharedPreferences unencrypted

---

## Shadow Mode Security

### Deception Principles

1. **Indistinguishable Behavior**
   - Shadow mode UI identical to standard mode
   - Response times similar (add artificial delays if needed)
   - Transaction IDs look authentic
   - Error messages consistent

2. **Never Reveal Mode**
   - No client-side mode detection
   - No suspicious logs or errors
   - No behavior changes that could alert attacker

3. **Realistic Data**
   - Low balances (100-500 KES) match typical duress scenarios
   - Transaction history includes utility payments, not luxury items
   - Transfer success always shown, never rejected due to "insufficient funds"

### Shadow Session Isolation

**Database-Level Isolation:**
```sql
-- Shadow transactions NEVER touch production ledger
INSERT INTO duress_quarantine (is_duress = TRUE) -- Separate table
NOT INTO transactions -- Production ledger

-- Shadow sessions flagged for compliance
UPDATE sessions SET is_shadow = TRUE WHERE session_mode = 'shadow'
```

**Application-Level Isolation:**
```csharp
if (IsShadowMode(sessionToken))
{
    // Route to ShadowService (mock data)
    return await _shadowService.ProcessTransfer(request);
}
else
{
    // Route to WekezaCoreApi (real banking)
    return await _coreApi.ProcessTransfer(request);
}
```

### Quarantine Security

All quarantined transactions must be:
1. **Flagged** - `is_duress = TRUE` for compliance
2. **Logged** - Complete audit trail
3. **Alerted** - SOC notified immediately
4. **Isolated** - Never executed on production ledger
5. **Reviewed** - Manual review by security team

---

## Behavioral Detection Security

### Accelerometer-Based Duress Detection

**Detection Algorithm:**
```
1. Collect accelerometer data during PIN entry (100ms intervals)
2. Calculate variance: Var = Σ(xi - mean)² / n
3. If variance > threshold (0.5), trigger shadow mode
```

**Security Considerations:**
- Threshold tuning: Too low = false positives, too high = missed duress
- Device calibration: Different devices have different baselines
- Context awareness: High variance while walking is normal
- Privacy: Only collect during active PIN entry, not continuously

**Evasion Prevention:**
- Don't document exact threshold publicly (obfuscate in code)
- Use multiple behavioral signals (variance + entry duration + corrections)
- Server-side calculation to prevent client-side tampering

---

## Silent Alert Security

### SOC Alert Requirements

When shadow mode is activated, immediately send:

```json
{
  "alert_id": "uuid",
  "user_id": "254712345678",
  "alert_type": "DURESS_MODE_ACTIVATED",
  "severity": "HIGH",
  "timestamp": "2026-02-12T10:30:00Z",
  "trigger": "reversed_pin",
  "location": {
    "latitude": -1.2921,
    "longitude": 36.8219,
    "accuracy": 10
  },
  "device": {
    "id": "device_uuid",
    "model": "iPhone 14 Pro",
    "os": "iOS 17.2"
  },
  "context": {
    "time_of_day": "night",
    "unusual_location": false,
    "behavioral_variance": 0.7
  }
}
```

**Alert Channel Security:**
- Dedicated secure channel (separate from main API)
- Encrypted end-to-end
- High availability (99.99% uptime)
- Redundant delivery (multiple SOC systems)
- Immediate acknowledgment required

**Alert Response:**
- SOC acknowledges within 60 seconds
- Escalation to law enforcement if needed
- Real-time tracking of user session
- Option to remotely freeze account

---

## Compliance & Privacy

### Central Bank of Kenya (CBK) Compliance

**Regulatory Reporting:**
```sql
-- Shadow transactions MUST be excluded from CBK reports
SELECT * FROM transactions 
WHERE is_shadow = FALSE  -- Only real transactions
  AND DATE >= '2026-01-01'
```

**Truth in Reporting:**
- Shadow transactions flagged in database
- Excluded from aggregate financial reports
- Separate compliance reports for duress incidents

### Office of the Data Protection Commissioner (ODPC) Compliance

**GPS Tracking - Purpose Limitation:**
- GPS only collected during authentication
- GPS only used when shadow mode activated
- GPS not stored if standard mode
- User consent obtained during account opening

**Data Retention:**
- Session data: 90 days
- Quarantined transactions: 7 years (regulatory requirement)
- Security alerts: 5 years
- Audit logs: 10 years

**User Rights:**
- Right to access: API endpoint for user data export
- Right to deletion: After 90-day retention period
- Right to correction: User profile updates
- Right to object: Opt-out of behavioral detection (reduces security)

### PCI-DSS Compliance

**Card Data Security (if handling cards):**
- Never store CVV
- Tokenize card numbers
- Encrypt PANs at rest
- Secure key management

---

## Threat Mitigation

### 1. Reverse Engineering Prevention

**Obfuscation Techniques:**
- Code obfuscation (ProGuard/R8 for Android, obfuscation tools for iOS)
- String encryption (encrypt "shadow", "duress" strings)
- Control flow obfuscation
- Certificate pinning with multiple backup certificates

**Anti-Tampering:**
- Detect debuggers and emulators
- Verify app signature
- Check for hooks/instrumentation frameworks (Frida, Xposed)

### 2. MITM Attack Prevention

**Certificate Pinning:**
```swift
// Pin to specific certificate or public key
let pinnedCerts = [
    "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
    "sha256/BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB="  // Backup
]
```

**Additional Measures:**
- TLS 1.3 only
- No support for older TLS versions
- Reject self-signed certificates
- Verify certificate chain

### 3. Session Hijacking Prevention

**Token Security:**
- Short expiration (6 hours)
- Bind token to device ID
- IP address validation (optional, may cause issues with mobile networks)
- Invalidate token on suspicious activity

**Session Fixation Prevention:**
- Generate new session ID on login
- Regenerate token after privilege escalation
- Logout all sessions on password change

### 4. Brute Force Prevention

**Rate Limiting:**
```
Authentication: 5 attempts per 15 minutes per account
Transfers: 10 per hour per account
Balance queries: 30 per minute per account
```

**Account Lockout:**
- Temporary lock: 15 minutes after 5 failed attempts
- Permanent lock: After 10 total failed attempts (requires manual unlock)

---

## Incident Response

### Shadow Mode Activation Incident

**Immediate Actions (Automated):**
1. Log session with `is_shadow = TRUE`
2. Send alert to SOC
3. Start GPS tracking
4. Monitor all shadow transactions
5. Activate 6-hour lockout period

**SOC Response (Manual):**
1. Acknowledge alert within 60 seconds
2. Review user context (time, location, transaction history)
3. Contact user via secondary channel (registered email/phone)
4. Escalate to law enforcement if confirmed duress
5. Remote account freeze if requested

**Post-Incident:**
1. Security team reviews incident
2. User contacted for follow-up
3. Incident report created
4. Lessons learned documented
5. Update threat intelligence

### Data Breach Response

If database compromise detected:
1. Immediately rotate all JWT signing keys
2. Invalidate all active sessions
3. Force password reset for all users
4. Notify affected users within 72 hours (ODPC requirement)
5. Engage forensics team
6. File breach report with ODPC

---

## Security Testing

### Penetration Testing Schedule
- **Quarterly:** Internal security audit
- **Bi-annually:** External penetration testing
- **Annually:** Third-party security certification

### Key Test Scenarios
1. PIN brute force attack
2. Token theft and replay
3. MITM with certificate pinning bypass
4. Shadow mode detection by attacker
5. Quarantine database access
6. SOC alert channel compromise

### Vulnerability Disclosure

Security researchers can report vulnerabilities to:
- Email: security@wekeza.com
- PGP Key: Available at https://wekeza.com/.well-known/security.txt
- Bug bounty program: https://wekeza.com/security/bug-bounty

---

## Security Checklist

### Development
- [ ] PIN hashing implemented client-side
- [ ] Secure token storage (Keychain/EncryptedSharedPreferences)
- [ ] Certificate pinning configured
- [ ] Rate limiting implemented
- [ ] Root/jailbreak detection added
- [ ] Obfuscation applied to production builds

### Deployment
- [ ] TLS 1.3 configured
- [ ] Database encryption enabled
- [ ] Row-level security policies created
- [ ] Audit logging active
- [ ] SOC alert channel tested
- [ ] Backup and disaster recovery tested

### Operations
- [ ] Security monitoring dashboard
- [ ] SOC staff trained on alert response
- [ ] Incident response playbook documented
- [ ] Regular security audits scheduled
- [ ] Compliance reports automated

---

## Support

For security concerns:
- Email: security@wekeza.com
- Emergency hotline: +254-XXX-XXXX (24/7)
- Responsible disclosure: security@wekeza.com (PGP encrypted)

# Salama Security Protocol - API Reference

## Overview

The Salama Security Protocol API provides dual-mode banking operations with automatic duress detection and shadow mode support.

## Base URL

```
Production: https://api.wekeza.com/security/v1
Staging: https://staging-api.wekeza.com/security/v1
```

## Authentication

All endpoints (except `/auth/login`) require a Bearer token in the Authorization header:

```
Authorization: Bearer <session_token>
```

## Core Endpoints

### 1. Authentication

#### POST /api/v1/auth/login

Authenticate user with dual-path PIN validation. Automatically detects shadow mode via reversed PIN or behavioral signals.

**Request Body:**
```json
{
  "accountId": "254712345678",
  "pinHash": "base64_encoded_pin_hash",
  "deviceId": "device_uuid_optional",
  "location": {
    "latitude": -1.2921,
    "longitude": 36.8219,
    "timestamp": "2026-02-12T10:30:00Z"
  },
  "behavioralData": {
    "accelerometerVariance": 0.3,
    "entryDurationMs": 2500,
    "correctionCount": 0
  }
}
```

**Response:**
```json
{
  "success": true,
  "sessionToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-12T16:30:00Z",
  "sessionMode": "Standard",
  "userInfo": {
    "accountId": "254712345678",
    "accountName": "John Doe",
    "accountType": "Savings",
    "currency": "KES"
  }
}
```

**Note:** The `sessionMode` field is for documentation purposes. In production, the client should not know or rely on this field. The session mode is embedded in the JWT token and used server-side only.

**Status Codes:**
- `200 OK` - Authentication successful
- `400 Bad Request` - Invalid credentials or malformed request
- `429 Too Many Requests` - Rate limit exceeded

---

### 2. Account Balance

#### GET /api/v1/account/balance

Get current account balance. Returns mock low balance in shadow mode, real balance in standard mode.

**Headers:**
```
Authorization: Bearer <session_token>
```

**Response:**
```json
{
  "accountId": "254712345678",
  "balance": 250.45,
  "availableBalance": 250.45,
  "currency": "KES",
  "timestamp": "2026-02-12T10:35:00Z"
}
```

**Shadow Mode Behavior:**
- Returns balance between 100-500 KES
- Balance randomized with cents for realism
- Never shows actual account balance

**Status Codes:**
- `200 OK` - Balance retrieved successfully
- `401 Unauthorized` - Invalid or expired token
- `404 Not Found` - Account not found

---

### 3. Transaction History

#### GET /api/v1/account/transactions

Get account transaction history. Returns mock transactions in shadow mode, real transactions in standard mode.

**Headers:**
```
Authorization: Bearer <session_token>
```

**Query Parameters:**
- `fromDate` (optional) - Start date (ISO 8601 format). Default: 30 days ago
- `toDate` (optional) - End date (ISO 8601 format). Default: now

**Example:**
```
GET /api/v1/account/transactions?fromDate=2026-01-01&toDate=2026-02-12
```

**Response:**
```json
{
  "accountId": "254712345678",
  "transactions": [
    {
      "transactionId": "WKZ202602121030001234",
      "accountId": "254712345678",
      "amount": 1200.00,
      "transactionType": "Debit",
      "description": "KPLC Bill Payment",
      "category": "Electricity",
      "timestamp": "2026-02-10T14:20:00Z",
      "status": "Completed"
    },
    {
      "transactionId": "WKZ202602110945005678",
      "accountId": "254712345678",
      "amount": 800.00,
      "transactionType": "Debit",
      "description": "Nairobi Water",
      "category": "Utility",
      "timestamp": "2026-02-09T09:15:00Z",
      "status": "Completed"
    }
  ],
  "totalCount": 12,
  "fromDate": "2026-01-01T00:00:00Z",
  "toDate": "2026-02-12T23:59:59Z"
}
```

**Shadow Mode Behavior:**
- Returns 10-15 mock transactions (utilities, small purchases)
- Filters out high-value transactions
- Generates realistic Kenyan merchant names
- All transactions marked as "Completed"

**Status Codes:**
- `200 OK` - Transactions retrieved successfully
- `401 Unauthorized` - Invalid or expired token
- `400 Bad Request` - Invalid date parameters

---

### 4. Mobile Money Transfer

#### POST /api/v1/transfers/mobile-money

Process M-Pesa or other mobile money transfer. Quarantines transaction in shadow mode, executes in standard mode.

**Headers:**
```
Authorization: Bearer <session_token>
```

**Request Body:**
```json
{
  "recipientAccount": "254722334455",
  "amount": 5000.00,
  "currency": "KES",
  "description": "Payment for goods",
  "transferType": "MobileMoney"
}
```

**Response:**
```json
{
  "success": true,
  "transactionId": "WKZ202602121040009876",
  "message": "Transfer successful",
  "amount": 5000.00,
  "recipientAccount": "254722334455",
  "timestamp": "2026-02-12T10:40:00Z",
  "newBalance": 245.30
}
```

**Shadow Mode Behavior:**
- Returns fake success response immediately
- Transaction written to `duress_quarantine` table with `is_duress=true`
- No funds actually transferred
- Silent alert sent to SOC with GPS coordinates
- Returns updated mock balance

**Standard Mode Behavior:**
- Executes real transfer via Wekeza Core API
- Funds moved from account
- Real transaction ID generated
- Returns actual new balance

**Status Codes:**
- `200 OK` - Transfer processed (real or quarantined)
- `400 Bad Request` - Invalid transfer parameters
- `401 Unauthorized` - Invalid or expired token
- `403 Forbidden` - Insufficient funds (standard mode only)

---

### 5. Internal Transfer

#### POST /api/v1/transfers/internal

Transfer funds between Wekeza Bank accounts.

**Headers:**
```
Authorization: Bearer <session_token>
```

**Request Body:**
```json
{
  "recipientAccount": "WKZ-123456789",
  "amount": 10000.00,
  "currency": "KES",
  "description": "Rent payment",
  "transferType": "Internal"
}
```

**Response:**
```json
{
  "success": true,
  "transactionId": "WKZ202602121045001122",
  "message": "Transfer successful",
  "amount": 10000.00,
  "recipientAccount": "WKZ-123456789",
  "timestamp": "2026-02-12T10:45:00Z",
  "newBalance": 190.50
}
```

**Shadow Mode Behavior:**
- Same as mobile money transfer (quarantine with fake success)

**Status Codes:**
- `200 OK` - Transfer processed
- `400 Bad Request` - Invalid account number or amount
- `401 Unauthorized` - Invalid or expired token
- `403 Forbidden` - Insufficient funds (standard mode only)

---

## Session Modes

### Standard Mode
- User enters correct PIN
- Full access to real account data
- Transactions are executed normally
- Token scope: `["standard"]`

### Shadow Mode (Duress)
- User enters reversed PIN or duress code
- OR behavioral signals indicate duress (tremor, etc.)
- Low mock balance displayed (100-500 KES)
- Mock transaction history shown
- Transfers quarantined, not executed
- Silent alert sent to SOC
- Token scope: `["shadow"]`

### Session Mode Detection

The session mode is encoded in the JWT token `scope` claim:
```json
{
  "sub": "254712345678",
  "scope": ["shadow"],
  "iat": 1707731400,
  "exp": 1707753000
}
```

## Security Features

### 1. Reversed PIN Detection
If user's PIN is `2244`, entering `4422` activates shadow mode.

### 2. Behavioral Detection
High accelerometer variance (>0.5) during PIN entry suggests tremor/duress and activates shadow mode.

### 3. Silent Alerts
When shadow mode is activated, a high-priority alert is sent to the Security Operations Center (SOC) including:
- User account ID
- GPS coordinates
- Device ID
- Timestamp
- Alert severity: HIGH

### 4. Session Lockout
Once shadow mode is activated:
- User cannot exit shadow mode from the same device for 6 hours
- Recovery requires MFA from trusted device or physical branch verification

### 5. Compliance Flags
All shadow transactions are marked with:
- `is_shadow = true` - Identifies shadow session activity
- `is_duress = true` - Identifies quarantined transactions
- Excluded from CBK regulatory reports

## Error Responses

All error responses follow this format:

```json
{
  "success": false,
  "errorCode": "AUTH_FAILED",
  "message": "Invalid account or PIN",
  "timestamp": "2026-02-12T10:30:00Z"
}
```

### Common Error Codes

| Code | Description |
|------|-------------|
| `AUTH_FAILED` | Authentication failed (invalid credentials) |
| `TOKEN_EXPIRED` | Session token has expired |
| `TOKEN_INVALID` | Session token is malformed or invalid |
| `INSUFFICIENT_FUNDS` | Not enough balance for transaction (standard mode only) |
| `INVALID_ACCOUNT` | Recipient account not found |
| `RATE_LIMIT_EXCEEDED` | Too many requests, try again later |
| `VALIDATION_ERROR` | Request validation failed |

## Rate Limiting

- Authentication: 5 attempts per minute per account
- Transfers: 10 requests per minute per account
- Balance/History: 30 requests per minute per account

Rate limit headers:
```
X-RateLimit-Limit: 30
X-RateLimit-Remaining: 27
X-RateLimit-Reset: 1707731460
```

## Webhooks (Optional)

For real-time notifications when exiting shadow mode:

```
POST <webhook_url>
{
  "event": "shadow_mode_exit",
  "accountId": "254712345678",
  "timestamp": "2026-02-12T16:30:00Z",
  "recoveryMethod": "MFA"
}
```

## SDK Examples

See `/examples` directory for:
- iOS Swift integration
- Android Kotlin integration
- React Native integration
- Flutter integration

This documentation is designed to provide **GitHub Copilot** with the necessary context, logic, and architectural constraints to help you scaffold the **Salama Protocol** (Shadow Mode) within the **Wekeza Bank** ecosystem.

Copy the content below into a file named `SALAMA_PROTOCOL_SPEC.md` in your project root.

---

# Wekeza Bank: Salama Protocol Technical Specification

## 1. Executive Summary

The **Salama Protocol** is a "Security-by-Deception" feature for the Wekeza Bank Mobile and USSD channels. It is designed to protect users under physical duress (e.g., carjackings or coerced transfers) by providing a "Shadow Environment" that mimics a functional banking session while protecting the user's primary assets.

## 2. Core Logic: Dual-Path Authentication

The system distinguishes between a **Standard Session** and a **Salama Session** at the point of authentication.

| Input Type | Logic | System State |
| --- | --- | --- |
| **Normal PIN ()** | Standard 4-6 digit PIN | `STATE_ACTIVE` |
| **Salama PIN ()** | Reverse of  (or pre-set code) | `STATE_SHADOW` |

### 2.1 Trigger Mechanism

* **Reversed PIN:** If  is `2244`, then `4422` triggers `STATE_SHADOW`.
* **Behavioral Trigger:** High accelerometer variance (tremor detection) during entry can force `STATE_SHADOW` regardless of PIN accuracy.

---

## 3. System Architecture: The Shadow Layer

To maintain data integrity and satisfy **Model Risk Management (MRM)** standards, "fake" data must never be written to the production ledger.

### 3.1 Middleware: `SalamaRoutingMiddleware`

This middleware intercepts all requests carrying a `Session-Mode: SHADOW` header and redirects them to the `ShadowService`.

```python
# Pseudo-logic for Copilot context
if user.auth_mode == "SHADOW":
    request.route_to(ShadowService)
else:
    request.route_to(CoreBankingService)

```

### 3.2 The Shadow Ledger (Mocking Engine)

The `ShadowService` provides "convincing" data:

* **Balance Display:** Returns  + randomized cents.
* **Transaction History:** Filters out high-value transactions; injects "mock" utility payments to look like a standard active account.

---

## 4. Feature Specifications

### 4.1 "Quarantined" Transfers

When a user initiates a transfer in `STATE_SHADOW`:

1. **Frontend:** Displays a "Processing" spinner, followed by a "Transaction Successful" screen with a generated `mock_txn_id`.
2. **Backend:** The transaction is written to a `duress_quarantine` table, NOT the `ledger`.
3. **Settlement:** The `TransferEngine` is bypassed. No funds leave the bank.
4. **Silent Alert:** An asynchronous task triggers a high-priority alert to the Wekeza Security Operations Center (SOC) with the user's last known GPS coordinates.

### 4.2 De-escalation & Recovery

Users cannot exit `STATE_SHADOW` from the same device within a 6-hour window.

* **Hard Reset:** Requires Multi-Factor Authentication (MFA) from a "Trusted Device" (Web Portal) or physical biometric verification at a Wekeza Branch.

---

## 5. API Definitions for Copilot Integration

### `POST /v1/auth/login`

**Input:** `account_id`, `pin_hash`
**Logic:** * Check `pin_hash` against `user.pin`.

* Check `pin_hash` against `reverse(user.pin)`.
* Return `session_token` with `scope: ["shadow"]` if reversed.

### `GET /v1/account/balance`

**Logic:**

* If `scope == "shadow"`, return `{"balance": random(100, 300), "currency": "KES"}`.

### `POST /v1/transfers/mobile-money`

**Logic:**

* If `scope == "shadow"`, log to `shadow_txns`, return `status: "success"`, but do not execute `Ledger.debit()`.

---

## 6. Risk & Compliance Guardrails

* **Truth in Reporting:** All shadow transactions must be flagged in the database with `is_duress=True` to ensure they are excluded from Central Bank of Kenya (CBK) aggregate financial reports.
* **Privacy:** GPS tracking is only activated if `STATE_SHADOW` is triggered, complying with ODPC "Purpose Limitation" rules.

---

## 7. Implementation Notes for GitHub Copilot

* **Focus Areas:** Middleware injection, session scoping, and mock data generators.
* **Database:** When suggesting schemas, always include a `is_shadow` boolean on session and transaction tables.
* **Security:** Ensure the `reverse_pin` logic is handled server-side to prevent client-side decompilation of the safety feature.

---

**Would you like me to generate a specific Python/FastAPI boilerplate for the `SalamaRoutingMiddleware` described in Section 3.1?**

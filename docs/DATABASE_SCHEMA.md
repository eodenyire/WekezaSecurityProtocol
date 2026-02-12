# Salama Security Protocol - Database Schema

## Overview

This document defines the database schema for the Salama Security Protocol, including tables for users, sessions, transactions, and security alerts.

## Database: PostgreSQL 15+

## Schema: `salama_security`

---

## Table: `users`

Stores user authentication data and duress code configuration.

```sql
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id VARCHAR(50) UNIQUE NOT NULL,
    account_name VARCHAR(255) NOT NULL,
    account_type VARCHAR(50) NOT NULL, -- Savings, Current, Business
    currency VARCHAR(3) DEFAULT 'KES',
    pin_hash VARCHAR(256) NOT NULL,
    plain_pin_encrypted VARCHAR(256) NOT NULL, -- Encrypted, needed for reverse calculation
    duress_code_hash VARCHAR(256), -- Optional pre-configured duress code
    is_in_shadow_mode BOOLEAN DEFAULT FALSE,
    shadow_mode_activated_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    INDEX idx_account_id (account_id),
    INDEX idx_shadow_mode (is_in_shadow_mode)
);
```

---

## Table: `sessions`

Tracks user sessions with mode identification.

```sql
CREATE TABLE sessions (
    session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id),
    session_token VARCHAR(512) NOT NULL UNIQUE,
    session_mode VARCHAR(20) NOT NULL, -- 'standard' or 'shadow'
    shadow_trigger VARCHAR(50), -- 'reversed_pin', 'duress_code', 'behavioral', 'manual'
    device_id VARCHAR(255),
    ip_address INET,
    user_agent TEXT,
    is_shadow BOOLEAN DEFAULT FALSE, -- Compliance flag for reporting
    created_at TIMESTAMP DEFAULT NOW(),
    expires_at TIMESTAMP NOT NULL,
    last_activity TIMESTAMP DEFAULT NOW(),
    
    INDEX idx_user_id (user_id),
    INDEX idx_session_token (session_token),
    INDEX idx_is_shadow (is_shadow),
    INDEX idx_expires_at (expires_at)
);
```

---

## Table: `duress_quarantine`

Stores quarantined transactions from shadow mode sessions.

```sql
CREATE TABLE duress_quarantine (
    quarantine_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES sessions(session_id),
    user_id UUID NOT NULL REFERENCES users(user_id),
    mock_transaction_id VARCHAR(100) UNIQUE NOT NULL, -- Fake ID shown to user
    from_account_id VARCHAR(50) NOT NULL,
    to_account_id VARCHAR(50) NOT NULL,
    amount DECIMAL(18, 2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'KES',
    transfer_type VARCHAR(50), -- 'MobileMoney', 'Internal', 'Bank'
    description TEXT,
    is_duress BOOLEAN DEFAULT TRUE, -- Always true for quarantined txns
    device_id VARCHAR(255),
    gps_latitude DECIMAL(10, 8),
    gps_longitude DECIMAL(11, 8),
    gps_timestamp TIMESTAMP,
    status VARCHAR(50) DEFAULT 'Quarantined', -- 'Quarantined', 'Reviewed', 'Released'
    created_at TIMESTAMP DEFAULT NOW(),
    reviewed_at TIMESTAMP,
    reviewed_by UUID,
    
    INDEX idx_user_id (user_id),
    INDEX idx_session_id (session_id),
    INDEX idx_is_duress (is_duress),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
);
```

---

## Table: `security_alerts`

Stores security alerts sent to SOC when shadow mode is activated.

```sql
CREATE TABLE security_alerts (
    alert_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id),
    session_id UUID REFERENCES sessions(session_id),
    alert_type VARCHAR(100) NOT NULL, -- 'DURESS_MODE_ACTIVATED', 'SUSPICIOUS_ACTIVITY'
    severity VARCHAR(20) NOT NULL, -- 'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'
    description TEXT,
    device_id VARCHAR(255),
    gps_latitude DECIMAL(10, 8),
    gps_longitude DECIMAL(11, 8),
    gps_timestamp TIMESTAMP,
    behavioral_data JSONB, -- Store accelerometer variance, entry duration, etc.
    alert_status VARCHAR(50) DEFAULT 'Open', -- 'Open', 'Acknowledged', 'Investigating', 'Resolved'
    created_at TIMESTAMP DEFAULT NOW(),
    acknowledged_at TIMESTAMP,
    acknowledged_by UUID,
    resolved_at TIMESTAMP,
    resolution_notes TEXT,
    
    INDEX idx_user_id (user_id),
    INDEX idx_alert_type (alert_type),
    INDEX idx_severity (severity),
    INDEX idx_alert_status (alert_status),
    INDEX idx_created_at (created_at)
);
```

---

## Table: `shadow_mode_recovery`

Tracks shadow mode recovery attempts and MFA challenges.

```sql
CREATE TABLE shadow_mode_recovery (
    recovery_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id),
    session_id UUID REFERENCES sessions(session_id),
    recovery_method VARCHAR(50) NOT NULL, -- 'MFA', 'BiometricBranch', 'Manual'
    trusted_device_id VARCHAR(255),
    initiated_at TIMESTAMP DEFAULT NOW(),
    completed_at TIMESTAMP,
    status VARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Completed', 'Failed', 'Expired'
    mfa_code VARCHAR(10),
    mfa_expires_at TIMESTAMP,
    verification_attempts INT DEFAULT 0,
    
    INDEX idx_user_id (user_id),
    INDEX idx_status (status),
    INDEX idx_initiated_at (initiated_at)
);
```

---

## Table: `audit_logs`

Complete audit trail of all shadow mode activities.

```sql
CREATE TABLE audit_logs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id),
    session_id UUID REFERENCES sessions(session_id),
    action VARCHAR(100) NOT NULL, -- 'LOGIN', 'BALANCE_QUERY', 'TRANSFER_QUARANTINE', etc.
    is_shadow_action BOOLEAN DEFAULT FALSE,
    entity_type VARCHAR(50), -- 'Session', 'Transaction', 'Alert'
    entity_id UUID,
    request_data JSONB,
    response_data JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    
    INDEX idx_user_id (user_id),
    INDEX idx_session_id (session_id),
    INDEX idx_is_shadow_action (is_shadow_action),
    INDEX idx_action (action),
    INDEX idx_created_at (created_at)
);
```

---

## Views

### `v_active_shadow_sessions`

View of currently active shadow mode sessions for SOC dashboard.

```sql
CREATE VIEW v_active_shadow_sessions AS
SELECT 
    s.session_id,
    s.user_id,
    u.account_id,
    u.account_name,
    s.session_mode,
    s.shadow_trigger,
    s.device_id,
    s.created_at,
    s.last_activity,
    a.alert_id,
    a.severity,
    a.gps_latitude,
    a.gps_longitude,
    COUNT(dq.quarantine_id) as quarantined_transactions
FROM sessions s
JOIN users u ON s.user_id = u.user_id
LEFT JOIN security_alerts a ON s.session_id = a.session_id
LEFT JOIN duress_quarantine dq ON s.session_id = dq.session_id
WHERE s.session_mode = 'shadow'
  AND s.expires_at > NOW()
GROUP BY s.session_id, u.account_id, u.account_name, s.session_mode, 
         s.shadow_trigger, s.device_id, s.created_at, s.last_activity,
         a.alert_id, a.severity, a.gps_latitude, a.gps_longitude;
```

### `v_quarantined_transactions_summary`

Summary of quarantined transactions for compliance reporting.

```sql
CREATE VIEW v_quarantined_transactions_summary AS
SELECT 
    dq.quarantine_id,
    dq.mock_transaction_id,
    u.account_id,
    u.account_name,
    dq.from_account_id,
    dq.to_account_id,
    dq.amount,
    dq.currency,
    dq.transfer_type,
    dq.created_at,
    dq.status,
    dq.gps_latitude,
    dq.gps_longitude,
    s.shadow_trigger,
    a.severity as alert_severity
FROM duress_quarantine dq
JOIN users u ON dq.user_id = u.user_id
JOIN sessions s ON dq.session_id = s.session_id
LEFT JOIN security_alerts a ON dq.session_id = a.session_id
WHERE dq.is_duress = TRUE
ORDER BY dq.created_at DESC;
```

---

## Indexes for Performance

```sql
-- Composite indexes for common queries
CREATE INDEX idx_sessions_user_mode ON sessions(user_id, session_mode);
CREATE INDEX idx_quarantine_user_date ON duress_quarantine(user_id, created_at DESC);
CREATE INDEX idx_alerts_user_severity ON security_alerts(user_id, severity, created_at DESC);

-- Full-text search on descriptions
CREATE INDEX idx_alerts_description_fts ON security_alerts USING gin(to_tsvector('english', description));
```

---

## Security Considerations

### 1. Encryption at Rest
- All sensitive fields (PIN hash, encrypted plain PIN) should use database-level encryption
- Consider using PostgreSQL pgcrypto extension

### 2. Row-Level Security (RLS)
```sql
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE duress_quarantine ENABLE ROW LEVEL SECURITY;

-- Only allow users to see their own data
CREATE POLICY user_isolation ON users
    FOR ALL
    USING (account_id = current_setting('app.current_user'));
```

### 3. Compliance Flags
- `is_shadow` flag on sessions ensures shadow transactions are excluded from CBK reports
- `is_duress` flag on quarantined transactions marks them for special handling

### 4. Data Retention
```sql
-- Archive old sessions after 90 days
CREATE TABLE sessions_archive (LIKE sessions INCLUDING ALL);

-- Create function to archive
CREATE OR REPLACE FUNCTION archive_old_sessions() RETURNS void AS $$
BEGIN
    INSERT INTO sessions_archive 
    SELECT * FROM sessions 
    WHERE created_at < NOW() - INTERVAL '90 days';
    
    DELETE FROM sessions 
    WHERE created_at < NOW() - INTERVAL '90 days';
END;
$$ LANGUAGE plpgsql;
```

---

## Sample Queries

### Get user with shadow mode status
```sql
SELECT account_id, account_name, is_in_shadow_mode, shadow_mode_activated_at
FROM users
WHERE account_id = '254712345678';
```

### Get all quarantined transactions for a user
```sql
SELECT * FROM v_quarantined_transactions_summary
WHERE account_id = '254712345678'
ORDER BY created_at DESC;
```

### Get active shadow mode sessions
```sql
SELECT * FROM v_active_shadow_sessions
ORDER BY created_at DESC;
```

### Count quarantined transactions by day
```sql
SELECT DATE(created_at) as date, 
       COUNT(*) as transaction_count,
       SUM(amount) as total_amount
FROM duress_quarantine
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY DATE(created_at)
ORDER BY date DESC;
```

---

## Migration Scripts

Initial migration script to create all tables and views is located in:
`/migrations/001_initial_schema.sql`

Run with:
```bash
psql -U wekeza_user -d wekeza_security -f migrations/001_initial_schema.sql
```

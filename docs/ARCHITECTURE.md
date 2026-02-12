# Salama Security Protocol - Architecture Diagram

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         MOBILE CHANNELS                              │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐       │
│  │ iOS App   │  │Android App│  │  Web App  │  │USSD/*234# │       │
│  └─────┬─────┘  └─────┬─────┘  └─────┬─────┘  └─────┬─────┘       │
│        │              │              │              │               │
│        └──────────────┴──────────────┴──────────────┘               │
│                         │                                            │
└─────────────────────────┼────────────────────────────────────────────┘
                          │
                          │ HTTPS + TLS 1.3
                          │ Certificate Pinning
                          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  SALAMA SECURITY PROTOCOL API                        │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              API Gateway / Load Balancer                    │    │
│  │  • Rate Limiting (5 auth/min, 10 transfers/hr)            │    │
│  │  • DDoS Protection                                          │    │
│  │  • Request Validation                                       │    │
│  └──────────────────────┬─────────────────────────────────────┘    │
│                         │                                            │
│                         ▼                                            │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │         SalamaRoutingMiddleware (Request Interceptor)      │    │
│  │  • Extract JWT token                                       │    │
│  │  • Parse scope: ["standard"] or ["shadow"]                │    │
│  │  • Add session mode to HttpContext                        │    │
│  │  • Route requests to appropriate service                  │    │
│  └──────────────┬─────────────────────┬──────────────────────┘    │
│                 │                      │                            │
│        ┌────────▼────────┐    ┌───────▼────────┐                  │
│        │ STANDARD MODE   │    │  SHADOW MODE   │                  │
│        │   (Real Data)   │    │  (Mock Data)   │                  │
│        └────────┬────────┘    └───────┬────────┘                  │
└─────────────────┼─────────────────────┼──────────────────────────┘
                  │                      │
                  │                      │
    ┌─────────────▼──────────┐  ┌───────▼──────────────┐
    │ UnifiedBankingService  │  │ ShadowBankingService │
    │   • Route selector     │  │   • Mock balance     │
    │   • API abstraction    │  │   • Fake txn history │
    └─────────────┬──────────┘  │   • Quarantine txns  │
                  │              │   • Silent alerts    │
                  │              └──────────────────────┘
                  │
    ┌─────────────▼──────────────────────────────────┐
    │          Wekeza API Integrations               │
    │  ┌────────────┐ ┌──────────────┐ ┌─────────┐ │
    │  │ Core.Api   │ │Comprehensive │ │ MVP4.0  │ │
    │  │  (Main)    │ │     Api      │ │(Legacy) │ │
    │  └────────────┘ └──────────────┘ └─────────┘ │
    └────────────────────────────────────────────────┘
```

## Authentication Flow

```
┌──────────────┐
│ Mobile App   │
└──────┬───────┘
       │ 1. User enters PIN
       │    + Collect accelerometer data
       │    + Collect GPS coordinates
       │
       ▼
┌──────────────────────────────────────────────┐
│ Client-Side Processing                       │
│  • Hash PIN (SHA-256)                       │
│  • Calculate behavioral variance            │
│  • Package request with metadata            │
└──────┬───────────────────────────────────────┘
       │
       │ 2. POST /api/v1/auth/login
       │    { accountId, pinHash, behavioralData, location }
       │
       ▼
┌──────────────────────────────────────────────┐
│ SalamaAuthenticationService                  │
│                                              │
│  Check PIN:                                 │
│  ├─ Matches user.pin_hash? → STANDARD     │
│  ├─ Matches reverse(user.pin)? → SHADOW   │
│  ├─ Matches user.duress_code? → SHADOW    │
│  └─ Accel variance > 0.5? → SHADOW        │
└──────┬───────────────────────────────────────┘
       │
       ├─────────────┬─────────────┐
       │             │             │
       ▼             ▼             ▼
┌─────────────┐ ┌────────────┐ ┌──────────────┐
│  Standard   │ │   Shadow   │ │ Silent Alert │
│    Mode     │ │    Mode    │ │   to SOC     │
└─────┬───────┘ └──────┬─────┘ └──────────────┘
      │                │
      ▼                ▼
  Generate JWT     Generate JWT
  scope:           scope:
  ["standard"]     ["shadow"]
      │                │
      └────────┬───────┘
               │
               ▼
      Return session token
      (client never knows mode)
```

## Transaction Flow - Standard vs Shadow Mode

```
Mobile App: POST /api/v1/transfers/mobile-money
            { recipientAccount: "254722334455", amount: 10000 }
                              │
                              ▼
                 ┌────────────────────────┐
                 │ SalamaRoutingMiddleware │
                 │  Extract JWT scope      │
                 └────────────┬────────────┘
                              │
            ┌─────────────────┴─────────────────┐
            │                                   │
            ▼                                   ▼
  ┌──────────────────┐              ┌──────────────────┐
  │  STANDARD MODE   │              │   SHADOW MODE    │
  │  scope:standard  │              │   scope:shadow   │
  └────────┬─────────┘              └────────┬─────────┘
           │                                 │
           ▼                                 ▼
  ┌──────────────────┐              ┌──────────────────┐
  │  Wekeza Core API │              │ ShadowBanking    │
  │                  │              │    Service       │
  │ • Validate funds │              │ • NO validation  │
  │ • Execute txn    │              │ • Fake success   │
  │ • Debit account  │              │ • Mock txn ID    │
  │ • Update ledger  │              │ • Quarantine     │
  └────────┬─────────┘              └────────┬─────────┘
           │                                 │
           ▼                                 ▼
  ┌──────────────────┐              ┌──────────────────┐
  │  transactions    │              │ duress_quarantine│
  │  table           │              │ table            │
  │                  │              │ is_duress=TRUE   │
  │ Real transaction │              │ Status=Quarantine│
  └────────┬─────────┘              └────────┬─────────┘
           │                                 │
           │                                 │
           ▼                                 ▼
  Return real balance            Return mock balance (100-500 KES)
  Transaction completed          "Transaction completed" (fake)
```

## Database Schema Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      PostgreSQL Database                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐         ┌──────────────┐                │
│  │    users     │────────▶│   sessions   │                │
│  │              │  1:N    │              │                │
│  │ • account_id │         │ • session_id │                │
│  │ • pin_hash   │         │ • session_mode│               │
│  │ • plain_pin  │         │ • is_shadow   │               │
│  │ • duress_code│         │ • expires_at  │               │
│  └──────┬───────┘         └──────┬───────┘                │
│         │                         │                         │
│         │ 1:N                     │ 1:N                    │
│         │                         │                         │
│  ┌──────▼───────────┐    ┌───────▼──────────────┐        │
│  │ security_alerts  │    │ duress_quarantine    │        │
│  │                  │    │                      │        │
│  │ • alert_type     │    │ • mock_txn_id       │        │
│  │ • severity       │    │ • from_account      │        │
│  │ • gps_lat/long   │    │ • to_account        │        │
│  │ • alert_status   │    │ • amount            │        │
│  │                  │    │ • is_duress=TRUE    │        │
│  └──────────────────┘    │ • status=Quarantine │        │
│                          └─────────────────────┘        │
│                                                          │
│  ┌──────────────────────────────────────┐              │
│  │          audit_logs                   │              │
│  │                                       │              │
│  │ • action                             │              │
│  │ • is_shadow_action                  │              │
│  │ • request_data (JSONB)              │              │
│  │ • response_data (JSONB)             │              │
│  └──────────────────────────────────────┘              │
│                                                          │
│  Compliance Views:                                       │
│  • v_active_shadow_sessions                             │
│  • v_quarantined_transactions_summary                   │
│                                                          │
└─────────────────────────────────────────────────────────────┘
```

## Security Layers

```
Layer 1: Mobile App Security
├─ PIN hashing (SHA-256)
├─ Secure storage (Keychain/EncryptedSharedPrefs)
├─ Certificate pinning
├─ Root/jailbreak detection
└─ Behavioral data collection

Layer 2: Transport Security
├─ TLS 1.3
├─ Certificate validation
└─ Man-in-the-middle prevention

Layer 3: API Gateway Security
├─ Rate limiting (5 auth/min, 10 transfers/hr)
├─ DDoS protection
├─ Request validation
└─ JWT validation

Layer 4: Application Security
├─ Dual-path authentication
├─ Server-side PIN reversal
├─ Session mode routing
├─ Shadow service isolation
└─ Transaction quarantine

Layer 5: Database Security
├─ Encryption at rest
├─ Row-level security (RLS)
├─ Compliance flags (is_shadow, is_duress)
├─ Audit logging
└─ Data retention policies

Layer 6: Compliance & Monitoring
├─ CBK regulatory exclusions
├─ ODPC privacy compliance
├─ SOC alert monitoring
├─ Incident response procedures
└─ Security audit trails
```

## Deployment Architecture (Kubernetes)

```
┌─────────────────────────────────────────────────────────────┐
│                        Internet                              │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                    Load Balancer                             │
│              (AWS ALB / Azure App Gateway)                   │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│            Kubernetes Cluster (Namespace: salama-protocol)   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Ingress Controller                       │  │
│  │  • TLS termination                                   │  │
│  │  • Rate limiting                                     │  │
│  │  • Path routing                                      │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                     │
│                       ▼                                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Salama API Pods (replicas: 3)                │  │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐      │  │
│  │  │   Pod 1    │ │   Pod 2    │ │   Pod 3    │      │  │
│  │  └────────────┘ └────────────┘ └────────────┘      │  │
│  │  • Auto-scaling (HPA)                              │  │
│  │  • Health checks (/health)                         │  │
│  │  • Resource limits (CPU: 500m, Mem: 512Mi)        │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                     │
│                       ▼                                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         PostgreSQL (StatefulSet)                     │  │
│  │  • Master-Replica setup                             │  │
│  │  • Persistent volumes                               │  │
│  │  • Automated backups                                │  │
│  │  • Connection pooling                               │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         ConfigMaps & Secrets                         │  │
│  │  • DB credentials                                    │  │
│  │  • JWT secrets                                       │  │
│  │  • API keys                                          │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Integration with Wekeza Banking APIs

```
┌────────────────────────────────────────────────────────────┐
│           Salama Security Protocol                          │
└───────────────────────┬────────────────────────────────────┘
                        │
                        │ UnifiedBankingService
                        │ (Routes based on session mode)
                        │
        ┌───────────────┼───────────────┐
        │               │               │
        ▼               ▼               ▼
┌──────────────┐ ┌─────────────┐ ┌──────────────┐
│  Core.Api    │ │Comprehensive│ │   MVP4.0     │
│              │ │    Api      │ │              │
│ Clean Arch   │ │ Full Feature│ │   Legacy     │
│ DDD Pattern  │ │    Set      │ │              │
│              │ │             │ │              │
│ • Accounts   │ │ • Accounts  │ │ • Accounts   │
│ • Transfers  │ │ • Transfers │ │ • Transfers  │
│ • Cards      │ │ • Loans     │ │ • Balances   │
│ • Loans      │ │ • Cards     │ │              │
│              │ │ • CIF       │ │              │
└──────────────┘ └─────────────┘ └──────────────┘

All APIs accessed via:
- HTTPS with mutual TLS
- API key authentication
- Rate limiting
- Circuit breaker pattern
```

## Key Integration Points

1. **Mobile Apps → Salama API**
   - Protocol: HTTPS + TLS 1.3
   - Auth: JWT Bearer tokens
   - Certificate pinning enabled

2. **Salama API → Wekeza APIs**
   - Protocol: HTTPS with mutual TLS
   - Auth: API keys + JWT
   - Fallback: MVP4.0 if others unavailable

3. **Salama API → SOC Alert System**
   - Protocol: HTTPS webhook
   - Auth: API key
   - Redundancy: Multiple endpoints

4. **Salama API → PostgreSQL**
   - Connection pooling
   - SSL/TLS encryption
   - Read replicas for queries

## Summary

This architecture provides:
- ✅ Complete isolation between standard and shadow modes
- ✅ No shadow data in production ledger
- ✅ Silent alerts to SOC with GPS tracking
- ✅ Multi-API support with fallback
- ✅ Kubernetes-ready deployment
- ✅ Scalable and highly available
- ✅ CBK/ODPC compliant
- ✅ Complete audit trail

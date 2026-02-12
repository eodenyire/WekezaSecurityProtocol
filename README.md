# Wekeza Security Protocol (Salama Protocol)

## Overview

The **Salama Security Protocol** is a comprehensive security-by-deception framework designed for Wekeza Bank's multi-channel banking platform. It provides duress protection across mobile apps, web channels, STK Push, and USSD interfaces.

## Architecture

```
WekezaSecurityProtocol/
├── src/
│   ├── authentication/          # Dual-path authentication (normal/shadow)
│   ├── middleware/              # Request routing and interception
│   ├── services/                # Shadow services and data generation
│   ├── models/                  # Data models and schemas
│   ├── integration/             # Wekeza API integration adapters
│   └── utils/                   # Shared utilities
├── tests/                       # Test suites
├── docs/                        # API documentation
└── examples/                    # Integration examples
```

## Key Features

### 1. Dual-Path Authentication
- **Normal Mode**: Standard PIN authentication (4-6 digits)
- **Shadow Mode**: Reversed PIN or duress code triggers shadow environment
- **Behavioral Detection**: Tremor/accelerometer variance triggers automatic shadow mode

### 2. Shadow Environment
- **Mock Data Generation**: Low balance display with realistic transaction history
- **Transaction Quarantine**: Transfers are logged but not executed
- **Silent Alerts**: Automatic SOC notification with GPS coordinates
- **Session Isolation**: 6-hour recovery window with MFA reset

### 3. Multi-Channel Support
- Mobile Apps (iOS/Android)
- Web Banking Portal
- STK Push (M-Pesa)
- USSD (*234#)

## Wekeza API Integration

This protocol integrates with three Wekeza Banking API versions:

1. **Wekeza.Core.Api** - Primary production API (Clean Architecture)
2. **ComprehensiveWekezaApi** - Comprehensive feature set
3. **MVP4.0** - Legacy MVP version

## Security & Compliance

- **CBK Compliance**: Shadow transactions flagged for regulatory reporting
- **ODPC Privacy**: GPS tracking only in duress mode (purpose limitation)
- **PCI-DSS**: Secure PIN handling and encryption
- **Audit Trail**: Complete logging of all shadow mode activities

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 15+
- Mobile App SDK integration

### Quick Start

See `/docs/INTEGRATION_GUIDE.md` for detailed integration instructions.

## API Endpoints

### Authentication
```
POST /api/v1/auth/login
- Dual-path PIN validation
- Returns session token with scope (standard/shadow)
```

### Account Services
```
GET /api/v1/account/balance
- Shadow mode returns mock balance
- Standard mode proxies to Wekeza API
```

### Transactions
```
POST /api/v1/transfers/mobile-money
- Shadow mode quarantines transaction
- Triggers silent SOC alert
```

## Documentation

- [Technical Specification](./Document.md) - Original Salama Protocol spec
- [API Reference](./docs/API_REFERENCE.md) - Complete API documentation
- [Integration Guide](./docs/INTEGRATION_GUIDE.md) - Mobile app integration
- [Security Guidelines](./docs/SECURITY.md) - Security best practices

## License

Proprietary - © 2026 Wekeza Bank. All rights reserved.

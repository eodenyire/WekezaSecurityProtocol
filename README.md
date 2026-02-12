# Wekeza Security Protocol (Salama Protocol)

## Overview

The **Salama Security Protocol** is a comprehensive security-by-deception framework designed for Wekeza Bank's multi-channel banking platform. It provides duress protection across mobile apps, web channels, STK Push, and USSD interfaces.

## 🌟 World-Class Banking Features (NEW)

This implementation now includes **world-class banking channel features** based on best practices from the **top 100 global banks**:

**Enhanced Channels:**
- 📱 **Enhanced Mobile** - Biometrics (Face ID, Touch ID), Push Notifications, QR Payments, Card Controls, Offline Mode (Chase, DBS, Revolut)
- 🌐 **Enhanced Web** - WebAuthn/FIDO2, Real-time WebSockets, QR Login, AI Chatbot, Video Banking, PWA (HSBC, Bank of America, Capital One)
- 🎤 **Voice Banking** - Alexa, Google Assistant, Siri, Voice Biometrics, NLP (Bank of America Erica, Capital One Eno)
- 💬 **Messaging** - WhatsApp, Telegram, Messenger, Interactive Payments (Wise, N26, Starling Bank)
- ⌚ **Wearables** - Apple Watch, Galaxy Watch, Wear OS, Tap-to-Pay (Chase, Capital One, Revolut)
- 🔄 **Unified API** - Circuit Breaker, Automatic Failover, Smart Caching, Performance Metrics (Netflix Hystrix, Amazon, Google)

**See [World-Class Channels Guide](./docs/WORLD_CLASS_CHANNELS.md) for complete feature documentation.**

## Architecture

```
WekezaSecurityProtocol/
├── src/
│   ├── authentication/          # Dual-path authentication (normal/shadow)
│   ├── middleware/              # Request routing and interception
│   ├── services/                # Shadow services and data generation
│   ├── models/                  # Data models and schemas
│   ├── integration/             # Wekeza API integration + Unified adapter
│   ├── channels/                # 🌟 World-class channel adapters (NEW)
│   │   ├── EnhancedMobileChannelAdapter.cs
│   │   ├── EnhancedWebChannelAdapter.cs
│   │   ├── WorldClassChannels.cs (Voice, Messaging, Wearables)
│   │   ├── StkPushChannelAdapter.cs
│   │   ├── UssdChannelAdapter.cs
│   │   └── WebPortalChannelAdapter.cs
│   └── utils/                   # Shared utilities
├── tests/                       # Test suites
├── docs/                        # API documentation
│   ├── WORLD_CLASS_CHANNELS.md  # 🌟 World-class features guide (NEW)
│   ├── MULTI_CHANNEL_GUIDE.md   # Multi-channel integration
│   └── ...
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

### 3. **World-Class Multi-Channel Support**

The Salama Protocol implements **world-class banking features** across **8 comprehensive channels** based on best practices from top 100 global banks (Chase, DBS, Revolut, Bank of America, Capital One, N26, Monzo, Wise, HSBC, Starling Bank).

#### Standard Channels
| Channel | Status | Duress Detection | Key Features |
|---------|--------|------------------|--------------|
| **📱 Mobile Apps** | ✅ Full Support | Reversed PIN + Accelerometer | GPS tracking, rich behavioral data |
| **🌐 Web Portal** | ✅ Full Support | Reversed PIN + Mouse/Keyboard | Browser fingerprinting, session tracking |
| **💰 STK Push** | ⚠️ Limited | Reversed PIN via USSD | M-Pesa integration, quick transfers |
| **📞 USSD (*234#)** | ✅ PIN-Only | Reversed PIN | Feature phone support, no internet needed |

#### World-Class Channels (NEW) 🌟
| Channel | Features | Inspired By |
|---------|----------|-------------|
| **📱 Enhanced Mobile** | Biometrics, Push Notifications, QR Payments, Card Controls, Offline Mode | Chase, DBS, Revolut, N26 |
| **🌐 Enhanced Web** | WebAuthn/FIDO2, WebSockets, QR Login, AI Chatbot, Video Banking, PWA | HSBC, Bank of America, Capital One |
| **🎤 Voice Banking** | Alexa, Google Assistant, Siri, Voice Biometrics, NLP | Bank of America (Erica), Capital One (Eno) |
| **💬 Messaging** | WhatsApp, Telegram, Messenger, Interactive Payments | Wise, N26, Starling Bank |
| **⌚ Wearables** | Apple Watch, Galaxy Watch, Wear OS, Quick Balance, Tap-to-Pay | Chase, Capital One, Revolut |

**See [Multi-Channel Guide](./docs/MULTI_CHANNEL_GUIDE.md) and [World-Class Channels](./docs/WORLD_CLASS_CHANNELS.md) for detailed integration examples.**

## Wekeza API Integration

This protocol integrates with three Wekeza Banking API versions with **intelligent failover**:

1. **Wekeza.Core.Api** - Primary production API (Clean Architecture)
2. **ComprehensiveWekezaApi** - Comprehensive feature set
3. **MVP4.0** - Legacy MVP version

### Unified API Adapter Features (NEW) 🌟
- ✅ **Automatic Failover** - Primary → Secondary → Fallback
- ✅ **Circuit Breaker Pattern** - Prevents cascade failures (Netflix Hystrix-inspired)
- ✅ **Smart Caching** - 5-minute cache for transaction history
- ✅ **Performance Metrics** - Success rate, P95/P99 response times
- ✅ **Health Monitoring** - Real-time API health status
- ✅ **Load Balancing** - Intelligent request distribution

**See `src/integration/UnifiedApiAdapter.cs` for implementation details.**

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

### Core Documentation
- [Technical Specification](./Document.md) - Original Salama Protocol spec
- [API Reference](./docs/API_REFERENCE.md) - Complete API documentation
- [Integration Guide](./docs/INTEGRATION_GUIDE.md) - Mobile app integration
- [Security Guidelines](./docs/SECURITY.md) - Security best practices
- [Database Schema](./docs/DATABASE_SCHEMA.md) - PostgreSQL schema design
- [Architecture](./docs/ARCHITECTURE.md) - System architecture diagrams

### Multi-Channel Documentation (NEW) 🌟
- [Multi-Channel Guide](./docs/MULTI_CHANNEL_GUIDE.md) - All channels integration
- [World-Class Channels](./docs/WORLD_CLASS_CHANNELS.md) - **Enhanced features from top 100 banks**
- [Multi-Channel Confirmation](./MULTI_CHANNEL_CONFIRMATION.md) - Channel support matrix

### Implementation Summary
- [Implementation Summary](./IMPLEMENTATION_SUMMARY.md) - Complete feature overview

## License

Proprietary - © 2026 Wekeza Bank. All rights reserved.

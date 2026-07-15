# CommerceHub

A production-ready, multi-vendor e-commerce platform built with **.NET 10 Modular Monolith** architecture, Clean Architecture, DDD, and CQRS.

## Architecture

```
Start → Modular Monolith (Feature-Based) + Clean Architecture
        │
        │ 1M → 100M users
        │
        ▼
        Extract Independent Modules into Microservices only when required
```

### Backend Structure

```
backend/
├── src/
│   ├── Api/                          # Thin ASP.NET Core host
│   ├── Domain/                       # Shared DDD core
│   ├── Application/                  # Shared app services (MediatR pipeline)
│   ├── Infrastructure/               # Shared infrastructure
│   ├── SharedKernel/                 # Value objects, base types
│   ├── BuildingBlocks/               # Module registration system
│   ├── ServiceDefaults/              # OpenTelemetry, Hangfire, health checks
│   └── Modules/                      # Feature modules
│       ├── Identity/                 # Auth, users, roles, JWT
│       ├── Product/                  # Products, categories, brands, reviews
│       ├── Order/                    # Orders, sagas, dispute resolution
│       ├── Cart/                     # Redis-backed shopping cart
│       ├── Payment/                  # Stripe, Razorpay, UPI, refunds
│       ├── Inventory/                # Stock management, warehouses
│       ├── Vendor/                   # Vendor profiles, payouts, settlements
│       ├── Notification/             # Email, SMS, push, WhatsApp
│       ├── Cms/                      # Pages, banners, coupons, menus
│       ├── Analytics/                # Events, reports, audit logs
│       └── Ai/                       # Gemini AI chat, recommendations
└── tests/
    ├── UnitTests/
    ├── IntegrationTests/
    ├── ContractTests/
    └── EndToEndTests/
```

### Module Structure (Vertical Slices)

Each module follows the same pattern:
```
Modules/{ModuleName}/
├── Domain/
│   ├── Entities/
│   ├── Events/
│   ├── ValueObjects/
│   └── Repositories/        (interfaces)
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── Validators/
│   ├── DTOs/
│   └── Services/
├── Infrastructure/
│   ├── Persistence/          (DbContext, migrations, repository impl)
│   ├── Messaging/            (MassTransit consumers, saga)
│   └── Configurations/
└── Presentation/
    ├── Controllers/          (REST endpoints)
    ├── Middleware/
    └── Endpoints/
```

## Tech Stack

- **Backend:** .NET 10, C#, Clean Architecture, DDD, CQRS
- **Database:** MySQL 8 (services), PostgreSQL (analytics), Redis (cache/cart)
- **Messaging:** MassTransit + RabbitMQ (saga pattern, event-driven)
- **Frontend:** Angular 20 (Module Federation micro-frontends)
- **AI:** Gemini-powered AI agent with real-time SignalR chat
- **Observability:** OpenTelemetry, Prometheus, Serilog/Seq, Jaeger
- **Deployment:** Docker, Kubernetes, Helm, .NET Aspire

## Key Features

- 11 bounded contexts with proper service isolation
- Order lifecycle managed via MassTransit saga state machine
- Multi-gateway payment processing (Stripe, Razorpay, UPI)
- Multi-channel notifications (Email, SMS, Push, WhatsApp)
- Elasticsearch-powered product search
- Real-time AI chat with product recommendations
- Admin dashboard with analytics and reporting
- Vendor portal with payout management

## Quick Start

### Prerequisites
- .NET 10 SDK
- Docker & Docker Compose

### Local Development
```bash
# Start infrastructure
docker-compose up -d

# Run the API
cd backend/src/Api
dotnet run

# Run tests
cd backend/tests/UnitTests
dotnet test
```

### Configuration
All settings are bound via `IOptions<T>` pattern. See `appsettings.json` and `.env.example` for available configuration.

## Deployment

- **Free Tier:** Railway, Render, Fly.io (see `deploy/free-tier/README.md`)
- **Production:** Kubernetes with Helm charts (see `deploy/k8s/`)
- **CI/CD:** GitHub Actions (see `.github/workflows/`)

## Evolution Path

1. **Now:** Modular Monolith (single deployable)
2. **Later:** Extract modules needing independent scaling (Identity, Payment, Notification)
3. **Future:** Event-driven microservices with Kubernetes service mesh

## License

MIT

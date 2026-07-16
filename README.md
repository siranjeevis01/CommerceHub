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

## Tech Stack

- **Backend:** .NET 10, C#, Clean Architecture, DDD, CQRS
- **Database:** MySQL 8 (services), PostgreSQL (analytics), Redis (cache/cart)
- **Messaging:** MassTransit + RabbitMQ (saga pattern, event-driven)
- **Frontend:** Angular 20 (Module Federation micro-frontends)
- **Search:** Meilisearch (product search)
- **AI:** Gemini-powered AI agent with real-time SignalR chat
- **Observability:** OpenTelemetry, Prometheus
- **Deployment:** Docker, Render (free tier)

## Key Features

- 11 bounded contexts with proper service isolation
- Order lifecycle managed via MassTransit saga state machine
- Multi-gateway payment processing (Stripe, Razorpay, UPI)
- Multi-channel notifications (Email, SMS, Push, WhatsApp)
- Meilisearch-powered product search
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

## Deployment (100% Free)

Every service runs on a free tier with **no credit card required**:

| Service | Provider | Free Tier |
|---------|----------|-----------|
| **Backend API** | Render | 512MB RAM, spins down on idle |
| **Frontend** | Firebase Hosting | 10GB storage, 360MB/day transfer |
| **MySQL** (9 schemas) | TiDB Serverless | 5GB storage, 50M row reads/month |
| **PostgreSQL** (analytics) | Neon | 0.5GB storage |
| **Redis** | Upstash | 10K commands/day, serverless |
| **RabbitMQ** | CloudAMQP | 1M messages/month |
| **Search** | Meilisearch (self-hosted on Render) | Free tier |
| **CI/CD** | GitHub Actions | 2000 min/month |

### Deploy Steps

1. **Create free accounts** (no credit card needed):
   - [Render](https://render.com) — Backend API
   - [Firebase](https://firebase.google.com) — Frontend hosting
   - [TiDB Serverless](https://tidbcloud.com) — MySQL (select "Serverless" plan)
   - [Neon](https://neon.tech) — PostgreSQL
   - [Upstash](https://upstash.com) — Redis
   - [CloudAMQP](https://cloudamqp.com) — RabbitMQ (free plan)

2. **Set environment variables** in Render dashboard using `.env.example` as reference

3. **Deploy**: Push to `main` branch triggers automatic deployment via GitHub Actions

### CI/CD Pipeline
- **CI** (`.github/workflows/ci.yml`): Build + test on every push/PR
- **Deploy** (`.github/workflows/deploy.yml`): Auto-deploy backend to Render, frontend to Firebase

## Configuration

All settings are bound via `IOptions<T>` pattern. See `appsettings.json` and `.env.example` for available configuration.

## Evolution Path

1. **Now:** Modular Monolith (single deployable)
2. **Later:** Extract modules needing independent scaling (Identity, Payment, Notification)
3. **Future:** Event-driven microservices with Kubernetes service mesh

## License

MIT

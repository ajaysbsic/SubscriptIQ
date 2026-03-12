# SubscriptIQ

A **plug-and-play subscription, billing, entitlement, and feature-gating platform** that can be integrated into ANY application (web or mobile) to verify users, enforce subscription plans, gate features, handle payments, and work in distributed/microservice environments.

## 🚀 Features

### Core Capabilities
- **Subscription Management**: Full lifecycle management (create, upgrade, downgrade, cancel)
- **Billing**: Support for monthly, quarterly, yearly, and lifetime billing cycles
- **Stripe Integration**: Complete payment processing with webhooks and idempotency
- **Entitlements**: Feature flags and usage limits per plan
- **Multi-Tenancy**: Isolated tenant data with API key authentication
- **Event Sourcing**: Complete audit trail of subscription changes
- **Async Processing**: Outbox pattern for reliable event publishing

### Technical Highlights
- **.NET 8 Minimal APIs**: Modern, high-performance API endpoints
- **PostgreSQL**: Robust, production-ready database with JSONB support
- **EF Core**: Type-safe ORM with migrations
- **Domain-Driven Design**: Clean architecture with domain models
- **Background Services**: Automatic outbox processing
- **Idempotency**: Safe payment retries
- **Webhook Support**: Stripe webhook handling with signature verification

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) (for PostgreSQL)
- [Stripe Account](https://stripe.com/) (for payment processing)

## 🔧 Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/ajaysbsic/SubscriptIQ.git
cd SubscriptIQ
```

### 2. Start PostgreSQL
```bash
docker-compose up -d
```

### 3. Configure Stripe Keys
Edit `src/SubscriptIQ.Api/appsettings.json` and add your Stripe keys:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_secret_key",
    "PublishableKey": "pk_test_your_stripe_publishable_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  }
}
```

### 4. Run Database Migrations
```bash
cd src/SubscriptIQ.Api
dotnet ef migrations add InitialCreate --project ../SubscriptIQ.Infrastructure
dotnet ef database update --project ../SubscriptIQ.Infrastructure
```

### 5. Run the API
```bash
dotnet run --project src/SubscriptIQ.Api
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

## 📚 API Documentation

Once running, access the Swagger documentation at: `https://localhost:5001/swagger`

### Key Endpoints

#### Tenants
- `POST /api/tenants` - Create a new tenant (returns API key)
- `GET /api/tenants/{id}` - Get tenant details
- `GET /api/tenants` - List all tenants

#### Plans
- `POST /api/plans` - Create a subscription plan
- `GET /api/plans` - List all plans (filter by active)
- `GET /api/plans/{id}` - Get plan details
- `POST /api/plans/{id}/entitlements` - Add entitlement to plan

#### Subscriptions
- `POST /api/subscriptions` - Create subscription
- `GET /api/subscriptions/{id}` - Get subscription details
- `GET /api/subscriptions/tenant/{tenantId}` - Get tenant subscriptions
- `POST /api/subscriptions/{id}/cancel` - Cancel subscription
- `POST /api/subscriptions/{id}/upgrade` - Upgrade subscription
- `POST /api/subscriptions/{id}/downgrade` - Schedule downgrade

#### Entitlements (Feature Gating)
- `GET /api/entitlements/check-access` - Check feature access
- `GET /api/entitlements/check-usage` - Check usage limits
- `POST /api/entitlements/increment-usage` - Increment usage counter
- `GET /api/entitlements/usage` - Get current usage stats

#### Webhooks
- `POST /api/webhooks/stripe` - Stripe webhook endpoint

## 💡 Usage Examples

### 1. Create a Tenant
```bash
curl -X POST https://localhost:5001/api/tenants \
  -H "Content-Type: application/json" \
  -d '{"name": "Acme Corp"}'
```

Response includes `apiKey` for authentication.

### 2. Create a Plan
```bash
curl -X POST https://localhost:5001/api/plans \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Pro Plan",
    "description": "Professional tier with advanced features",
    "priceAmount": 49.99,
    "priceCurrency": "USD",
    "billingInterval": "Monthly",
    "trialDays": 14,
    "features": ["advanced-analytics", "priority-support"]
  }'
```

### 3. Add Entitlements
```bash
# Feature Flag
curl -X POST https://localhost:5001/api/plans/{planId}/entitlements \
  -H "Content-Type: application/json" \
  -d '{
    "featureKey": "advanced-analytics",
    "type": "FeatureFlag",
    "isEnabled": true
  }'

# Usage Limit
curl -X POST https://localhost:5001/api/plans/{planId}/entitlements \
  -H "Content-Type: application/json" \
  -d '{
    "featureKey": "api-calls",
    "type": "UsageLimit",
    "isEnabled": true,
    "limit": 10000
  }'
```

### 4. Create a Subscription
```bash
curl -X POST https://localhost:5001/api/subscriptions \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "{tenant-id}",
    "planId": "{plan-id}"
  }'
```

### 5. Check Feature Access
```bash
curl -X GET "https://localhost:5001/api/entitlements/check-access?tenantId={tenant-id}&featureKey=advanced-analytics"
```

### 6. Track Usage
```bash
curl -X POST https://localhost:5001/api/entitlements/increment-usage \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "{tenant-id}",
    "featureKey": "api-calls",
    "amount": 1
  }'
```

## 🏗️ Architecture

### Project Structure
```
SubscriptIQ/
├── src/
│   ├── SubscriptIQ.Api/          # Minimal API endpoints
│   ├── SubscriptIQ.Core/         # Domain models, events, services
│   └── SubscriptIQ.Infrastructure/  # EF Core, repositories, Stripe
└── tests/
    ├── SubscriptIQ.Tests/        # Unit tests
    └── SubscriptIQ.IntegrationTests/  # Integration tests
```

### Domain Models
- **Tenant**: Multi-tenant isolation with API keys
- **Plan**: Subscription plans with pricing
- **Subscription**: Event-sourced aggregate root
- **Entitlement**: Feature flags and usage limits
- **Payment**: Payment transactions with idempotency
- **UsageTracking**: Usage metering per billing period
- **OutboxMessage**: Reliable event publishing

### Event Sourcing
All subscription state changes are captured as domain events:
- `SubscriptionCreatedEvent`
- `SubscriptionActivatedEvent`
- `SubscriptionUpgradedEvent`
- `SubscriptionDowngradedEvent`
- `SubscriptionCancelledEvent`
- `SubscriptionRenewedEvent`
- `SubscriptionExpiredEvent`

## 🔐 Security

- **API Key Authentication**: Each tenant has a unique API key
- **Stripe Webhook Verification**: Signature validation on webhooks
- **Idempotency**: Safe payment retries with idempotency keys
- **CORS**: Configurable cross-origin policies
- **SQL Injection Protection**: Parameterized queries via EF Core

## 🚢 Deployment Options

### 1. SaaS (Cloud-Hosted)
Deploy to Azure, AWS, or any cloud provider with:
- Container orchestration (Kubernetes, ECS)
- Managed PostgreSQL (Azure Database, RDS)
- Load balancing
- Auto-scaling

### 2. Self-Hosted
Run on your own infrastructure:
- Docker Compose (development/small teams)
- VM with .NET 8 runtime
- On-premises PostgreSQL

### 3. Internal Service
Integrate as a microservice in your architecture:
- Service mesh integration
- Internal API gateway
- Service-to-service authentication

## 🧪 Testing

```bash
# Run unit tests
dotnet test tests/SubscriptIQ.Tests

# Run integration tests (requires running PostgreSQL)
dotnet test tests/SubscriptIQ.IntegrationTests
```

## 📝 Configuration

### Database Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=subscriptiq;Username=postgres;Password=postgres"
  }
}
```

### Stripe Configuration
```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Follow the existing code style
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Built with .NET 8 and Entity Framework Core
- Payment processing by Stripe
- Database powered by PostgreSQL

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub.

---

**Built with ❤️ for developers who need subscription management without the hassle**

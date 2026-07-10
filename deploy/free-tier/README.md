# CommerceHub - Free Hosting Deployment Guide

Deploy the full CommerceHub microservices architecture at **$0/month** using free tiers.

---

## Railway

Railway offers $5 free credit per month (no credit card required for free usage).

### Steps

1. Create an account at [railway.app](https://railway.app)
2. Connect your GitHub repository
3. Railway auto-detects `railway.json` at the root
4. Add a PostgreSQL plugin: **Settings > Databases > PostgreSQL**
5. Add a Redis plugin: **Settings > Databases > Redis**
6. Set environment variables in **Variables** tab (see below)
7. Deploy — Railway builds from the root `Dockerfile`

### Per-service deployment

For individual microservices, create separate Railway services and set the **Dockerfile Path** to:
```
src/Services/Identity/CommerceHub.Identity.Api/Dockerfile
```

### Limitations

- $5/month credit covers ~500 hours of a 512MB service
- Single region deployment
- Services sleep after inactivity on free plan

---

## Render

Render provides free tier for web services (spins down after 15 min of inactivity).

### Steps

1. Create an account at [render.com](https://render.com)
2. Connect your GitHub repository
3. Render auto-detects `render.yaml` at the root
4. Click **Apply Blueprint** to create all services
5. Manually set sensitive environment variables (marked `sync: false`)
6. Services deploy automatically on push

### Services created by render.yaml

| Service | Type | Free Tier |
|---------|------|-----------|
| commercehub-gateway | Web Service | Yes |
| commercehub-identity | Web Service | Yes |
| commercehub-product | Web Service | Yes |
| commercehub-order | Web Service | Yes |
| commercehub-cart | Web Service | Yes |
| commercehub-payment | Web Service | Yes |
| commercehub-vendor | Web Service | Yes |
| commercehub-inventory | Web Service | Yes |
| commercehub-notification | Web Service | Yes |
| commercehub-cms | Web Service | Yes |
| commercehub-analytics | Web Service | Yes |
| commercehub-frontend | Web Service | Yes |
| commercehub-db | PostgreSQL | Free (90 days) |
| commercehub-analytics-db | PostgreSQL | Free (90 days) |
| commercehub-redis | Redis | Free |

### Limitations

- Free web services spin down after 15 min of inactivity
- First request after spin-down takes ~30s
- PostgreSQL free instances expire after 90 days
- 750 hours/month per service

---

## Fly.io

Fly.io provides 3 shared-cpu-1x VMs with 256MB RAM free.

### Steps

1. Install flyctl: `fly auth login`
2. Launch the app: `fly launch`
3. Set secrets: `fly secrets set KEY=VALUE`
4. Deploy: `fly deploy`

### Deploy individual services

```bash
fly launch --name commercehub-identity --dockerfile src/Services/Identity/CommerceHub.Identity.Api/Dockerfile
fly deploy --app commercehub-identity
```

### Free tier VMs

```bash
fly scale count 1 --app commercehub-gateway
fly scale memory 256 --app commercehub-gateway
```

### Limitations

- 3 shared-cpu-1x VMs (256MB RAM each)
- 3GB persistent volume storage
- 160GB outbound transfer
- Auto-stop/start saves credits

---

## Environment Variables

All services share these base variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `JWT_KEY` | JWT signing key (min 32 chars) | `<generate-a-strong-key>` |
| `JWT_ISSUER` | Token issuer | `CommerceHub` |
| `JWT_AUDIENCE` | Token audience | `CommerceHubClient` |

### Service-specific database variables

| Variable | Service |
|----------|---------|
| `IDENTITY_DB_CONNECTION` | Identity |
| `PRODUCT_DB_CONNECTION` | Product |
| `ORDER_DB_CONNECTION` | Order |
| `PAYMENT_DB_CONNECTION` | Payment |
| `VENDOR_DB_CONNECTION` | Vendor |
| `INVENTORY_DB_CONNECTION` | Inventory |
| `CMS_DB_CONNECTION` | CMS |
| `ANALYTICS_DB_CONNECTION` | Analytics |

### Optional integrations

| Variable | Service | Purpose |
|----------|---------|---------|
| `STRIPE_SECRET_KEY` | Payment | Stripe payments |
| `STRIPE_WEBHOOK_SECRET` | Payment | Stripe webhooks |
| `RAZORPAY_KEY_ID` | Payment | Razorpay payments |
| `RAZORPAY_KEY_SECRET` | Payment | Razorpay secret |
| `TWILIO_ACCOUNT_SID` | Notification | SMS |
| `TWILIO_AUTH_TOKEN` | Notification | SMS |
| `SMTP_HOST` | Notification | Email |
| `SMTP_PORT` | Notification | Email |
| `SMTP_USER` | Notification | Email |
| `SMTP_PASS` | Notification | Email |

---

## Cost Breakdown

| Provider | Services | Cost |
|----------|----------|------|
| Railway | Gateway + 2-3 microservices | $0 (within $5 credit) |
| Render | All services + managed DB | $0 (free tier) |
| Fly.io | 3 VMs | $0 (free allowance) |

**Recommended approach**: Deploy gateway + critical services on one provider, frontend on another to maximize free tier limits.

### Minimal production setup ($0/month)

- **Railway**: Gateway + Identity + Product (3 services within $5 credit)
- **Render**: Frontend (static/nginx, free tier)
- **Upstash Redis**: Free tier Redis for caching (generous free tier)
- **PlanetScale/Supabase**: Free tier MySQL/PostgreSQL

This keeps everything within free tier limits while maintaining the full API gateway pattern.

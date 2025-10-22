
#  Bidzy Auction Platform – Backend

Welcome to the backend of **Bidzy**, a scalable and modular auction platform built with .NET Core, Docker, and SQL Server. This service powers real-time bidding, user management, auction lifecycle, and intelligent suggestions.

---

## 📚 Table of Contents

- [📦 Tech Stack](#-tech-stack)
- [📁 Folder Structure](#-folder-structure)
- [🚀 Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Setup Instructions](#setup-instructions)
- [⚙️ Environment Variables](#️-environment-variables)
- [🧪 Testing](#-testing)
- [🔐 Authentication](#-authentication)
- [🧠 Suggestion Engine](#-suggestion-engine)
- [🧩 Admin Workflow (BPMN)](#-admin-workflow-bpmn)
- [📈 Scalability](#-scalability)
- [📊 Monitoring & Logging](#-monitoring--logging)
- [📃 API Documentation](#-api-documentation)
- [🛠️ Contributing](#️-contributing)
- [📃 License](#-license)

---

## 📦 Tech Stack

| Layer             | Technology         |
|------------------|--------------------|
| Language         | C# (.NET 8)        |
| API Framework    | ASP.NET Core Web API |
| Database         | SQL Server         |
| Caching          | Redis              |
| Messaging Queue  | RabbitMQ           |
| Containerization | Docker + Docker Compose |
| Auth             | JWT + OAuth2       |

---

## 📁 Folder Structure

```
/Bidzy.Backend
│
├── /AuctionService         # Auction creation, bidding, lifecycle
├── /UserService            # Registration, login, roles
├── /SuggestionEngine       # Auction recommendation logic
├── /AdminWorkflowService   # BPMN-modeled admin processes
├── /NotificationService    # Email/SMS/in-app notifications
├── /SharedKernel           # Common models, interfaces, utilities
├── /Infrastructure         # DB context, repositories, integrations
├── /API.Gateway            # Unified entry point for clients
├── /Docker                 # Dockerfiles and Compose setup
└── docker-compose.yml      # Multi-service orchestration
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-org/bidzy-backend.git
   cd bidzy-backend
   ```

2. **Run with Docker Compose:**
   ```bash
   docker-compose up --build
   ```

3. **Access services:**
   - Backend API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`
   - Frontend: `http://localhost:3000`
   - SQL Server: `localhost:1433`
   - RabbitMQ Dashboard: `http://localhost:15672` (guest/guest)

---

## ⚙️ Environment Variables

These are defined in `docker-compose.yml` and injected into each container:

### 🔧 Backend Service

| Variable | Description |
|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Sets the environment (e.g., Development, Production) |
| `ASPNETCORE_URLS` | Binds the app to port 8080 inside the container |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string for main DB |
| `ConnectionStrings__HangfireConnection` | Connection string for Hangfire background jobs |
| `EmailSettings__Sender` | Email address used to send notifications |
| `EmailSettings__AppPassword` | App password for SMTP authentication |
| `Jwt__Key` | Secret key for JWT token signing |
| `Jwt__Issuer` | JWT token issuer |
| `Jwt__Audience` | JWT token audience |
| `Stripe__PublishableKey` | Stripe public key for frontend |
| `Stripe__SecretKey` | Stripe secret key for backend transactions |
| `Stripe__WebhookSecret` | Stripe webhook verification secret |
| `Stripe__Currency` | Default currency for auctions |
| `Stripe__CommissionRate` | Platform commission rate (e.g., 10%) |
| `IpRateLimiting__EnableEndpointRateLimiting` | Enables rate limiting per endpoint |
| `IpRateLimiting__StackBlockedRequests` | Controls request stacking behavior |
| `IpRateLimiting__RealIpHeader` | Header used to extract real IP |
| `IpRateLimiting__ClientIdHeader` | Header used to identify client |
| `IpRateLimiting__HttpStatusCode` | Status code returned when rate limit is hit |

### 🗄️ Database Service

| Variable | Description |
|---------|-------------|
| `SA_PASSWORD` | SQL Server admin password |
| `ACCEPT_EULA` | Required to accept Microsoft EULA |

### 🌐 Frontend Service

| Variable | Description |
|---------|-------------|
| `NEXT_PUBLIC_BASE_URL` | Base URL for backend API |
| `NEXT_PUBLIC_AUCTION_API_URL` | Endpoint for auction-related APIs |
| `NEXT_PUBLIC_AUCTION_HUB_URL` | SignalR hub for auction events |
| `NEXT_PUBLIC_BID_HUB_URL` | SignalR hub for bid events |
| `NEXT_PUBLIC_USER_HUB_URL` | SignalR hub for user events |

---

## 🧪 Testing

Run unit and integration tests:

```bash
dotnet test
```

---

## 🔐 Authentication

- JWT-based authentication
- Role-based access control (Admin, Seller, Bidder)

---

## 🧠 Suggestion Engine

- Rule-based filtering (category, price, tags)

---

## 📈 Scalability

- Stateless microservices for horizontal scaling
- Redis caching for hot auctions and sessions
- Asynchronous messaging via RabbitMQ

---


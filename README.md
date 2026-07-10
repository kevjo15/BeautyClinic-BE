# BeautyClinic-BE

A production-grade .NET 8 backend for a beauty clinic management system.
Built with Clean Architecture, CQRS, and a focus on observability, security, and testability.

The frontend lives in a separate repository ([ElsaBeauty-FE](https://github.com/Kevjo15/ElsaBeauty-FE)) and consumes this API.

---

## Highlights

- **Clean Architecture** with strictly enforced layer boundaries (API → Application → Domain → Infrastructure)
- **CQRS** via MediatR — every use case is a `Command` or `Query` with an isolated handler
- **Pipeline behaviors** for cross-cutting concerns: logging and validation
- **Structured logging** with Serilog + automatic HTTP request logging
- **Application Insights** integration for production telemetry
- **JWT auth** with refresh-token rotation, SHA256-hashed storage, and reuse detection
- **Real-time** chat and notifications via SignalR
- **Azure Blob Storage** for images, with short-lived SAS URLs and in-memory URL caching
- **44 unit/integration tests** across handlers, validators, and pipeline behaviors

---

## Tech Stack

| Concern | Choice |
|---------|--------|
| Runtime | .NET 8 (LTS) |
| API | ASP.NET Core, REST + SignalR |
| Persistence | Entity Framework Core + SQL Server |
| Identity | ASP.NET Identity + JWT bearer tokens |
| CQRS / Mediator | MediatR |
| Validation | FluentValidation |
| Object mapping | AutoMapper |
| Logging | Serilog (Console, ready for additional sinks) |
| Telemetry | Application Insights |
| Storage | Azure Blob Storage (Azurite locally) |
| Caching | `IMemoryCache` |
| Email / SMS | Azure Communication Services |
| Testing | NUnit + FakeItEasy |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  API-Layer        Controllers · SignalR Hubs · Middleware   │
├─────────────────────────────────────────────────────────────┤
│  Application      Commands · Queries · Handlers · DTOs      │
│                   Validators · Pipeline Behaviors           │
│                   Interfaces (abstractions only)            │
├─────────────────────────────────────────────────────────────┤
│  Domain           Models · OperationResult · Domain rules   │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure   EF Core · Identity · Repositories         │
│                   JWT · Refresh tokens · Azure Blob         │
│                   Email/SMS adapters · Notification temps   │
└─────────────────────────────────────────────────────────────┘
```

**Layer rules**

- The Application layer depends only on **abstractions** — never on EF Core, Azure SDKs, SignalR, or ASP.NET Identity directly.
- Concrete implementations live in Infrastructure (or in the API layer for transport-specific adapters).
- The Domain layer is framework-free.

### Request Flow (CQRS via MediatR)

Every request travels through the same pipeline:

```
HTTP Request
   ↓
Controller (thin — only translates HTTP ↔ MediatR)
   ↓
LoggingBehaviour      ← logs name, elapsed time, outcome
   ↓
ValidationBehaviour   ← runs all FluentValidation rules
   ↓
Handler               ← the actual use case
   ↓
OperationResult<T>    ← success / typed business failure
   ↓
Controller maps result → HTTP response (200/400/404/...)
```

`LoggingBehaviour` runs **outside** `ValidationBehaviour` so validation failures are observable as warnings.

### Result Handling

Expected business failures use `OperationResult<T>` — no exceptions for control flow:

```csharp
return OperationResult<BookingModel>.Failure(
    "Den valda tiden är inte längre tillgänglig.",
    OperationFailureType.Conflict
);
```

`BaseApiController.HandleResult` maps the failure type to the right HTTP status (`400`, `401`, `403`, `404`, `409`).
Unhandled exceptions hit `ErrorHandlingMiddleware` and become RFC 7807 Problem Details.

---

## Authentication

Production-grade JWT auth with secure refresh-token rotation.

| Token | Storage | Lifetime | Purpose |
|-------|---------|----------|---------|
| Access token | Client memory | 15 min | API authorization |
| Refresh token | HttpOnly secure cookie | 7 days | Obtain new access tokens |

**Login flow**

1. `POST /api/auth/login` — credentials in body
2. Server returns access token in JSON, sets refresh token in `HttpOnly; Secure` cookie scoped to `/api/auth`
3. Client keeps the access token in memory (never `localStorage`)

**Refresh flow**

1. `POST /api/auth/refresh` — refresh token comes from the cookie automatically
2. Server validates the SHA256-hashed token against the database
3. On success: rotate the token (old revoked, new issued), return a new access token
4. On reuse of a revoked token: **all** of the user's tokens are invalidated

**Endpoints**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/register` | POST | Create account |
| `/api/auth/login` | POST | Authenticate, receive tokens |
| `/api/auth/refresh` | POST | Rotate refresh token, return new access token |
| `/api/auth/logout` | POST | Revoke current refresh token |
| `/api/me` | GET | Current authenticated user |

**Security measures**

- SHA256-hashed refresh tokens with a server-side pepper (`JwtSettings:RefreshTokenPepper`)
- HttpOnly + Secure cookies (XSS protection)
- Rate limiting on login and refresh
- Reuse detection invalidates the entire token family

---

## Logging & Telemetry

- **Serilog** is wired in `Program.cs` via `UseSerilog`. Console sink is configured by default — file/Seq/etc. can be added without changing application code.
- **`UseSerilogRequestLogging`** writes one structured log line per HTTP request (method, path, status, elapsed).
- **`LoggingBehaviour`** logs every MediatR command/query: success at `Information`, business failures at `Warning`, exceptions at `Error`.
- **Application Insights** is registered with `AddApplicationInsightsTelemetry()`. Set `ApplicationInsights:ConnectionString` (or `ApplicationInsights__ConnectionString` in Azure) to enable production telemetry.
- Application code only depends on `ILogger<T>` — the logging implementation is swappable.

---

## Storage

Service images and homepage assets live in Azure Blob Storage (Azurite locally).

- **One container** (`images` by default) with virtual folders for grouping: `images/services/<guid>.png`, `images/homepage/Elsa2.png`.
- The database stores blob *paths*, not URLs. The backend issues **short-lived SAS URLs** on read.
- SAS URLs are cached in `IMemoryCache` for half the SAS lifetime, so repeat requests don't hammer Azure.
- `IFileService` is the abstraction. `AzureBlobFileService` is the production implementation. `NullFileService` is registered when no storage connection is configured (useful for tests / minimal local runs).

Configuration keys:

```json
"Storage": {
  "ConnectionString": "UseDevelopmentStorage=true",
  "ServiceImagesContainer": "images",
  "SasExpiryMinutes": 60
}
```

---

## Real-time Communication

Two SignalR hubs:

- `/chatHub` — booking-scoped chat between client and assigned employee
- `/notificationHub` — booking confirmations, reminders, system notifications

Auth uses the access token via the `access_token` query parameter (browsers can't set headers on WebSocket upgrades). Hubs are protected with the `SignalRPolicy` CORS policy.

---

## Testing

- 44 tests across handlers, validators, and pipeline behaviors
- NUnit + FakeItEasy for mocking
- Tests live next to the feature they cover under `Test-Layer/<Feature>Tests/`

```bash
dotnet test Test-Layer/Test-Layer.csproj
```

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker (for SQL Server + Azurite)

### 1. Clone and configure

```bash
git clone https://github.com/Kevjo15/BeautyClinic-BE.git
cd BeautyClinic-BE
cp API-Layer/appsettings.example.json API-Layer/appsettings.Development.json
```

Edit `appsettings.Development.json`:

- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `JwtSettings:Secret` — minimum 32 characters
- `JwtSettings:RefreshTokenPepper` — secure random string
- `Storage:ConnectionString` — `UseDevelopmentStorage=true` for Azurite
- `ApplicationInsights:ConnectionString` — optional, for telemetry

### 2. Start dependencies

```bash
# SQL Server
docker run -d --name dev-mssql -p 1433:1433 \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password123!" \
  mcr.microsoft.com/mssql/server:2022-latest

# Azurite (Blob Storage emulator)
docker run -d --name dev-azurite -p 10000:10000 \
  -e AZURITE_CORS=true \
  mcr.microsoft.com/azure-storage/azurite
```

### 3. Run

```bash
dotnet run --project API-Layer
```

The app applies migrations and seeds initial data on startup. Swagger UI is available at `/swagger` in development.

### 4. Verify

```bash
dotnet build ElsaBeauty-BE.sln
dotnet test Test-Layer/Test-Layer.csproj
```

---

## API Surface

All endpoints are documented via Swagger when running locally. High-level grouping:

| Group | Routes |
|-------|--------|
| Auth | `/api/auth/{register,login,refresh,logout}` |
| Current user | `/api/me` (full profile), `/api/me/profile`, `/api/me/password` |
| Users (admin) | `/api/users/employees` |
| Services | `/api/services` (image URLs SAS-signed on read), `/api/services/{id}/image` |
| Categories | `/api/categories`, `/api/categories/with-services` |
| Bookings | `/api/bookings`, `/api/bookings/availability`, `/api/bookings/me`, `/api/bookings/assigned` |
| Schedules | `/api/schedules/{employeeId}` |
| Work days | `/api/workdays/{employeeId}`, `/api/workdays/{employeeId}/generate` |
| Chat | `/api/conversations/{id}/messages` |
| Notifications | `/api/notifications` |
| SignalR hubs | `/chatHub`, `/notificationHub` |

---

## Project Structure

```
BeautyClinic-BE/
├── API-Layer/                  ASP.NET Core host, controllers, hubs, middleware
│   ├── Controllers/
│   ├── Hubs/
│   ├── Middleware/             ErrorHandlingMiddleware, RateLimiting
│   ├── Notifications/          SignalR notification adapter
│   └── Program.cs              Composition root
├── Application-Layer/
│   ├── Commands/<Feature>Commands/<Action>/
│   ├── Queries/<Feature>Queries/<Action>/
│   ├── Interfaces/             Abstractions only — never concretes
│   ├── PipelineBehaviour/      LoggingBehaviour, ValidationBehaviour
│   ├── Validators/
│   ├── DTOs/
│   └── AutoMapper/
├── Domain-Layer/
│   ├── Models/                 Booking, Service, User, Category, ...
│   └── Common/                 OperationResult, OperationFailureType
├── Infrastructure-Layer/
│   ├── Database/               EF Core DbContext + migrations
│   ├── Identity/               ASP.NET Identity user model
│   ├── Repositories/
│   ├── Storage/                Azure Blob, SAS, caching
│   ├── Notifications/          Email/SMS senders, templates
│   └── Services/               JWT, refresh token services
└── Test-Layer/                 NUnit, organized by feature
    ├── Behaviors/              Pipeline behavior tests
    ├── BookingTests/
    ├── ServiceTests/
    └── UserTests/
```

---

## License

MIT.

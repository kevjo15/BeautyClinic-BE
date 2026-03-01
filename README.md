# BeautyClinic-BE 🌟

A modern, scalable backend solution for beauty clinics built with .NET 8, following Clean Architecture and CQRS principles.

## About

This is the backend infrastructure that powers a beauty clinic management system. The system handles appointments, treatments, real-time notifications, and client communications through a modern API-driven architecture.

## 🚀 Features

- **Appointment Management**

  - Real-time booking system
  - Automated time slot availability
  - Booking notifications
  - Cancellation/modification handling

- **Service Management**

  - Categorized beauty services
  - Dynamic pricing
  - Service duration management
  - Category-based organization

- **Real-time Communication**

  - Live chat functionality using SignalR
  - Instant notifications
  - Booking confirmations
  - Appointment reminders

- **User Management**
  - JWT-based authentication
  - Role-based authorization
  - Secure password handling
  - Profile management

## 🏗️ Architecture

Built using Clean Architecture with 4 distinct layers:

- **API Layer**: REST endpoints, SignalR hubs
- **Application Layer**: CQRS implementation, DTOs, business logic
- **Domain Layer**: Core business models
- **Infrastructure Layer**: Database context, repositories, migrations

### Key Design Patterns

- Command Query Responsibility Segregation (CQRS)
- Repository Pattern
- Mediator Pattern (using MediatR)
- Domain-Driven Design principles

## 🛠️ Technology Stack

- **.NET 8** (Latest LTS version)
- **Entity Framework Core**
- **SignalR** for real-time communications
- **MediatR** for CQRS implementation
- **AutoMapper** for object mapping
- **FluentValidation** for robust validation
- **JWT** for authentication
- **SQL Server** for data persistence

## 🔒 Security Features

- JWT token authentication
- Refresh token rotation with reuse detection
- SHA256-hashed refresh tokens with server-side pepper
- HttpOnly secure cookies for refresh tokens
- Password hashing
- Input validation
- API endpoint protection

## 🔐 Authentication Flow

The system uses a production-grade JWT authentication with secure refresh token rotation:

### Token Types

| Token | Storage | Lifetime | Purpose |
|-------|---------|----------|---------|
| Access Token | Client memory | 15 minutes | API authorization |
| Refresh Token | HttpOnly cookie | 7 days | Obtain new access tokens |

### Login Flow

1. Client sends credentials to `POST /api/User/login`
2. Server validates and returns `accessToken` in response body
3. Server sets `refreshToken` in HttpOnly secure cookie
4. Client stores access token in memory (not localStorage)

### Token Refresh Flow

1. When access token expires, client calls `POST /api/User/refreshAccessToken`
2. Server reads refresh token from HttpOnly cookie
3. Server validates token against database (hashed)
4. If valid: rotates token (old revoked, new created), returns new access token
5. Client receives new access token, cookie is updated with new refresh token

### Security Measures

- **Token Rotation**: Each refresh creates a new token and revokes the old one
- **Reuse Detection**: If a revoked token is used, all user tokens are invalidated
- **Hashed Storage**: Refresh tokens are SHA256-hashed before database storage
- **HttpOnly Cookies**: Refresh tokens cannot be accessed by JavaScript (XSS protection)
- **Secure Flag**: Cookies only sent over HTTPS in production

### Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/User/login` | POST | Authenticate and receive tokens |
| `/api/User/refreshAccessToken` | POST | Get new access token using refresh cookie |
| `/api/User/revokeRefreshToken` | POST | Logout - revoke current refresh token |
| `/api/User/revokeAllTokens` | POST | Revoke all sessions for current user |

### Configuration (appsettings.json)

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "ElsaBeautyClinic",
    "Audience": "ElsaBeautyClinic",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7,
    "RefreshTokenPepper": "your-secure-pepper"
  }
}
```

## 🧪 Testing

- Unit tests using nUnit and FakeItEasy for mocking
- Integration tests for API endpoints
- Repository pattern tests
- Command/Query handler tests

## 💻 Getting Started

1. **Prerequisites**

   ```bash
   - .NET 8 SDK
   - SQL Server
   - Visual Studio 2022 or VS Code
   ```

2. **Configuration Setup**

   ```bash
   # Copy the example configuration file
   cp API-Layer/appsettings.example.json API-Layer/appsettings.json

   # Edit appsettings.json with your settings:
   # - Set your database connection
   # - Generate a secure JWT secret
   # - Configure other settings as needed
   ```

3. **Clone the Repository**

   ```bash
   git clone https://github.com/yourusername/BeautyHub-API.git
   ```

4. **Database Setup**

   ```bash
   cd BeautyHub-API
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run --project API-Layer
   ```

## 📝 API Documentation

The API includes endpoints for:

- User authentication and management
- Booking operations
- Service management
- Real-time notifications
- Chat functionality

Detailed API documentation is available through Swagger UI when running the application.

## 🎯 Key Features in Detail

### Booking System

- Smart time slot management
- Conflict prevention
- Real-time availability updates
- Automatic duration calculation

### Notification System

- Real-time booking notifications
- Appointment reminders
- Service updates
- Chat messages

### Chat System

- Real-time messaging
- Conversation management
- Message history
- User-to-user communication

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Contact

For any inquiries, please reach out through GitHub issues.

---

⭐ Don't forget to star this repository if you found it helpful!

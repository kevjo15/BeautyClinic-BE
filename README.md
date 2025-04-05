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
- Refresh token mechanism
- Password hashing
- Input validation
- API endpoint protection

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

2. **Clone the Repository**

   ```bash
   git clone https://github.com/yourusername/BeautyHub-API.git
   ```

3. **Database Setup**

   ```bash
   cd BeautyHub-API
   dotnet ef database update
   ```

4. **Run the Application**
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

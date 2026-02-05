# CloudContactManager - Project Scaffolding

A SaaS application skeleton for customer contact management built with ASP.NET Core 8.0 MVC.

## Tech Stack

- **Framework:** ASP.NET Core 8.0 MVC (C#)
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Cloud SDK:** AWS SDK (SES/SNS for notifications)

## Project Structure

```
CloudContactManager/
├── Controllers/
│   ├── HomeController.cs
│   ├── CustomersController.cs      # Customer CRUD operations
│   └── CommunicationController.cs  # Bulk SMS/Email operations
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext
├── Models/
│   └── Customer.cs                 # Customer entity (Id, FullName, Address, PhoneNumber, EmailAddress)
├── ViewModels/
│   └── CommunicationRequest.cs     # ViewModel for bulk communication
├── Services/
│   └── Interfaces/
│       └── INotificationService.cs # Notification service interface
├── Views/
│   ├── Customers/
│   ├── Communication/
│   ├── Home/
│   └── Shared/
├── wwwroot/
├── appsettings.json
└── Program.cs
```

## Components

### Models

- **Customer.cs**: Entity with properties matching assignment:
  - `Id` (int)
  - `FullName` (string)
  - `Address` (string?)
  - `PhoneNumber` (string)
  - `EmailAddress` (string)

### ViewModels

- **CommunicationRequest.cs**: ViewModel for bulk communication:
  - `CustomerIds` (List<int>) - Selected customer IDs
  - `MessageContent` (string) - Message to send
  - `Type` (string) - "SMS" or "Email"

### Data Layer

- **AppDbContext.cs**: Entity Framework Core DbContext with `DbSet<Customer>`

### Services

- **INotificationService.cs**: Interface defining:
  - `SendEmailAsync(string toEmail, string subject, string body)`
  - `SendSmsAsync(string phoneNumber, string message)`
  - `SendBulkAsync(List<string> recipients, string message, string type)`

### Controllers

- **CustomersController.cs**: Standard CRUD (Index, Create, Edit, Delete)
- **CommunicationController.cs**: Bulk communication (Index with checkboxes, Send action)

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=CloudContactManager;..."
  },
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1"
  }
}
```

### AWS Authentication

The project uses `AWSSDK.Extensions.NETCore.Setup` which supports:

1. **Local Development**: Uses AWS CLI profile from `~/.aws/credentials`
2. **EC2 Deployment**: Automatically uses IAM Instance Profile (no config needed)

See `Program.cs` for detailed comments on credential resolution.

## Getting Started

1. Clone the repository
2. Update `appsettings.json` with your configuration
3. Implement `INotificationService` using AWS SDK
4. Register the implementation in `Program.cs`
5. Run migrations: `dotnet ef migrations add InitialCreate`
6. Update database: `dotnet ef database update`
7. Run the application: `dotnet run`

## TODO

- [ ] Implement `INotificationService` using AWS SES/SNS
- [ ] Add data validation attributes to Customer model
- [ ] Create Customer views (Index, Create, Edit, Delete)
- [ ] Implement CRUD business logic in CustomersController
- [ ] Implement bulk communication logic in CommunicationController
- [ ] Add authentication and authorization

## License

This project is created for educational purposes as part of a Cloud Computing course.

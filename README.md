# CloudContactManager

A SaaS application for customer contact management built with ASP.NET Core 8.0 MVC, deployed on AWS (EC2 & RDS).

## Tech Stack

- **Framework:** ASP.NET Core 8.0 MVC (C#)
- **ORM:** Entity Framework Core 8.0
- **Database:** SQL Server (LocalDB for development, RDS for production)
- **Cloud SDK:** AWS SDK (SES for Email, SNS for SMS)
- **UI:** Bootstrap 5 + Bootstrap Icons

## Project Structure

```
CloudContactManager/
├── Controllers/
│   ├── HomeController.cs
│   ├── CustomersController.cs          # Customer CRUD operations
│   └── CommunicationController.cs      # Bulk SMS/Email operations
├── Data/
│   └── AppDbContext.cs                 # EF Core DbContext
├── Models/
│   └── Customer.cs                     # Customer entity with validation
├── ViewModels/
│   └── CommunicationRequest.cs         # ViewModel for bulk communication
├── Services/
│   ├── Interfaces/
│   │   └── INotificationService.cs     # Notification service interface
│   ├── AwsNotificationService.cs       # AWS SES/SNS implementation
│   └── LocalNotificationService.cs     # Local simulation (no AWS required)
├── Migrations/                         # EF Core migrations
├── Views/
│   ├── Customers/                      # Index, Create, Edit, Delete
│   ├── Communication/                  # Bulk send with checkbox selection
│   ├── Home/
│   └── Shared/
├── wwwroot/
├── appsettings.json
└── Program.cs
```

## Components

### Models

- **Customer.cs**: Entity with Data Annotations validation:
  - `Id` (int)
  - `FullName` (string, required)
  - `Address` (string?)
  - `PhoneNumber` (string, required)
  - `EmailAddress` (string, required, email format)
  - `CreatedAt` (DateTime)

### ViewModels

- **CommunicationRequest.cs**: ViewModel for bulk communication:
  - `CustomerIds` (List\<int\>) — selected customer IDs from checkboxes
  - `MessageContent` (string) — message body
  - `Type` (string) — `"SMS"` or `"Email"`

### Data Layer

- **AppDbContext.cs**: Entity Framework Core DbContext with `DbSet<Customer>`

### Services

- **INotificationService.cs**: Interface defining:
  - `SendEmailAsync(string toEmail, string subject, string body)`
  - `SendSmsAsync(string phoneNumber, string message)`
  - `SendBulkAsync(List<string> recipients, string message, string type)`
- **AwsNotificationService**: Production implementation using AWS SES/SNS
- **LocalNotificationService**: Development implementation that logs to console

### Controllers

- **CustomersController.cs**: CRUD — Index, Create, Edit, Delete (async/await)
- **CommunicationController.cs**: Bulk communication — checkbox selection, Send action

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CloudContactManager;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1"
  }
}
```

For production, replace `DefaultConnection` with your RDS endpoint.

### AWS Authentication

The project uses `AWSSDK.Extensions.NETCore.Setup` and auto-detects credentials:

1. **Local Development**: No AWS credentials required — `LocalNotificationService` is used automatically, all notifications are logged to console
2. **With AWS credentials**: If `~/.aws/credentials` exists, `AwsNotificationService` is used (real SES/SNS)
3. **EC2 Deployment**: Automatically uses IAM Instance Profile — no credentials needed in config

See `Program.cs` for credential detection logic.

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| AWSSDK.Extensions.NETCore.Setup | 3.7.301 | AWS SDK DI integration |
| AWSSDK.SimpleEmail | 3.7.401.2 | AWS SES — Email |
| AWSSDK.SimpleNotificationService | 3.7.400.54 | AWS SNS — SMS |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | EF Core SQL Server |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | EF Core migrations CLI |

## Getting Started

### Prerequisites

| Tool | Notes |
|---|---|
| [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | Required |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) | Required |
| SQL Server LocalDB | Bundled with Visual Studio — enable via VS Installer → Individual Components → *SQL Server Express LocalDB* |
| EF Core CLI | Run once: `dotnet tool install --global dotnet-ef` |
| [AWS CLI](https://aws.amazon.com/cli/) | Optional — only needed for production deployment |

### Step 1 — Configure the database connection

Open `appsettings.json` and verify the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CloudContactManager;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 2 — Create the database

Run once in a terminal inside the project folder:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the `CloudContactManager` database and `Customers` table on LocalDB.

### Step 3 — Run the app

Stop any existing instance first:
```powershell
Get-Process -Name CloudContactManager -ErrorAction SilentlyContinue | Stop-Process -Force
```

Start the app (opens a new CMD window as the server):
```powershell
Start-Process cmd -ArgumentList '/k cd /d D:\CS\CLOUDCOMPUTING\V2\Cloud-Computing-Final-Project\CloudContactManager && dotnet run --urls http://localhost:5028'
```

> A black CMD window will open — that is the running server. **Do not close it.**

### Step 4 — Open in browser

Navigate to **http://localhost:5028**

| Page | URL |
|---|---|
| Customer list | http://localhost:5028/Customers |
| Add customer | http://localhost:5028/Customers/Create |
| Send bulk messages | http://localhost:5028/Communication |

### Stopping the app

```powershell
Get-Process -Name CloudContactManager -ErrorAction SilentlyContinue | Stop-Process -Force
```

> See **[RUNNING_LOCALLY.md](./RUNNING_LOCALLY.md)** for the full local development guide.

## License

This project is created for educational purposes as part of a Cloud Computing course.

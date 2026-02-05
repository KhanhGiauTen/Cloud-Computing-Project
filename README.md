# CloudContactManager - Cloud Computing Final Project

A SaaS Customer Management System built with ASP.NET Core 8.0 MVC, featuring AWS integration for email and SMS notifications.

## Tech Stack

- **Framework:** ASP.NET Core 8.0 MVC (C#)
- **Database:** SQL Server (Amazon RDS compatible)
- **Cloud Services:** 
  - AWS SES (Simple Email Service) - Email notifications
  - AWS SNS (Simple Notification Service) - SMS notifications

## Project Structure

```
CloudContactManager/
├── Controllers/
│   └── CustomersController.cs    # CRUD operations for customers
├── Data/
│   └── ApplicationDbContext.cs   # Entity Framework Core DbContext
├── Models/
│   └── Customer.cs               # Customer entity with data annotations
├── Services/
│   ├── INotificationService.cs   # Interface for notification services
│   └── AwsNotificationService.cs # AWS SES & SNS implementation
├── Views/
│   └── Customers/                # Customer CRUD views
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       ├── Details.cshtml
│       └── Delete.cshtml
├── appsettings.json              # Configuration (AWS, ConnectionStrings)
└── Program.cs                    # Application entry point & DI configuration
```

## Features

1. **Customer Management (CRUD)**
   - Create new customers with validation
   - View customer list and details
   - Edit customer information
   - Delete customers

2. **AWS Integration**
   - Welcome email sent automatically when creating a new customer (AWS SES)
   - Send SMS to customers from the details page (AWS SNS)
   - Graceful error handling for AWS Sandbox limitations

3. **Security**
   - No hardcoded AWS credentials
   - Configuration via `appsettings.json` or Environment Variables
   - Anti-forgery token validation on forms

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-rds-endpoint.region.rds.amazonaws.com;Database=CloudContactManager;User Id=your-username;Password=your-password;TrustServerCertificate=True;"
  },
  "AWS": {
    "Region": "us-east-1",
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY",
    "SenderEmail": "verified-email@yourdomain.com"
  }
}
```

### Environment Variables (Recommended for Production)

```bash
export ConnectionStrings__DefaultConnection="your-connection-string"
export AWS__Region="us-east-1"
export AWS__AccessKey="your-access-key"
export AWS__SecretKey="your-secret-key"
export AWS__SenderEmail="noreply@yourdomain.com"
```

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/KhanhGiauTen/Cloud-Computing-Final-Project.git
   cd Cloud-Computing-Final-Project/CloudContactManager
   ```

2. **Update Configuration**
   - Update `appsettings.json` with your AWS credentials and RDS connection string
   - Or set environment variables

3. **Apply Database Migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - Click on "Customers" in the navigation menu

## AWS Setup Notes

### AWS SES (Simple Email Service)
- In Sandbox mode, both sender and recipient emails must be verified
- Move to production mode to send emails to any address
- Configure sender email in `appsettings.json` under `AWS:SenderEmail`

### AWS SNS (Simple Notification Service)
- Phone numbers should be in E.164 format (e.g., +1234567890)
- In Sandbox mode, you may need to verify phone numbers
- SMS type is set to "Transactional" for reliable delivery

## Dependencies

- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.EntityFrameworkCore.Tools (8.0.0)
- AWSSDK.SimpleEmail
- AWSSDK.SimpleNotificationService

## License

This project is created for educational purposes as part of a Cloud Computing course.
# GitHub Issues for CloudContactManager Project

This document contains pre-formatted GitHub Issues for tracking the remaining work on the CloudContactManager project. Each issue can be directly copy-pasted into GitHub Issues.

---

## Issue 1: Database Implementation & Migration

### Title
```
Database Implementation & Migration
```

### Body
```markdown
## Description

Implement the database layer for the CloudContactManager application using Entity Framework Core with SQL Server.

## Tasks

- [ ] Verify `Customer` model attributes are correctly defined:
  - `Id` (int, Primary Key)
  - `FullName` (string, Required)
  - `Address` (string, Optional)
  - `PhoneNumber` (string, Required)
  - `EmailAddress` (string, Required)
- [ ] Add data validation attributes to the `Customer` model (e.g., `[Required]`, `[EmailAddress]`, `[Phone]`)
- [ ] Configure the connection string in `appsettings.json` for Local SQL Server
- [ ] Create the EF Core Initial Migration:
  ```bash
  dotnet ef migrations add InitialCreate
  ```
- [ ] Apply the migration to create the database:
  ```bash
  dotnet ef database update
  ```

## Acceptance Criteria

- [ ] Database `CloudContactManager` is created in Local SQL Server
- [ ] Table `Customers` exists with correct schema matching the model
- [ ] Connection string is properly configured and application can connect to the database
- [ ] No migration errors occur during the database creation process

## Technical Notes

- Ensure `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Tools` packages are installed
- The `AppDbContext` is already configured in `Data/AppDbContext.cs`
- Update `Program.cs` if needed to ensure proper DbContext registration

## Labels

`database`, `backend`, `priority-high`
```

---

## Issue 2: Implement Customer CRUD Logic & Views

### Title
```
Implement Customer CRUD Logic & Views
```

### Body
```markdown
## Description

Complete the Customer management functionality with full CRUD (Create, Read, Update, Delete) operations and corresponding Razor Views using Bootstrap for UI styling.

## Tasks

### Controller Implementation (`CustomersController.cs`)

- [ ] **Index Action**: Retrieve all customers from database and pass to view
- [ ] **Create GET Action**: Return empty form for creating new customer
- [ ] **Create POST Action**: Validate and save new customer to database, redirect to Index
- [ ] **Edit GET Action**: Retrieve customer by ID and return populated edit form
- [ ] **Edit POST Action**: Validate and update existing customer in database
- [ ] **Delete GET Action**: Retrieve customer and show delete confirmation view
- [ ] **DeleteConfirmed POST Action**: Remove customer from database

### View Implementation (`Views/Customers/`)

- [ ] **Index.cshtml**: Display table of all customers with Edit/Delete links
- [ ] **Create.cshtml**: Form with fields for FullName, Address, PhoneNumber, EmailAddress
- [ ] **Edit.cshtml**: Pre-populated form for editing existing customer
- [ ] **Delete.cshtml**: Confirmation page showing customer details before deletion

### UI Requirements

- [ ] Use Bootstrap classes for consistent styling
- [ ] Include form validation feedback (client-side and server-side)
- [ ] Add navigation links between views
- [ ] Ensure responsive design

## Acceptance Criteria

- [ ] Can add a new customer via the web interface
- [ ] Customer list displays all customers from the database
- [ ] Can edit existing customer information
- [ ] Can delete customers with confirmation prompt
- [ ] Data persists in the SQL Server database
- [ ] Form validation prevents invalid data submission
- [ ] UI is responsive and uses Bootstrap styling

## Dependencies

- Issue #1: Database Implementation & Migration (must be completed first)

## Labels

`frontend`, `backend`, `crud`, `priority-high`
```

---

## Issue 3: Implement AWS Service Integration (SES & SNS)

### Title
```
Implement AWS Service Integration (SES & SNS)
```

### Body
```markdown
## Description

Create the `AwsNotificationService` class that implements the `INotificationService` interface using AWS SDK for sending emails (SES) and SMS messages (SNS).

## Tasks

### Service Implementation

- [ ] Create `Services/AwsNotificationService.cs` implementing `INotificationService`
- [ ] Install required NuGet packages:
  ```bash
  dotnet add package AWSSDK.SimpleEmail
  dotnet add package AWSSDK.SimpleNotificationService
  ```
- [ ] Implement `SendEmailAsync(string toEmail, string subject, string body)`:
  - Use `AmazonSimpleEmailServiceClient`
  - Send email using SES `SendEmailAsync` method
  - Include proper exception handling for sandbox limits
- [ ] Implement `SendSmsAsync(string phoneNumber, string message)`:
  - Use `AmazonSimpleNotificationServiceClient`
  - Send SMS using SNS `PublishAsync` method
  - Phone number should be in E.164 format (e.g., +1234567890)
- [ ] Implement `SendBulkAsync(List<string> recipients, string message, string type)`:
  - Loop through recipients and call appropriate method based on type ("SMS" or "Email")
  - Handle partial failures gracefully

### Dependency Injection Setup

- [ ] Register `AwsNotificationService` as `INotificationService` in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<INotificationService, AwsNotificationService>();
  ```
- [ ] Uncomment `INotificationService` dependencies in controllers

### Error Handling (Required)

- [ ] Wrap all AWS calls in try-catch blocks
- [ ] Log exceptions appropriately
- [ ] Handle common AWS errors:
  - `MessageRejected` (SES sandbox/verification issues)
  - `AmazonSimpleNotificationServiceException` (SNS limit issues)
  - `AmazonServiceException` (general AWS errors)
- [ ] Return meaningful error messages to the user

## Acceptance Criteria

- [ ] Code compiles without errors
- [ ] `AwsNotificationService` properly implements all three interface methods
- [ ] Exception handling is in place for AWS sandbox limitations
- [ ] Service is registered in dependency injection container
- [ ] Unit test or mock test confirms method calls work correctly

## Technical Notes

- AWS credentials can be configured via:
  - AWS CLI profile (`~/.aws/credentials`) for local development
  - IAM Instance Profile for EC2 deployment
- SES requires email verification in sandbox mode
- SNS SMS requires phone number verification in sandbox mode

## Labels

`aws`, `backend`, `integration`, `priority-high`
```

---

## Issue 4: Implement Bulk Communication Feature

### Title
```
Implement Bulk Communication Feature
```

### Body
```markdown
## Description

Complete the bulk communication functionality that allows users to select multiple customers and send SMS or Email messages to all selected recipients.

## Tasks

### Controller Implementation (`CommunicationController.cs`)

- [ ] **Index GET Action**:
  - Retrieve all customers from database
  - Pass customer list to view for checkbox selection
- [ ] **Send POST Action**:
  - Validate the `CommunicationRequest` model
  - Retrieve selected customers using `CustomerIds` from request
  - Extract recipient addresses based on communication type:
    - If "Email": collect `EmailAddress` from selected customers
    - If "SMS": collect `PhoneNumber` from selected customers
  - Call `_notificationService.SendBulkAsync(recipients, message, type)`
  - Handle success/failure and display appropriate message
  - Redirect back to Index with status message

### View Implementation (`Views/Communication/Index.cshtml`)

- [ ] Display all customers in a list with checkboxes for selection
- [ ] Each checkbox should submit the customer ID
- [ ] Include dropdown to select communication type (SMS/Email)
- [ ] Include textarea for message content
- [ ] Add "Select All" / "Deselect All" functionality (optional enhancement)
- [ ] Display success/error messages after sending

### ViewModel Updates (if needed)

- [ ] Verify `CommunicationRequest.cs` includes:
  - `List<int> CustomerIds`
  - `string MessageContent`
  - `string Type` ("SMS" or "Email")

## Acceptance Criteria

- [ ] Communication page displays all customers with selection checkboxes
- [ ] User can select multiple customers using checkboxes
- [ ] User can choose between SMS and Email communication types
- [ ] User can enter a custom message or use default "Hello" message
- [ ] Submit button triggers `SendBulkAsync` for all selected customers
- [ ] Success/error feedback is displayed to the user
- [ ] System handles cases where no customers are selected

## Dependencies

- Issue #2: Implement Customer CRUD Logic & Views (customers must exist)
- Issue #3: Implement AWS Service Integration (notification service must work)

## Labels

`frontend`, `backend`, `feature`, `priority-medium`
```

---

## Issue 5: AWS Infrastructure Configuration (AWS Console)

### Title
```
AWS Infrastructure Configuration (AWS Console)
```

### Body
```markdown
## Description

**Manual Task**: Configure the AWS infrastructure required to host the CloudContactManager application according to the university assignment architecture requirements.

## Tasks

### VPC Configuration

- [ ] Create a new VPC with appropriate CIDR block (e.g., 10.0.0.0/16)
- [ ] Enable DNS hostnames and DNS resolution

### Subnet Configuration

- [ ] Create **Public Subnet** (e.g., 10.0.1.0/24):
  - For EC2 web server
  - Enable auto-assign public IP
- [ ] Create **Private Subnet** (e.g., 10.0.2.0/24):
  - For RDS database server
  - No public IP assignment

### Internet Gateway & Route Tables

- [ ] Create and attach Internet Gateway to VPC
- [ ] Create Route Table for public subnet with route to Internet Gateway (0.0.0.0/0)
- [ ] Associate public subnet with public route table

### Security Groups

- [ ] Create **Web Security Group** (for EC2):
  - Inbound: HTTP (80), HTTPS (443), SSH (22) from your IP
  - Outbound: All traffic
- [ ] Create **Database Security Group** (for RDS):
  - Inbound: SQL Server (1433) or MySQL (3306) from Web Security Group only
  - Outbound: All traffic

### RDS Instance

- [ ] Create RDS subnet group using private subnets
- [ ] Launch RDS instance:
  - Engine: SQL Server Express or MySQL
  - Instance class: db.t3.micro (free tier eligible)
  - Attach Database Security Group
  - Note down endpoint, port, username, password

### EC2 Instance

- [ ] Launch EC2 instance:
  - AMI: Amazon Linux 2 or Ubuntu Server
  - Instance type: t2.micro (free tier eligible)
  - Place in public subnet
  - Attach Web Security Group
  - Create or use existing key pair for SSH access
- [ ] Allocate and associate Elastic IP (optional but recommended)

### IAM Configuration

- [ ] Create IAM Role for EC2 with policies:
  - `AmazonSESFullAccess` (for email)
  - `AmazonSNSFullAccess` (for SMS)
- [ ] Attach IAM Role to EC2 instance

## Acceptance Criteria

- [ ] VPC is created with proper CIDR configuration
- [ ] Public and private subnets are correctly configured
- [ ] Security groups restrict access appropriately
- [ ] RDS instance is running and accessible from EC2 only
- [ ] EC2 instance is running and accessible via public IP
- [ ] Architecture matches the design diagram from assignment
- [ ] Connection string for RDS is obtained and documented

## Deliverables

- [ ] Screenshot of VPC configuration
- [ ] Screenshot of EC2 instance running
- [ ] Screenshot of RDS instance running
- [ ] RDS connection string (secure storage)
- [ ] EC2 public IP/DNS

## Labels

`aws`, `infrastructure`, `manual`, `priority-high`
```

---

## Issue 6: Deployment & Production Configuration

### Title
```
Deployment & Production Configuration
```

### Body
```markdown
## Description

Configure the production environment and deploy the CloudContactManager application to the AWS EC2 instance.

## Tasks

### Production Configuration

- [ ] Create `appsettings.Production.json` with production settings:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=<RDS-ENDPOINT>;Database=CloudContactManager;User Id=<username>;Password=<password>;TrustServerCertificate=True;"
    },
    "AWS": {
      "Region": "us-east-1"
    },
    "Logging": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  }
  ```
- [ ] Ensure sensitive credentials are not committed to source control
- [ ] Consider using AWS Secrets Manager or environment variables for secrets

### Application Publishing

- [ ] Publish the application for Linux deployment:
  ```bash
  dotnet publish -c Release -r linux-x64 --self-contained false
  ```
- [ ] Or for Windows deployment:
  ```bash
  dotnet publish -c Release -r win-x64 --self-contained false
  ```

### EC2 Server Setup

- [ ] Connect to EC2 instance via SSH
- [ ] Install .NET Runtime:
  ```bash
  # For Amazon Linux 2
  sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
  sudo yum install dotnet-runtime-8.0
  ```
- [ ] Create application directory:
  ```bash
  sudo mkdir -p /var/www/cloudcontactmanager
  ```
- [ ] Transfer published files to EC2 (using SCP or SFTP)

### Reverse Proxy Configuration

#### Option A: Nginx (Linux)
- [ ] Install Nginx:
  ```bash
  sudo yum install nginx  # Amazon Linux
  sudo apt install nginx  # Ubuntu
  ```
- [ ] Configure Nginx as reverse proxy (`/etc/nginx/conf.d/cloudcontactmanager.conf`):
  ```nginx
  server {
      listen 80;
      server_name _;
      
      location / {
          proxy_pass http://localhost:5000;
          proxy_http_version 1.1;
          proxy_set_header Upgrade $http_upgrade;
          proxy_set_header Connection keep-alive;
          proxy_set_header Host $host;
          proxy_cache_bypass $http_upgrade;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;
      }
  }
  ```
- [ ] Start and enable Nginx

#### Option B: IIS (Windows)
- [ ] Install IIS and ASP.NET Core Hosting Bundle
- [ ] Create IIS website pointing to published folder
- [ ] Configure application pool for No Managed Code

### Systemd Service (Linux)

- [ ] Create systemd service file (`/etc/systemd/system/cloudcontactmanager.service`):
  ```ini
  [Unit]
  Description=CloudContactManager ASP.NET Core App
  After=network.target

  [Service]
  WorkingDirectory=/var/www/cloudcontactmanager
  ExecStart=/usr/bin/dotnet /var/www/cloudcontactmanager/CloudContactManager.dll
  Restart=always
  RestartSec=10
  SyslogIdentifier=cloudcontactmanager
  User=www-data
  Environment=ASPNETCORE_ENVIRONMENT=Production
  Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

  [Install]
  WantedBy=multi-user.target
  ```
- [ ] Enable and start the service:
  ```bash
  sudo systemctl enable cloudcontactmanager
  sudo systemctl start cloudcontactmanager
  ```

### Database Migration (Production)

- [ ] Run EF Core migrations on production database:
  ```bash
  dotnet ef database update --connection "<production-connection-string>"
  ```

### Verification

- [ ] Verify application is running on localhost:5000
- [ ] Verify Nginx/IIS is proxying correctly
- [ ] Test application via EC2 public IP
- [ ] Verify database connectivity
- [ ] Test customer CRUD operations
- [ ] Test communication features (may fail due to AWS sandbox)

## Acceptance Criteria

- [ ] Application is deployed to EC2 instance
- [ ] Application runs as a background service (systemd or Windows Service)
- [ ] Reverse proxy (Nginx/IIS) is configured and working
- [ ] Application is accessible via EC2 Public IP or DNS
- [ ] Database connection to RDS is working
- [ ] All CRUD operations work in production
- [ ] Application logs are being captured

## Dependencies

- Issue #1-4: All application features must be implemented
- Issue #5: AWS Infrastructure must be configured

## Labels

`deployment`, `devops`, `production`, `priority-high`
```

---

## Quick Reference: Issue Creation Order

For optimal workflow, create and work on these issues in the following order:

1. **Issue #1: Database Implementation & Migration** - Foundation for all data operations
2. **Issue #2: Implement Customer CRUD Logic & Views** - Core application functionality
3. **Issue #3: Implement AWS Service Integration** - Backend notification service
4. **Issue #4: Implement Bulk Communication Feature** - Depends on #2 and #3
5. **Issue #5: AWS Infrastructure Configuration** - Can be done in parallel with #1-4
6. **Issue #6: Deployment & Production Configuration** - Final step after all features complete

## Labels to Create in GitHub

Before creating issues, set up these labels in your repository:

| Label | Color | Description |
|-------|-------|-------------|
| `database` | `#0052CC` | Database related tasks |
| `backend` | `#5319E7` | Backend/server-side code |
| `frontend` | `#FBCA04` | Frontend/UI related tasks |
| `aws` | `#FF9900` | AWS services and infrastructure |
| `crud` | `#1D76DB` | CRUD operations |
| `integration` | `#D93F0B` | External service integration |
| `feature` | `#0E8A16` | New feature implementation |
| `infrastructure` | `#BFD4F2` | Infrastructure setup |
| `deployment` | `#C2E0C6` | Deployment related |
| `devops` | `#E99695` | DevOps tasks |
| `manual` | `#FEF2C0` | Manual/non-code tasks |
| `priority-high` | `#B60205` | High priority |
| `priority-medium` | `#FBCA04` | Medium priority |
| `production` | `#006B75` | Production environment |

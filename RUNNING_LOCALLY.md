# Running Locally

## Prerequisites

| Tool | Required | Install |
|---|---|---|
| .NET 8.0 SDK | ✅ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Visual Studio 2022 | ✅ | https://visualstudio.microsoft.com/ |
| SQL Server LocalDB | ✅ (bundled with VS) | Visual Studio Installer → Individual Components → SQL Server Express LocalDB |
| EF Core CLI | ✅ | `dotnet tool install --global dotnet-ef` |
| AWS CLI | ❌ (production only) | https://aws.amazon.com/cli/ |

## First-Time Setup — Create the Database

```powershell
cd D:\CS\CLOUDCOMPUTING\V2\Cloud-Computing-Final-Project\CloudContactManager
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Running the App

**Step 1 — Stop any existing instance:**
```powershell
Get-Process -Name CloudContactManager -ErrorAction SilentlyContinue | Stop-Process -Force
```

**Step 2 — Start the app:**
```powershell
Start-Process cmd -ArgumentList '/k cd /d D:\CS\CLOUDCOMPUTING\V2\Cloud-Computing-Final-Project\CloudContactManager && dotnet run --urls http://localhost:5028'
```

A black CMD window will open — that is the server. **Do not close it.**

**Step 3 — Open the browser:**

Navigate to **http://localhost:5028**

| Page | URL |
|---|---|
| Home | http://localhost:5028 |
| Customer list | http://localhost:5028/Customers |
| Add customer | http://localhost:5028/Customers/Create |
| Send bulk messages | http://localhost:5028/Communication |

## Stopping the App

Close the black CMD window, or run:
```powershell
Get-Process -Name CloudContactManager -ErrorAction SilentlyContinue | Stop-Process -Force
```

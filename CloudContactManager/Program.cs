using CloudContactManager.Data;
using CloudContactManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================================
// AWS SDK Configuration using AWSSDK.Extensions.NETCore.Setup
// ============================================================================
// This configuration supports both local development and EC2 deployment:
//
// LOCAL DEVELOPMENT (appsettings.json):
// - Uses the "AWS" section in appsettings.json
// - Configure "Profile" to use a named profile from ~/.aws/credentials
// - Or configure "AccessKey" and "SecretKey" directly (not recommended)
//
// EC2 DEPLOYMENT (IAM Instance Profile):
// - When deployed to EC2 with an IAM Role attached, the SDK automatically
//   detects and uses the instance profile credentials
// - No additional configuration needed - just remove or leave empty the
//   "Profile", "AccessKey", and "SecretKey" settings
// - The SDK's credential chain will automatically find IAM role credentials
//
// Credential Resolution Order:
// 1. Explicit credentials in code (not used here)
// 2. Environment variables (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY)
// 3. AWS credentials file (~/.aws/credentials) with profile
// 4. EC2 Instance Profile / IAM Role (automatic on EC2)
// ============================================================================

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// TODO: Register INotificationService implementation
// builder.Services.AddScoped<INotificationService, AwsNotificationService>();
// builder.Services.AddAWSService<IAmazonSimpleEmailService>();
// builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

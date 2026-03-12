using CloudContactManager.Data;
using CloudContactManager.Services;
using CloudContactManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================================
// Notification Service Registration
// ============================================================================
// LOCAL  → LocalNotificationService  (no AWS needed, logs to console)
// AWS    → AwsNotificationService    (requires AWS credentials)
// Switch by checking if AWS credentials are available
// ============================================================================

var awsProfile = builder.Configuration.GetSection("AWS")["Profile"];
var hasAwsEnvVars = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"));
var hasAwsProfile = false;

if (!string.IsNullOrEmpty(awsProfile))
{
    var credFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".aws", "credentials");
    hasAwsProfile = File.Exists(credFile);
}

if (hasAwsEnvVars || hasAwsProfile)
{
    // AWS credentials found → use real AWS services
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>();
    builder.Services.AddAWSService<Amazon.SimpleEmail.IAmazonSimpleEmailService>();
    builder.Services.AddScoped<INotificationService, AwsNotificationService>();
    Console.WriteLine("✅ Using AWS Notification Service (SES/SNS)");
}
else
{
    // No AWS credentials → use local simulation
    builder.Services.AddScoped<INotificationService, LocalNotificationService>();
    Console.WriteLine("✅ Using Local Notification Service (console simulation)");
}

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

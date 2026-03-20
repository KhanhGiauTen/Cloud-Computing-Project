using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.Services;
using CloudContactManager.Services.API;
using CloudContactManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// API-only backend: we use controllers with attribute routing and no Razor views.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddSingleton<IPasswordHasher<Tenant>, PasswordHasher<Tenant>>();
// SpeedSMS client (SDK-style) and HTTP client support for SMS API
builder.Services.AddSingleton<Speedsmsapi>();
builder.Services.AddHttpClient();

// Allow cross-origin calls from external UI (SPA/static HTML)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

// Configure JWT authentication so APIs like /api/auth/login and /api/auth/register can issue and validate tokens
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

string NormalizeSqlServerConnectionString(string connectionString)
{
    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        var index = part.IndexOf('=');
        if (index <= 0)
        {
            continue;
        }

        var key = part[..index].Trim();
        var value = part[(index + 1)..].Trim();
        values[key] = value;
    }

    if (!values.ContainsKey("Server") && values.TryGetValue("Host", out var host))
    {
        values["Server"] = host;
        values.Remove("Host");
    }

    if (values.TryGetValue("Port", out var port) && values.TryGetValue("Server", out var server))
    {
        if (!server.Contains(','))
        {
            values["Server"] = $"{server},{port}";
        }
        values.Remove("Port");
    }

    if (values.TryGetValue("User", out var user) && !values.ContainsKey("User Id"))
    {
        values["User Id"] = user;
        values.Remove("User");
    }

    return string.Join(';', values.Select(kvp => $"{kvp.Key}={kvp.Value}")) + ';';
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseMySql(defaultConnection, ServerVersion.AutoDetect(defaultConnection));
    }
    else
    {
        var sqlServerConnection = NormalizeSqlServerConnectionString(defaultConnection);
        options.UseSqlServer(sqlServerConnection);
    }
});

// ============================================================================
// Notification Service Registration
// ============================================================================
// Use AWS-backed notification service. Clients are created with the default
// AWS credential/region resolution (env vars, IAM role, shared credentials,
// etc.), so we avoid calling GetAWSOptions (which does not match the
// installed AWSSDK.Extensions.NETCore.Setup version).
// ============================================================================

builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient());
builder.Services.AddSingleton<IAmazonSimpleEmailService>(_ => new AmazonSimpleEmailServiceClient());

// Use environment-based notification services, following V4 logic.
if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
{
	// Production/Staging: real email (AWS SES) and SMS (SpeedSMS HTTP API)
	builder.Services.AddScoped<INotificationService, SpeedSmsNotificationService>();
	Console.WriteLine("Using AWS SES for Email and Speed SMS for SMS");
}
else
{
	// Development: local simulation via console/logs
	builder.Services.AddScoped<INotificationService, LocalNotificationService>();
	Console.WriteLine("Using Local Notification Service (console simulation)");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Tenant>>();

    if (!db.SubscriptionPlans.Any(p => p.PlanName == "Free"))
    {
        db.SubscriptionPlans.Add(new SubscriptionPlan
        {
            PlanName = "Free",
            MaxCustomers = 100,
            Price = 0m
        });
        db.SaveChanges();
    }

    if (!db.Tenants.Any(t => t.Email == "demo@cloudcontact.local"))
    {
        var freePlanId = db.SubscriptionPlans
            .Where(p => p.PlanName == "Free")
            .Select(p => p.Id)
            .First();

        var demoTenant = new Tenant
        {
            PlanId = freePlanId,
            CompanyName = "Demo Company",
            Email = "demo@cloudcontact.local",
            CreatedAt = DateTime.UtcNow
        };

        demoTenant.PasswordHash = hasher.HashPassword(demoTenant, "Demo@123");
        db.Tenants.Add(demoTenant);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS for external UI calling this API from another origin
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Use attribute routing for APIs (e.g., /api/auth/login, /api/auth/register)
app.MapControllers();

app.Run();



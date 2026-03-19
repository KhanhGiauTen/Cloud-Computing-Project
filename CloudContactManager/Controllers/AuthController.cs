using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Tenant> _passwordHasher;

        public AuthController(AppDbContext context, IPasswordHasher<Tenant> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid sign-up payload." });
            }

            var exists = await _context.Tenants.AnyAsync(t => t.Email == request.Email);
            if (exists)
            {
                return Conflict(new { Success = false, Message = "Email is already registered." });
            }

            var freePlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanName == "Free");
            if (freePlan is null)
            {
                freePlan = new SubscriptionPlan
                {
                    PlanName = "Free",
                    MaxCustomers = 100,
                    Price = 0
                };
                _context.SubscriptionPlans.Add(freePlan);
                await _context.SaveChangesAsync();
            }

            var tenant = new Tenant
            {
                PlanId = freePlan.Id,
                CompanyName = request.CompanyName,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            tenant.PasswordHash = _passwordHasher.HashPassword(tenant, request.Password);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return Json(new
            {
                Success = true,
                Message = "Sign-up successful.",
                TenantId = tenant.Id,
                Plan = freePlan.PlanName
            });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid login payload." });
            }

            // Allow users to login using email (recommended) or company name as a simple username.
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == request.Username || t.CompanyName == request.Username);

            if (tenant is null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid username or password." });
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(tenant, tenant.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { Success = false, Message = "Invalid username or password." });
            }

            var token = GenerateJwtToken(tenant);

            return Json(new
            {
                Success = true,
                Message = "Login successful.",
                Token = token
            });
        }

        private string GenerateJwtToken(Tenant tenant)
        {
            // Read JWT configuration from appsettings.json using DI from HttpContext.
            var configuration = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            if (configuration is null)
            {
                throw new InvalidOperationException("IConfiguration is not available.");
            }

            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var issuer = jwtSection["Issuer"] ?? string.Empty;
            var audience = jwtSection["Audience"] ?? string.Empty;
            var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var mins) ? mins : 60;

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, tenant.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, tenant.Email),
                new System.Security.Claims.Claim("tenant_id", tenant.Id.ToString())
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

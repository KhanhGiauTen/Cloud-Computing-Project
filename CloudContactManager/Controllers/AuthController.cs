using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Tenant> _passwordHasher;

        public AuthController(AppDbContext context, IPasswordHasher<Tenant> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
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
    }
}

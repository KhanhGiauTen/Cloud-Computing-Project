using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<CustomersController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.UtcNow;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                // Send welcome email to the new customer
                var emailSubject = "Welcome to CloudContactManager!";
                var emailBody = $@"
                    <h1>Welcome, {customer.FullName}!</h1>
                    <p>Thank you for joining CloudContactManager.</p>
                    <p>Your account has been successfully created.</p>
                    <p>Best regards,<br/>The CloudContactManager Team</p>";

                var emailSent = await _notificationService.SendEmailAsync(
                    customer.Email, 
                    emailSubject, 
                    emailBody);

                if (emailSent)
                {
                    TempData["SuccessMessage"] = $"Customer '{customer.FullName}' created successfully. Welcome email sent!";
                }
                else
                {
                    TempData["WarningMessage"] = $"Customer '{customer.FullName}' created successfully, but welcome email could not be sent (AWS SES Sandbox limitation).";
                }

                _logger.LogInformation("Customer created: {CustomerName} ({CustomerId})", 
                    customer.FullName, customer.Id);

                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,Address,CreatedAt")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Customer '{customer.FullName}' updated successfully.";
                    _logger.LogInformation("Customer updated: {CustomerName} ({CustomerId})", 
                        customer.FullName, customer.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                var customerName = customer.FullName;
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Customer '{customerName}' deleted successfully.";
                _logger.LogInformation("Customer deleted: {CustomerName} ({CustomerId})", 
                    customerName, id);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Customers/SendSms/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSms(int id, string message)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var smsSent = await _notificationService.SendSmsAsync(customer.PhoneNumber, message);

            if (smsSent)
            {
                TempData["SuccessMessage"] = $"SMS sent successfully to {customer.FullName}.";
            }
            else
            {
                TempData["WarningMessage"] = $"SMS could not be sent to {customer.FullName} (AWS SNS Sandbox limitation).";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}

using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Controller for Customer CRUD operations.
    /// NOTE: INotificationService dependency is commented out until implementation is registered in Program.cs
    /// </summary>
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        // TODO: Uncomment when INotificationService implementation is registered
        // private readonly INotificationService _notificationService;

        public CustomersController(AppDbContext context /*, INotificationService notificationService*/)
        {
            _context = context;
            // _notificationService = notificationService;
        }

        // GET: Customers

        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            // TODO: Return create customer form view
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // TODO: Validate and save new customer to database
            // TODO: Call _notificationService.SendEmailAsync() to send welcome email
            if (ModelState.IsValid)
            {
                _context.Add(customer) ;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }    
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // TODO: Retrieve customer by id and return edit form view
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            // TODO: Validate and update customer in database
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
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
            // TODO: Retrieve customer by id and return delete confirmation view
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();
            return View();
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // TODO: Delete customer from database
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}

using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Controller for Customer CRUD operations.
    /// </summary>
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CustomersController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Customers
        public IActionResult Index()
        {
            // TODO: Retrieve all customers from database and return to view
            return View();
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
        public IActionResult Create(Customer customer)
        {
            // TODO: Validate and save new customer to database
            return View(customer);
        }

        // GET: Customers/Edit/5
        public IActionResult Edit(int id)
        {
            // TODO: Retrieve customer by id and return edit form view
            return View();
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Customer customer)
        {
            // TODO: Validate and update customer in database
            return View(customer);
        }

        // GET: Customers/Delete/5
        public IActionResult Delete(int id)
        {
            // TODO: Retrieve customer by id and return delete confirmation view
            return View();
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // TODO: Delete customer from database
            return RedirectToAction(nameof(Index));
        }
    }
}

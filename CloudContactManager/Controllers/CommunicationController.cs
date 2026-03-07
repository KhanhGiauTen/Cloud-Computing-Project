using CloudContactManager.Data;
using CloudContactManager.Services.Interfaces;
using CloudContactManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Controller for bulk communication operations (SMS/Email to multiple customers).
    /// </summary>
    public class CommunicationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _context;

        public CommunicationController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Communication
        // Displays list of customers with checkboxes to select multiple people
        public IActionResult Index()
        {
            var customers = _context.Customers.OrderBy(c => c.FullName).ToList();
            return View(customers);
        }

        // POST: Communication/Send
        // Receives selected customer IDs and sends communication via INotificationService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(CommunicationRequest request)
        {
            // 1 valid request
            if (request.CustomerIds == null || !request.CustomerIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one customer.");
                return View("Index", _context.Customers.ToList());
            }

            // 2. Get email or Phone number
            var recipients = _context.Customers
                .Where(c => request.CustomerIds.Contains(c.Id))
                .Select(c => request.Type == "SMS" ? c.PhoneNumber : c.EmailAddress)
                .ToList();

            // 3. bulk shipping service
            try
            {
                await _notificationService.SendBulkAsync(recipients, request.MessageContent, request.Type);
                TempData["Success"] = $"Sent {request.Type} successfully to {recipients.Count} receiver.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while sending the notification.: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using CloudContactManager.Data;
using CloudContactManager.Services.Interfaces;
using CloudContactManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Controller for bulk communication operations (SMS/Email to multiple customers).
    /// </summary>
    public class CommunicationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _context;
        // TODO: Uncomment when INotificationService implementation is registered
        private readonly INotificationService _notificationService;

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
            if (request.CustomerIds == null || !request.CustomerIds.Any())
            {
                TempData["Error"] = "Please choose at least one customer.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(request.MessageContent))
            {
                TempData["Error"] = "Message context can not be blank.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Take customer info from DB based on list of IDs sent up
                var selectedCustomers = await _context.Customers
                    .Where(c => request.CustomerIds.Contains(c.Id))
                    .ToListAsync();

                //List of receivers (Email or SMS)
                List<string> recipients = new List<string>();

                if (request.Type == "email")
                {
                    // Take EmailAddress column
                    recipients = selectedCustomers
                        .Where(c => !string.IsNullOrEmpty(c.EmailAddress))
                        .Select(c => c.EmailAddress)
                        .ToList();
                }
                else if (request.Type == "SMS")
                {
                    // sms implenment later
                }

                // Call bulk sending service
                if (recipients.Any())
                {
                    // await _notificationService.SendBulkAsync(recipients, request.MessageContent, request.Type);
                    // TempData["Success"] = $"Sent for {recipients.Count} people.";
                }
                else
                {
                    TempData["Warning"] = "No valid email address found in the list.";
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                TempData["Error"] = "Error occur: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using CloudContactManager.Data;
using CloudContactManager.Services.Interfaces;
using CloudContactManager.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Controller for bulk communication operations (SMS/Email to multiple customers).
    /// </summary>
    public class CommunicationController : Controller
    {
        private readonly AppDbContext _context;
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
            // TODO: Retrieve all customers from database
            // TODO: Return view with customer list for selection
            return View();
        }

        // POST: Communication/Send
        // Receives selected customer IDs and sends communication via INotificationService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(CommunicationRequest request)
        {
            // TODO: Validate request
            // TODO: Retrieve selected customers from database using request.CustomerIds
            // TODO: Extract recipient addresses (email or phone) based on request.Type
            // TODO: Call _notificationService.SendBulkAsync(recipients, request.MessageContent, request.Type)
            // TODO: Return result to user

            return RedirectToAction(nameof(Index));
        }
    }
}

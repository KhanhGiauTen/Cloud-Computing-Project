using System.ComponentModel.DataAnnotations;

namespace CloudContactManager.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        public int PlanId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SubscriptionPlan? Plan { get; set; }

        public ICollection<Customer> Customers { get; set; } = new List<Customer>();

        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}

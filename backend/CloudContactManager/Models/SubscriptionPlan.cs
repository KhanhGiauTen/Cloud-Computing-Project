using System.ComponentModel.DataAnnotations;

namespace CloudContactManager.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        public int MaxCustomers { get; set; }

        public decimal Price { get; set; }

        public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}

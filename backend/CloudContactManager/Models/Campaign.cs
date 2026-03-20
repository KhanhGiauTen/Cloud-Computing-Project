using System.ComponentModel.DataAnnotations;

namespace CloudContactManager.Models
{
    public class Campaign
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CommunicationType { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Tenant? Tenant { get; set; }

        public ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();
    }
}

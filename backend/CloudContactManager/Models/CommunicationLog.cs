namespace CloudContactManager.Models
{
    public class CommunicationLog
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public int CustomerId { get; set; }

        public string DeliveryStatus { get; set; } = string.Empty;

        public string? ExternalId { get; set; }

        public string? ErrorMessage { get; set; }

        public Campaign? Campaign { get; set; }

        public Customer? Customer { get; set; }
    }
}

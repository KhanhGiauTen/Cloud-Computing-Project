namespace CloudContactManager.Models
{
    /// <summary>
    /// Customer entity model.
    /// Properties match assignment requirements: Id, FullName, Address, PhoneNumber, EmailAddress.
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}

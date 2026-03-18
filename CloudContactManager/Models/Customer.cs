using System.ComponentModel.DataAnnotations;
namespace CloudContactManager.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ")]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = string.Empty;

        public string? Address { get; set; }

        public Tenant? Tenant { get; set; }

        public ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();
    }
}
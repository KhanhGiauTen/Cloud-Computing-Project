using System.ComponentModel.DataAnnotations;

namespace CloudContactManager.ViewModels
{
    /// <summary>
    /// Request payload for login API.
    /// </summary>
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

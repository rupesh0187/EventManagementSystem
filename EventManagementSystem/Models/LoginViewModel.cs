using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models
{
    public class LoginViewModel
    {
        public int id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; internal set; }
    }
}

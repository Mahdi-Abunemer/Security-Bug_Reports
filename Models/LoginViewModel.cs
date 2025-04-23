using System.ComponentModel.DataAnnotations;

namespace Security_Bug_Reports.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

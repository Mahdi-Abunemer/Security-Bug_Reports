using System.ComponentModel.DataAnnotations;

namespace Security_Bug_Reports.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }
    }
}

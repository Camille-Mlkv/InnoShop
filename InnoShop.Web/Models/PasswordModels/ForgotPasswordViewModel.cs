using System.ComponentModel.DataAnnotations;

namespace InnoShop.Web.Models.PasswordModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

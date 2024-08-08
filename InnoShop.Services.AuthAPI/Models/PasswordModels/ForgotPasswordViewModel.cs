using System.ComponentModel.DataAnnotations;

namespace InnoShop.Services.AuthAPI.Models.PasswordModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

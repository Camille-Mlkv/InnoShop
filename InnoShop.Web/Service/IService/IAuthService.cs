using InnoShop.Web.Models;
using InnoShop.Web.Models.PasswordModels;

namespace InnoShop.Web.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<ResponseDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

        Task<ResponseDTO?> ForgotPasswordAsync(ForgotPasswordViewModel forgotPasswordViewModel);
        Task<ResponseDTO?> ResetPasswordAsync(ResetPasswordViewModel model);
    }
}

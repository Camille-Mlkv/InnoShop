using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;

namespace InnoShop.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDTO registrationRequestDTO);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel);
        Task<ResponseDTO> ResetPassword(ResetPasswordViewModel model);
    }
}

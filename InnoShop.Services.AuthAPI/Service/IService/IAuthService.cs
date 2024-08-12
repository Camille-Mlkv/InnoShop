using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;

namespace InnoShop.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDTO registrationRequestDTO);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        Task<string> ConfirmAccount(string userId);
        Task ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel);
        Task<string> ResetPassword(string userId);
        Task<ResponseDTO> SavePassword(ResetPasswordViewModel model);

        Task<ResponseDTO> UpdateUserAsync(string id, UpdateUserDTO model);
        Task<ResponseDTO> DeleteUserAsync(string id);
    }
}

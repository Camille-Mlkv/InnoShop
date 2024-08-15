using InnoShop.Web.Models;
using InnoShop.Web.Models.PasswordModels;
using InnoShop.Web.Service.IService;
using InnoShop.Web.Utility;

namespace InnoShop.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;

        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/login"

            }, withBearer: false);
        }

        public async Task<ResponseDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data= registrationRequestDTO,
                Url=SD.AuthAPIBase+"/api/auth/register"

            }, withBearer: false);
        }


        public async Task<ResponseDTO?> ForgotPasswordAsync(ForgotPasswordViewModel obj)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url=SD.AuthAPIBase+"/api/auth/ForgotPassword"
            });
        }


        public async Task<ResponseDTO?> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = model,
                Url = SD.AuthAPIBase + "/api/auth/SaveNewPassword"
            });
        }
    }
}

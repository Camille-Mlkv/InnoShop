using InnoShop.Services.AuthAPI.Models;

namespace InnoShop.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser);
    }
}

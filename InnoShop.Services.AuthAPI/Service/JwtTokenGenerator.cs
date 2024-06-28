using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Service.IService;
using System.Runtime.CompilerServices;

namespace InnoShop.Services.AuthAPI.Service
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(JwtOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }
        public string GenerateToken(ApplicationUser applicationUser)
        {
            throw new NotImplementedException();
        }
    }
}

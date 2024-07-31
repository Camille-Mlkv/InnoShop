using InnoShop.Services.AuthAPI.Data;
using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace InnoShop.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;

            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }
        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user=_db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower()==loginRequestDTO.UserName.ToLower());
            bool isValid=await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            bool isAccountConfirmed=await _userManager.IsEmailConfirmedAsync(user);

            if(user==null || !isValid || !isAccountConfirmed)
            {
                return new LoginResponseDTO() { User = null, Token = "" };
            }

            //if user was found, generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(user);


            UserDTO userDTO = new()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
            };

            LoginResponseDTO loginResponseDTO = new()
            {
                User = userDTO,
                Token = token,
            };
            return loginResponseDTO;
        }

        public async Task<string> Register(RegistrationRequestDTO registrationRequestDTO) //add confirmation
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.Email,
                Email = registrationRequestDTO.Email,
                NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                PhoneNumber = registrationRequestDTO.PhoneNumber,
                Name = registrationRequestDTO.Name,
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDTO.Email);
                    UserDTO userDTO = new()
                    {
                        Email = userToReturn.Email,
                        Id = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber,
                    };

                    // Generate email confirmation token
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Construct callback URL
                    var actionContext = new ActionContext(
                        _httpContextAccessor.HttpContext,
                        _httpContextAccessor.HttpContext.GetRouteData(),
                        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

                    var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                    var callbackUrl = urlHelper.Action(
                        "ConfirmEmail",
                        "AuthAPI",
                        new { userId = user.Id, code = code },
                        protocol: _httpContextAccessor.HttpContext.Request.Scheme);

                    // Преобразуем локальный URL в публичный URL ngrok
                    callbackUrl = callbackUrl.Replace("https://localhost:7001", "https://92c1-158-181-40-255.ngrok-free.app");

                    // Send email
                    await _emailService.SendEmailAsync(registrationRequestDTO.Email, "Confirm your account",
                        $"Confrim your account on InnoShop through this link: <a href='{callbackUrl}'>Here</a>");
                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }


            }
            catch (Exception ex)
            {

            }
            return "Error occured";
        }
    }
}

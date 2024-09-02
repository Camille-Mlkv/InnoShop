using InnoShop.Services.AuthAPI.Data;
using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;
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
            if (user==null)
            {
                return new LoginResponseDTO() { User = null, Token = "" };
            }
            bool isValid=await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            bool isAccountConfirmed=await _userManager.IsEmailConfirmedAsync(user);

            if(!isValid || !isAccountConfirmed)
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
                        new { userId = user.Id },
                        protocol: _httpContextAccessor.HttpContext.Request.Scheme);

                    callbackUrl = callbackUrl.Replace("https://localhost:7001", "https://ed63-212-47-148-183.ngrok-free.app");
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

        public async Task<string> ConfirmAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "Error, user is null.";
            }

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (code == null)
            {
                return "Error, code is null.";
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return "";
            }

            else return "Unhandled Error";
        }


        public async Task ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return; //we don't show that the user doesn't exist
            }

            var actionContext = new ActionContext(
                _httpContextAccessor.HttpContext,
                _httpContextAccessor.HttpContext.GetRouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var callbackUrl = urlHelper.Action(
                "ResetPassword",
                "AuthAPI",
                new { userId = user.Id },
                protocol: _httpContextAccessor.HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(model.Email, "Reset Password",
                $"To reset the password follow the link: <a href='{callbackUrl}'>link</a> . Here you will find the password reset code.");
        }

        public async Task<string> ResetPassword(string userId) 
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "Error";
            }
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            return code;
        }

        public async Task<ResponseDTO> SavePassword(ResetPasswordViewModel model)
        {
            ResponseDTO response=new ResponseDTO();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                response.IsSuccess = true;
                response.Message = "User is null";
                return response;
            }
            var resetResult=await _userManager.ResetPasswordAsync(user,model.Code,model.Password);
            if (resetResult.Succeeded)
            {
                response.IsSuccess = true;
                response.Message = "Password successfully reset";
                return response;
            }
            response.IsSuccess = false;
            response.Message = "Error saving password";
            return response;
            
        }

        public async Task<ResponseDTO> UpdateUserAsync(string id, UpdateUserDTO model)
        {
            var response=new ResponseDTO();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User not found";
                return response;
            }
            if (model.Email != null && model.Email != user.Email)
            {
                var emailInUse = await _userManager.FindByEmailAsync(model.Email);
                if (emailInUse != null)
                {
                    response.IsSuccess = false;
                    response.Message="Email is already in use";
                    return response;
                }

                user.Email = model.Email;
                user.EmailConfirmed = false;

                // Construct callback URL
                var actionContext = new ActionContext(
                    _httpContextAccessor.HttpContext,
                    _httpContextAccessor.HttpContext.GetRouteData(),
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

                var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                var callbackUrl = urlHelper.Action(
                    "ConfirmEmail",
                    "AuthAPI",
                    new { userId = user.Id },
                    protocol: _httpContextAccessor.HttpContext.Request.Scheme);

                // Send email
                await _emailService.SendEmailAsync(model.Email, "Confirm your account",
                    $"Confrim your account on InnoShop through this link: <a href='{callbackUrl}'>Here</a>");
            }

            user.Name = model.Name ?? user.Name;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.UserName=model.Email?? user.Email;
            user.NormalizedEmail = model.Email.ToUpper() ?? user.Email.ToUpper();

            var result=await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.IsSuccess = false;
                response.Message = "An unhandled error occured";
                return response;
            }

            response.IsSuccess = true;
            response.Message = "User successfully updated";
            return response;
        }


        public async Task<ResponseDTO> DeleteUserAsync(string id)
        {
            var response=new ResponseDTO();
            var user=await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.IsSuccess=false;
                response.Message = "User doesn't exist";
                return response;
            }

            var result=await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                response.IsSuccess=true;
                response.Message = "Successfully deleted";
                return response;
            }
            response.IsSuccess = false;
            response.Message = "Failed to delete user";
            return response;
        }
    }
}

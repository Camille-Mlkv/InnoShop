using InnoShop.Services.AuthAPI.Models;
using InnoShop.Services.AuthAPI.Models.DTO;
using InnoShop.Services.AuthAPI.Models.PasswordModels;
using InnoShop.Services.AuthAPI.Service;
using InnoShop.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace InnoShop.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDTO _response;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthAPIController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _response = new();
            _userManager= userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse=await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Credentials are incorrect or account is not confirmed.";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpGet("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return Ok("Your email has been confirmed, thanks");
            }
                
            else
                return BadRequest("Error");
            
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _authService.ForgotPassword(model); //sending email
                _response.IsSuccess = true;
                _response.Message = "Okay";
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.Message = "Not fine";
            return BadRequest(_response);
        }

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            var userEmail = (await _userManager.FindByIdAsync(userId)).Email;
            var callbackUrl = $"https://localhost:7271/Auth/ResetPassword?userId={userId}&code={Uri.EscapeDataString(code)}&email={Uri.EscapeDataString(userEmail)}";
            return Redirect(callbackUrl);
        }

        [HttpPost("SaveNewPassword")]
        public async Task<IActionResult> SaveNewPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ResponseDTO response=await _authService.ResetPassword(model);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response);
            }
            return BadRequest();    
        }

    }
}

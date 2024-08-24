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
using Microsoft.IdentityModel.Tokens;

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
            _userManager = userManager;
        }


        //create
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
        public async Task<IActionResult> ConfirmEmail(string userId)
        {
            string result=await _authService.ConfirmAccount(userId);
            if (result.IsNullOrEmpty())
            {
                return Ok("Email is confirmed");
            }
            else
            {
                return BadRequest(result); 
            }
            
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _authService.ForgotPassword(model); //sending email
                _response.IsSuccess = true;
                _response.Message = "Email sent";
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.Message = "An error occured";
            return BadRequest(_response);
        }

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var result=await _authService.ResetPassword(userId);
            if (result == "Error")
            {
                return BadRequest(result);
            }
            else
            {
                return Ok(result); //code is displayed
            }
        }

        [HttpPost("SaveNewPassword")]
        public async Task<IActionResult> SaveNewPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ResponseDTO response=await _authService.SavePassword(model);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response);
            }
            return BadRequest();    
        }

        //CRUD operations


        //Read
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        //Update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDTO model)
        {
            var response=await _authService.UpdateUserAsync(id, model);
            if (response.IsSuccess) {
                return Ok(response.Message);
            }
            else
            {
                return BadRequest(response.Message);
            }
        }


        //Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var response = await _authService.DeleteUserAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response.Message);
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

    }
}

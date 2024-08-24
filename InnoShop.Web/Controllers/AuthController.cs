using InnoShop.Web.Models;
using InnoShop.Web.Models.PasswordModels;
using InnoShop.Web.Service.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace InnoShop.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService,ITokenProvider tokenProvider) 
        { 
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDTO = new();
            return View(loginRequestDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            ResponseDTO responseDto = await _authService.LoginAsync(obj);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDTO loginResponseDto =
                    JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDto.Result));
                await SignInUser(loginResponseDto);
                _tokenProvider.SetToken(loginResponseDto.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = responseDto.Message;
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDTO obj)
        {
            ResponseDTO result=await _authService.RegisterAsync(obj);
            if (result != null && result.IsSuccess)
            {
                TempData["success"] = "A confirmation email has been sent to your adress!";
                return RedirectToAction(nameof(Login));

            }
            else
            {
                TempData["error"] = result.Message;
            }
            return View(obj);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDTO model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(model.Token);

            var identity= new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(u=>u.Type==JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            ResponseDTO result = await _authService.ForgotPasswordAsync(model);
            if (result != null && result.IsSuccess)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email = model.Email }); 
            }
            else
            {
                TempData["error"] = result.Message;
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation(string email=null)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordConfirmation(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ResponseDTO response = await _authService.ResetPasswordAsync(model);
            if (response != null && response.IsSuccess)
            {
                return View("ResetPasswordConfirmation");
            }
            else
            {
                TempData["error"] = response.Message;
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

    }
}

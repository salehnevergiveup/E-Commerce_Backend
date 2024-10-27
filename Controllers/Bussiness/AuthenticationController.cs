using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.Auth;
using PototoTrade.Enums;
using PototoTrade.ServiceBusiness.Authentication;

namespace PototoTrade.Controllers.Bussiness
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly Authentication _authService;

        public AuthenticationController(Authentication authService)
        {
            _authService = authService;
        }

        [HttpPost("public/login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var response = await _authService.LoginAsync(loginDto, [UserRolesEnum.User.ToString()]);

            if (response.Data.AccessToken == null || response.Data.RefreshToken == null)
            {
                return Unauthorized(new { message = "Invalide Username/Email or  Password" });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                IsEssential = true,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", response.Data.RefreshToken, cookieOptions);

            return Ok(new { AccessToken = response.Data.AccessToken });

        }

        [HttpPost("public/admin/login")]
        public async Task<IActionResult> LoginAdmin(LoginDTO loginDto)
        {

            var response = await _authService.LoginAsync(loginDto, [UserRolesEnum.SuperAdmin.ToString(), UserRolesEnum.Admin.ToString()]);

            if (response.Data.AccessToken == null || response.Data.RefreshToken == null)
            {
                return Unauthorized(new { message = "Invalide Username/Email or  Password" });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                IsEssential = true,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", response.Data.RefreshToken, cookieOptions);

            return Ok(new { AccessToken = response.Data.AccessToken });
        }

        [HttpPost("public/logout")]
        public async Task<IActionResult> Logout()
        {
            string refreshToken = Request.Cookies["refreshToken"];

            var response = await _authService.LogoutAsync(refreshToken);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1),
                IsEssential = true,
                Secure = true

            };

            Response.Cookies.Append("refreshToken", "", cookieOptions);


            return response.Success ? Ok(response) : BadRequest(response);

        }

        [HttpPost("public/refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO request)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var response = await _authService.RefreshTokenAsync(request.AccessToken, refreshToken);

            if (!response.Success)
            {
                Response.Cookies.Delete("refreshToken");
            }

            return response.Success ? Ok(new { AccessToken = response.Data }) : Unauthorized(response);
        }

        [HttpPost("public/register")]
        public async Task<IActionResult> Register(UserRegistrationDTO userRegistrationDto)
        {
            var response = await _authService.RegisterUserAsync(userRegistrationDto);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDTO changePasswordDto)
        {
            var response = await _authService.ChangePasswordAsync(changePasswordDto, User);

            return response.Success ? Ok(response) : BadRequest(response);
        }

    }

}

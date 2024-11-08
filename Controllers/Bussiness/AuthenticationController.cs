using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.Auth;
using PototoTrade.Enums;
using PototoTrade.ServiceBusiness.Authentication;

namespace PototoTrade.Controllers.Bussiness
{
    [Route("api/authentication/public")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly Authentication _authService;

        public AuthenticationController(Authentication authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var response = await _authService.LoginAsync(loginDto, [UserRolesEnum.User.ToString()]);

            if (response.Data.AccessToken == null || response.Data.RefreshToken == null)
            {
                return Unauthorized(response);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                IsEssential = true,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", response.Data.RefreshToken, cookieOptions);

            return Ok(new { Success = response.Success, Message = response.Message, Data = new { AccessToken = response.Data.AccessToken } });

        }

        [HttpPost("admin/login")]
        public async Task<IActionResult> LoginAdmin(LoginDTO loginDto)
        {

            var response = await _authService.LoginAsync(loginDto, [UserRolesEnum.SuperAdmin.ToString(), UserRolesEnum.Admin.ToString()]);

            if (response.Data.AccessToken == null || response.Data.RefreshToken == null)
            {
                return Unauthorized(response);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                IsEssential = true,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", response.Data.RefreshToken, cookieOptions);

            return Ok(new { Success = response.Success, Message = response.Message, Data = new { AccessToken = response.Data.AccessToken } });
        }

        [HttpPost("logout")]
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

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO request)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var response = await _authService.RefreshTokenAsync(request.AccessToken, refreshToken);

            if (!response.Success)
            {
                Response.Cookies.Delete("refreshToken");
            }

            return response.Success ? Ok(new { Success = response.Success, Message = response.Message, Data = new { AccessToken = response.Data } }) : Unauthorized(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDTO userRegistrationDto)
        {
            var response = await _authService.RegisterUserAsync(userRegistrationDto);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("/api/authentication/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDTO changePasswordDto)
        {
            var response = await _authService.ChangePasswordAsync(changePasswordDto, User);

            return response.Success ? Ok(response) : BadRequest(response);
        }

    }

}

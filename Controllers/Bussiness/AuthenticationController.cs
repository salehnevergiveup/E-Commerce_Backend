using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.Auth;
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

            var (accessToken, refreshToken) = await _authService.LoginAsync(loginDto);

            if (accessToken == null || refreshToken == null)
            {
                return Unauthorized(new {message = "Invalide Username/Email or  Password" }); 
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires =  loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                IsEssential = true 
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new { AccessToken = accessToken });
        }

        [HttpPost("public/logout")]
        public async Task<IActionResult> Logout()
        {
            string refreshToken = Request.Cookies["refreshToken"];

            var result  = await _authService.LogoutAsync(refreshToken);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1),
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", "", cookieOptions); 


            return result.IsSuccessful? Ok(new { message = result.Message })  :   BadRequest(new { message = result.Message });

        }


        [HttpPost("public/refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO request)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authService.RefreshTokenAsync( request.AccessToken , refreshToken);

            if (!result.IsSuccessful)
            {
                Response.Cookies.Delete("refreshToken");
                return Unauthorized(new { message = "Invalid access or refresh token." });
            }

            return result.IsSuccessful?  Ok(new { AccessToken = result.Message }) :  Unauthorized(new { message = result.Message});
        }

        [HttpPost("public/register")]
        public async Task<IActionResult> Register(UserRegistrationDTO userRegistrationDto)
        {
            var result = await _authService.RegisterUserAsync(userRegistrationDto);
            
            return result.IsSuccessful?  Ok(new { message = result.Message}) :  BadRequest(new { message = result.Message });  
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(UpdatePasswordDTO changePasswordDto)
        {
            var result = await _authService.ChangePasswordAsync(changePasswordDto, User);

            return result.IsSuccessful? Ok(new { message = result.Message }) :  BadRequest(new { message = result.Message});
        }

    }
    
}

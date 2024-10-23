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
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (accessToken, refreshToken) = await _authService.LoginAsync(loginDto);

            if (accessToken == null || refreshToken == null)
            {
                return Unauthorized(); 
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

            var logoutSuccess = await _authService.LogoutAsync(refreshToken);

            if (!logoutSuccess)
            {
                return BadRequest(new {message = "invalid request."});
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1),
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", "", cookieOptions); 

            return Ok("Logged out successfully.");
        }


        [HttpPost("public/refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody]RefreshTokenDTO request)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var newAccessToken = await _authService.RefreshTokenAsync( request.AccessToken , refreshToken);

            if (string.IsNullOrEmpty(newAccessToken))
            {
                Response.Cookies.Delete("refreshToken");
                return Unauthorized(new { message = "Invalid access or refresh token." });
            }

            return Ok(new { AccessToken = newAccessToken });
        }

        [HttpPost("public/register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDto)
        {
            if(!ModelState.IsValid)  return BadRequest(new 
            {
                message = "Registration failed.",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
     
            var result = await _authService.RegisterUserAsync(userRegistrationDto);
            
            return result?  Ok(new { message = "User registered successfully." }) :  BadRequest(new { message = "Registration failed. Please check your details." });  
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordDTO changePasswordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ChangePasswordAsync(changePasswordDto, User);

            if (!result) return BadRequest(new { message = "Password change failed. Please check your details." });

            return Ok(new { message = "Password changed successfully." });
        }

    }
    
}

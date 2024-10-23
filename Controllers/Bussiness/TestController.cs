using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PototoTrade.Data;

namespace PototoTrade.Controllers.Bussiness
{
      [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DBC _context; 
        private readonly  IConfiguration _configuration;  

        public  TestController(IConfiguration config, DBC context)  {  
            _configuration = config; 
            _context =  context; 

        } 
        // Public route  

        [HttpGet("something")]
        public IActionResult PublicTest() {  
            var dataChecking  =  this._context.UserAccounts.ToList();  
            return Ok(dataChecking);  
        }

        [HttpGet("private")]
        public IActionResult PrivateTest() {  

            return Ok("everything is working");
        }



        [HttpGet("public/generate_token")] 
        public IActionResult generateToken()  {  
    
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "saleh"),
                new(ClaimTypes.Role, "Admin") 
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                SecurityAlgorithms.HmacSha256));

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));  
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(3),
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

           return Ok(new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken =  refreshToken
            });
        
        } 

        [HttpGet("public/refresh")] 
        public IActionResult refreshToken(TokenRefreshRequestDto refreshRequest) {  
 
            string name = User.Identity.Name;  
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            string role = roleClaim != null ? roleClaim.Value : "No role assigned";

            string refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is missing");
            }

            // Generate new access token

             var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "saleh"),
                new(ClaimTypes.Role, "Admin") 
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                SecurityAlgorithms.HmacSha256));


            return Ok(new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token), 
                RefreshToken  = refreshToken,  
                Role  = role,  
                Name = name
            });
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false  ,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

    public class TokenRefreshRequestDto
    {
    }
}

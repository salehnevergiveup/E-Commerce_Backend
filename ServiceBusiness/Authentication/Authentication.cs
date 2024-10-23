using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PototoTrade.DTO.Auth;
using PototoTrade.Models.User;
using PototoTrade.Repository.Users;
using PototoTrade.Service.Utilites.Hash;

namespace PototoTrade.ServiceBusiness.Authentication
{
    public class Authentication
    {
         private readonly UserAccountRepository _userAccountRepo;  

        private readonly SessionRepository _SessionRepo;  
        private readonly IHashing _hashing;  

         private  readonly IConfiguration _configuration; 

        public Authentication(IConfiguration configuration, UserAccountRepository userAccountRepo, IHashing hashing, SessionRepository sessionRepo) {  
            _userAccountRepo  = userAccountRepo;  
            _hashing = hashing; 
            _configuration = configuration; 
            _SessionRepo = sessionRepo;
        }      
        public async Task<(string? AccessToken, string? RefreshToken)> LoginAsync(LoginDTO loginDto)
        {
            string username = loginDto.Username;
            string password = loginDto.Password;
            bool rememberMe = loginDto.RememberMe;

            var user = await _userAccountRepo.GetUserByUserNameAsync(username);

            if (user == null || !_hashing.Verify(user.PasswordHash, password)) return (null, null);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Id.ToString()), 
                new(ClaimTypes.Role, user.Role.RoleName),
                new("username", user.Username)
            };

            var accessToken = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), 
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            );

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshTokenEntity = new SessionDTO
            {
                UserId = user.Id,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                RefreshToken = refreshToken
            };

            await _SessionRepo.SetSession(refreshTokenEntity);

            return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false; 
            }

            var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

            if (storedRefreshToken != null && !storedRefreshToken.IsRevoked && storedRefreshToken.ExpiresAt > DateTime.UtcNow)
            {
                storedRefreshToken.IsRevoked = true;
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                await _SessionRepo.UpdateSessionAsync(storedRefreshToken);
                return true;
            }

            return false;
        }


        public async Task<string?> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return null; 
            }

            var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return null; 
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            
            if (!jwtHandler.CanReadToken(accessToken))
            {
                return null; 
            }

            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; 
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(usernameClaim) || string.IsNullOrEmpty(roleClaim))
            {
                return null; 
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userIdClaim), 
                new(ClaimTypes.Role, roleClaim),  
                new("username", usernameClaim)  
            };

            var newAccessToken = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(newAccessToken);
        }


        public async Task<bool> RegisterUserAsync(UserRegistrationDTO userRegistrationDto)
        {
            var existingUserUsername = await _userAccountRepo.GetUserByUserNameAsync(userRegistrationDto.Username);
            var existingUserEmail  =  await _userAccountRepo.GetUserByUserEmailAsync(userRegistrationDto.Email); 

            if (existingUserUsername != null || existingUserEmail != null)
            {
                return false;
            }

            var passwordHash = _hashing.Hash(userRegistrationDto.Password);

            var user = new UserAccount
            {
                Username = userRegistrationDto.Username,
                PasswordHash = passwordHash,
                RoleId = 3, 
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var userDetails = new UserDetail
            {
                Email = userRegistrationDto.Email,
                PhoneNumber = userRegistrationDto.PhoneNumber,
                BillingAddress = userRegistrationDto.BillingAddress,
                Age = userRegistrationDto.Age,
                Gender = userRegistrationDto.Gender,
            };

            await _userAccountRepo.AddUserWithDetailsAsync(user, userDetails);

            return true; 
        }


    public async Task<bool> ChangePasswordAsync(UpdatePasswordDTO changePasswordDto, ClaimsPrincipal userClaims)
    {
        var userIdClaim = userClaims.FindFirst(ClaimTypes.Name);

        if (userIdClaim == null) return false;

        int userId = int.Parse(userIdClaim.Value);
        
        var user = await _userAccountRepo.GetUserByIdAsync(userId);

        if (user == null) return false;

        if (!_hashing.Verify(user.PasswordHash, changePasswordDto.CurrentPassword)) return false;

        string newPasswordHash = _hashing.Hash(changePasswordDto.NewPassword);

        user.PasswordHash = newPasswordHash;

        await _userAccountRepo.UpdateUserPasswordAsync(user);

        return true;
    }

       
    }
}

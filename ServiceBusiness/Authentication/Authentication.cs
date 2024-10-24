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

        private JwtSecurityToken CreateAccessToken(UserAccount user) {  
                var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Id.ToString()), 
                new(ClaimTypes.Role, user.Role.RoleName),
            };
            return new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), 
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            );

        }

        private string  GenerateRefreshToken() {  
            return  Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }    
        
        public async Task<(string? AccessToken, string? RefreshToken)> LoginAsync(LoginDTO loginDto)
        {
            string emailOrUsername = loginDto.EmailOrUsername; 
            string password = loginDto.Password;
            bool rememberMe = loginDto.RememberMe;

            var user = await _userAccountRepo.GetUserByUserNameOrEmailAsync(emailOrUsername);

            if (user == null || !_hashing.Verify(user.PasswordHash, password)) return (null, null);
  
            var accessToken = this.CreateAccessToken(user);
         
            var refreshToken = this.GenerateRefreshToken();

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

        public async Task<(bool IsSuccessful, string Message)> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return (false, "Refresh Token Not Found"); 

            var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

            if (storedRefreshToken != null && !storedRefreshToken.IsRevoked && storedRefreshToken.ExpiresAt > DateTime.UtcNow)
            {
                storedRefreshToken.IsRevoked = true;
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                await _SessionRepo.UpdateSessionAsync(storedRefreshToken);
                return (true, "Logged Out Successfully");
            }

            return (false , "Something Went Wrong");
        }

        public async Task<(bool IsSuccessful, string Message)> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return (false,  "Invalide Refresh Token"); 

            var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return (false,  "Refreh Token Expired"); 
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            
            if (!jwtHandler.CanReadToken(accessToken)) return (false, "Invalide Access Token"); 

            var jwtToken = jwtHandler.ReadJwtToken(accessToken);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; 

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim)) return (false,  "User Details Not Found" ); 

            var user = await _userAccountRepo.GetUserByIdAsync(int.Parse(userIdClaim));

            if(user == null) return (false,  "User Not Found"); 

            var newAccessToken = this.CreateAccessToken(user);  

            return (true , new JwtSecurityTokenHandler().WriteToken(newAccessToken));
        }


        public async Task<(bool IsSuccessful, string Message)> RegisterUserAsync(UserRegistrationDTO userRegistrationDto)
        {
            var existingUserUsername = await _userAccountRepo.GetUserByUserNameOrEmailAsync(userRegistrationDto.Username);
            var existingUserEmail  =  await _userAccountRepo.GetUserByUserNameOrEmailAsync(userRegistrationDto.Email); 
            

            if (existingUserUsername != null)
            {
                return (false, "username is already exist");
            }

            if(existingUserEmail !=  null) {  

              return (false, "Email is already exist"); 
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

            return (true, "Account Created Successfully"); 
        }


    public async Task<(bool IsSuccessful, string Message)> ChangePasswordAsync(UpdatePasswordDTO changePasswordDto, ClaimsPrincipal userClaims)
    {
        var userIdClaim = userClaims.FindFirst(ClaimTypes.Name)?.Value;

        if (userIdClaim == null) return (false, "User not Found");

        var user = await _userAccountRepo.GetUserByIdAsync(int.Parse(userIdClaim));

        if (user == null) return  (false, "User not Found");

        if (!_hashing.Verify(user.PasswordHash, changePasswordDto.CurrentPassword)) return  (false, "Old password is not correct");

        string newPasswordHash = _hashing.Hash(changePasswordDto.NewPassword);

        user.PasswordHash = newPasswordHash;

        await _userAccountRepo.UpdateUserPasswordAsync(user);

        return (true, "Password Changed Successfully");
    }

       
    }
}

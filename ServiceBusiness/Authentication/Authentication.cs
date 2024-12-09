using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PototoTrade.Data;
using PototoTrade.DTO.Auth;
using PototoTrade.Models.User;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.Role;
using PototoTrade.Repository.Users;
using PototoTrade.Repository.Wallet;
using PototoTrade.Service.Utilites.Hash;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.ServiceBusiness.Authentication
{
    public class Authentication
    {
        private readonly UserAccountRepository _userAccountRepo;
        private readonly UserDetailsRepository _userDetails;
        private readonly RoleRepository _roleRepository;

        private readonly MediaRepository _mediaReposiotry;

        private readonly SessionRepository _SessionRepo;
        private readonly IHashing _hashing;

        private readonly WalletRepository _walletRepo;

        private readonly DBC _context;

        private readonly IConfiguration _configuration;

        public Authentication(IConfiguration configuration, MediaRepository mediaReposiotry, UserAccountRepository userAccountRepo, IHashing hashing, SessionRepository sessionRepo, UserDetailsRepository userDetails, DBC bC, RoleRepository roleRepository, WalletRepository walletRepo)
        {
            _mediaReposiotry = mediaReposiotry;
            _userAccountRepo = userAccountRepo;
            _hashing = hashing;
            _configuration = configuration;
            _SessionRepo = sessionRepo;
            _userDetails = userDetails;
            _context = bC;
            _roleRepository = roleRepository;
            _walletRepo = walletRepo;
        }

        private async Task<JwtSecurityToken> CreateAccessToken(UserAccount user)
        {

            var roleInfo = await _roleRepository.GetRoleAsync(user.RoleId);
            var roleType = roleInfo?.RoleType ?? "DefaultRole";

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Id.ToString()),
                new(ClaimTypes.Role, roleType),
            };

            var permissions = roleInfo.AdminPermissions.FirstOrDefault(p => p.RoleId == roleInfo.Id);

            if (permissions != null)
            {
                if (permissions.CanCreate)
                    claims.Add(new Claim("Permission", "CanCreate"));
                if (permissions.CanEdit)
                    claims.Add(new Claim("Permission", "CanEdit"));
                if (permissions.CanDelete)
                    claims.Add(new Claim("Permission", "CanDelete"));
                if (permissions.CanView)
                    claims.Add(new Claim("Permission", "CanView"));
            }

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

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<ResponseModel<(string? AccessToken, string? RefreshToken)>> LoginAsync(LoginDTO loginDto, string[] roles)
        {
            var response = new ResponseModel<(string? AccessToken, string? RefreshToken)>
            {
                Success = false,
                Data = ("", ""),
                Message = "Login not allowed"
            };
            try
            {
                string emailOrUsername = loginDto.EmailOrUsername;
                string password = loginDto.Password;
                bool rememberMe = loginDto.RememberMe;

                var user = await _userAccountRepo.GetUserByUserNameOrEmailAsync(emailOrUsername);

                if (user == null)
                {
                    response.Message = "User not found.";
                    return response;
                }

                if (user.Status != "Active")
                {
                    response.Message = "You are not allowed to login the current acount is " + user.Status;
                    return response;
                }

                var isPasswordValid = _hashing.Verify(user.PasswordHash, password);

                var isUserOrAdmin = roles.Contains(user.Role.RoleType);

                if (!isPasswordValid || !isUserOrAdmin)
                {
                    response.Message = "Invalid credentials or insufficient permissions.";
                    return response;
                }

                var accessToken = await this.CreateAccessToken(user);
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

                response.Success = true;
                response.Data = (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken);
                response.Message = "Login successful.";

                return response;
            }
            catch (Exception e)
            {
                response.Message = "Unable to Login The User";
                return response;
            }
        }

        public async Task<ResponseModel<bool>> LogoutAsync(string refreshToken)
        {

            var response = new ResponseModel<bool> { Success = false, Data = false };
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    response.Message = "Refresh Token Not Found";
                    return response;
                }

                var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

                if (storedRefreshToken != null)
                {
                    storedRefreshToken.IsRevoked = true;
                    storedRefreshToken.RevokedAt = DateTime.UtcNow;
                    await _SessionRepo.UpdateSessionAsync(storedRefreshToken);
                    response.Success = true;
                    response.Data = true; // Indicate that the logout was successful.
                    response.Message = "Logged Out Successfully";

                    return response;
                }

                response.Message = "Something Went Wrong";
                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong";
                return response;
            }
        }

        public async Task<ResponseModel<string>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var response = new ResponseModel<string>
            {
                Success = false,
                Data = null
            };
            try
            {

                if (string.IsNullOrEmpty(refreshToken))
                {
                    response.Message = "Invalid Refresh Token";
                    return response;
                }
                var storedRefreshToken = await _SessionRepo.GetSessionByRefreshTokenAsync(refreshToken);

                if (storedRefreshToken == null || storedRefreshToken.IsRevoked)
                {
                    response.Message = "Refresh Token Expired";
                    return response;
                }

                //if the session expired revoke it
                if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
                {
                    storedRefreshToken.IsRevoked = true;
                    storedRefreshToken.RevokedAt = DateTime.UtcNow;
                    await _SessionRepo.UpdateSessionAsync(storedRefreshToken);
                    response.Message = "Refresh Token Expired";
                    return response;
                }

                var jwtHandler = new JwtSecurityTokenHandler();

                if (!jwtHandler.CanReadToken(accessToken))
                {
                    response.Message = "Invalid Access Token";
                    return response;
                }
                var jwtToken = jwtHandler.ReadJwtToken(accessToken);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
                {
                    response.Message = "User Details Not Found";
                    return response;
                }

                var user = await _userAccountRepo.GetUserByIdAsync(int.Parse(userIdClaim));

                if (user == null)
                {
                    response.Message = "User Not Found";
                    return response;
                }

                var newAccessToken = await this.CreateAccessToken(user);
                response.Success = true;
                response.Data = new JwtSecurityTokenHandler().WriteToken(newAccessToken);
                response.Message = "Token refreshed successfully.";

                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong";
                return response;
            }
        }

        public async Task<ResponseModel<string>> RegisterUserAsync(UserRegistrationDTO newUser)
        {
            var response = new ResponseModel<string>
            {
                Success = false,
                Data = null
            };
            try
            {
                var userAccount = newUser.UserAccount;
                var userDetails = newUser.UserDetails;

                if (userAccount == null || userDetails == null)
                {
                    response.Message = "messing details.";
                    return response;
                }

                if (userAccount.Username == null || userDetails.Email == null)
                {
                    response.Message = "Invalid Username or Email.";
                    return response;
                }

                var existingUserUsername = await _userAccountRepo.GetUserByUserNameOrEmailAsync(userAccount.Username);
                var existingUserEmail = await _userAccountRepo.GetUserByUserNameOrEmailAsync(userDetails.Email);
                var existingPhoneNumber = await _userAccountRepo.GetUserByPhoneNumber(userDetails.PhoneNumber);

                if (existingUserUsername != null)
                {
                    response.Message = "Username already exists.";
                    return response;
                }

                if (existingUserEmail != null)
                {
                    response.Message = "Email already exists.";
                    return response;
                }

                if (existingPhoneNumber != null)
                {
                    response.Message = "Phone number already used by other account";
                    return response;
                }

                //mapping 

                var newUserAccount = new UserAccount
                {
                    Name = userAccount.Name,
                    Username = userAccount.Username,
                    Status = "Active",
                    RoleId = 3,
                    PasswordHash = _hashing.Hash(userAccount.Password),
                    CreatedAt = DateTime.UtcNow
                };

                int userId = await _userAccountRepo.CreateNewUser(newUserAccount);

                var newUserDetails = new UserDetail
                {
                    Age = userDetails.Age,
                    BillingAddress = userDetails.BillingAddress,
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = userDetails.PhoneNumber,
                    Email = userDetails.Email,
                    Gender = userDetails.Gender,
                    UserId = newUserAccount.Id
                };

                await _userDetails.CreateUserDetails(userId, newUserDetails);
                
                try{ 
                    if (_walletRepo == null)
                    {
                        Console.WriteLine("Wallet repository is null.");
                        response.Message = "Wallet repository not configured.";
                        return response;
                    }

                    var newWallet = new UserWallet{
                    UserId = userId,
                    AvailableBalance = 0,
                    OnHoldBalance = 0,
                    Currency = "MYR",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,};


                    await  _walletRepo.CreateWallet(newWallet);

               
                }catch (Exception e){
                    Console.WriteLine("auth class wallet error" + e);
                    response.Message = "Wallet not created";
                    return response;
                };

                

                response.Success = true;
                response.Data = null; // You can also return userId or other info if needed
                response.Message = "Account created successfully.";

                return response;
            }
            catch (Exception e)
            {
                response.Message = "Invalid Details";
                return response;
            }
        }


        public async Task<ResponseModel<string>> ChangePasswordAsync(UpdatePasswordDTO updatePasswordDto, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<string>
            {
                Success = false,
                Data = null
            };
            try
            {
                var userIdClaim = userClaims.FindFirst(ClaimTypes.Name)?.Value;

                if (userIdClaim == null)
                {
                    response.Message = "User not found.";
                    return response;
                }

                var userId = int.Parse(userIdClaim);

                var user = await _userAccountRepo.GetUserByIdAsync(userId);

                if (user == null)
                {
                    response.Message = "User not found.";
                    return response;
                }

                if (!_hashing.Verify(user.PasswordHash, updatePasswordDto.CurrentPassword))
                {
                    response.Message = "Old password is not correct.";
                    return response;
                }

                user.PasswordHash = _hashing.Hash(updatePasswordDto.NewPassword);

                await _userAccountRepo.UpdateUserPasswordAsync(user); 

                response.Success = true;
                response.Message = "Password changed successfully.";
                response.Data = null;

                return response;

            }
            catch (Exception e)
            {
                response.Message = "Unable to change the password";
                return response;
            }
        }


    }
}

using System.Data;
using System.Security.Claims;
using PotatoTrade.DTO.User;
using PototoTrade.DTO.Product;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.DTO.User;
using PototoTrade.Models.Media;
using PototoTrade.Models.User;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Repository.Role;
using PototoTrade.Repository.Users;
using PototoTrade.Service.Utilites.Hash;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.User
{
    public class UserAccountService
    {
        private readonly UserAccountRepository _userAccountRepository;
        private readonly MediaRepository _mediaRepository;
        private readonly RoleRepository _roleRepository;
        private readonly UserDetailsRepository _userDetailRepository;
        private readonly IHashing _hashing;

        public UserAccountService(UserAccountRepository userAccountRepository, RoleRepository roleRepository, IHashing hashing, MediaRepository mediaRepository, UserDetailsRepository userDetailsRepository)
        {
            _userAccountRepository = userAccountRepository;
            _roleRepository = roleRepository;
            _hashing = hashing;
            _mediaRepository = mediaRepository;
            _userDetailRepository = userDetailsRepository;
        }

        public async Task<ResponseModel<List<GetUserListDTO>>> GetUserList(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<List<GetUserListDTO>>
            {
                Success = false,
                Data = new List<GetUserListDTO>(),
                Message = "Failed to retrieve users."
            };
            try
            {
                var userList = await _userAccountRepository.GetUsersList();

                var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;

                if (userList == null || !userList.Any())
                {
                    response.Message = "No users found.";
                    return response;
                }

                var users = userList.Select(user => new GetUserListDTO
                {
                    Id = user.Id,
                    Avatar = "", // Assuming AvatarMedia is correctly set up
                    Name = user.Name,
                    Username = user.Username,
                    Email = user.UserDetails.FirstOrDefault()?.Email ?? string.Empty,
                    RoleName = user.Role.RoleName,
                    RoleType = user.Role.RoleType,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt
                }).ToList();

                foreach (var user in users)
                {
                    var media = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                    user.Avatar = media == null ? "" : media.MediaUrl;
                }

                response.Data = users;
                response.Success = true;
                response.Message = "Users retrieved successfully.";
                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Fetch Users Details";
                response.Success = false;
                return response;
            }
        }

        public async Task<ResponseModel<GetUserDTO>> GetUser(int id, ClaimsPrincipal userClaims)
        {

            var response = new ResponseModel<GetUserDTO>();

            var userRole = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            try
            {
                var user = await _userAccountRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                // Create a new GetGetUserDTO object and populate initial fields
                var userInfo = new GetUserDTO
                {
                    Id = user.Id,
                    RoleId = user.RoleId,
                    UserName = user.Username,
                    Name = user.Name,
                    Status = user.Status,
                    Gender = user.UserDetails.FirstOrDefault()?.Gender ?? string.Empty,
                    Email = user.UserDetails.FirstOrDefault()?.Email ?? string.Empty,
                    Age = user.UserDetails.FirstOrDefault()?.Age ?? 0,
                    BillingAddress = user.UserDetails.FirstOrDefault()?.BillingAddress ?? string.Empty,
                    PhoneNumber = user.UserDetails.FirstOrDefault()?.PhoneNumber ?? string.Empty,
                    UserCover = "",
                    Avatar = "",
                    RoleName = user.Role.RoleName,
                    RoleType = user.Role.RoleType,
                    CreatedAt = user.CreatedAt,
                    ProductList = new List<ProductDTO>(),
                    Roles = new List<UpdateViewRoleDTO>(),
                };

                // Fetch and populate product information with media URLs
                if (user.Products != null)
                {

                    foreach (var product in user.Products)
                    {
                        var productImage = await _mediaRepository.GetMediaBySourceIdAndType(product.Id, "Product");
                        var productDto = new ProductDTO
                        {
                            Id = product.Id,
                            Price = product.Price,
                            CreatedAt = product.CreatedAt,
                            RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                            Title = product.Title,
                            // Fetch the media URL asynchronously
                            Image = productImage == null ? "" : productImage.MediaUrl
                        };

                        userInfo.ProductList.Add(productDto);
                    }
                }

                //Fetch Roles 

                var roles = await _roleRepository.GetRolesAsync();
                var usersRole = roles.Select(r =>
                {
                    return r.RoleType != "SuperAdmin" && r.RoleType != "User" ? new UpdateViewRoleDTO
                    {
                        Id = r.Id,
                        RoleType = r.RoleType,
                        RoleName = r.RoleName
                    } : null;
                }).ToList();

                userInfo.Roles = usersRole;

                // Fetch user cover and avatar asynchronously
                var coverImage = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "user_Cover");
                var avatarImage = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "user_Profile");
                userInfo.UserCover = coverImage == null ? "" : coverImage.MediaUrl;
                userInfo.Avatar = avatarImage == null ? "" : avatarImage.MediaUrl;

                response.Data = userInfo;
                response.Success = true;
                response.Message = "User Fetched Successfully";
                // Fetch roles and permissions

                return response;

            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Fetch The User Information";
                response.Success = false;
                return response;
            }
        }

        public async Task<ResponseModel<int>> CreateUser(CreateUserDTO userInfo, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<int> { Success = false, Data = 0 };

            try
            {
                if (string.IsNullOrWhiteSpace(userInfo.Name) ||
                    string.IsNullOrWhiteSpace(userInfo.Email) ||
                    string.IsNullOrWhiteSpace(userInfo.Gender) ||
                    userInfo.Age <= 0 ||
                    userInfo.RoleId < 1)
                {
                    response.Message = "Invalid input data. Please ensure all required fields are filled correctly.";
                    return response;
                }

                var existingEmail = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.Email);
                var existingUsername = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.UserName);
                var existingPhoneNumber = await _userAccountRepository.GetUserByPhoneNumber(userInfo.PhoneNumber);
                if (existingEmail != null)
                {
                    response.Message = "Email already taken.";
                    return response;
                }
                if (existingUsername != null)
                {
                    response.Message = "Username already taken.";
                    return response;
                }
                if (existingPhoneNumber != null)
                {
                    response.Message = "Phone number already used by other account";
                    return response;
                }


                var newUser = new UserAccount
                {
                    Name = userInfo.Name,
                    Username = userInfo.UserName,
                    Status = userInfo.Status,
                    RoleId = userInfo.RoleId,
                    PasswordHash = _hashing.Hash("password"),
                    CreatedAt = DateTime.UtcNow,
                    UserDetails = new List<UserDetail>
                {
                    new UserDetail
                    {
                        Gender = userInfo.Gender,
                        Email = userInfo.Email,
                        Age = userInfo.Age,
                        BillingAddress = userInfo.BillingAddress,
                        PhoneNumber = userInfo.PhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                    }
                }
                };

                //Create New User
                await _userAccountRepository.CreateNewUser(newUser);

                //Handle media 
                if (!string.IsNullOrEmpty(userInfo.Avatar) || !string.IsNullOrEmpty(userInfo.UserCover))
                {
                    var mediaList = new List<Media>();

                    if (!string.IsNullOrEmpty(userInfo.Avatar))
                    {
                        var newMediaProfile = new Media
                        {
                            CreatedAt = DateTime.UtcNow,
                            SourceType = "User_Profile",
                            SourceId = newUser.Id,
                            MediaUrl = userInfo.Avatar
                        };
                        mediaList.Add(newMediaProfile);
                    }

                    if (!string.IsNullOrEmpty(userInfo.UserCover))
                    {
                        var newMediaCover = new Media
                        {
                            CreatedAt = DateTime.UtcNow,
                            SourceType = "User_Cover",
                            SourceId = newUser.Id,
                            MediaUrl = userInfo.UserCover
                        };
                        mediaList.Add(newMediaCover);
                    }

                    if (mediaList.Any())
                    {
                        await _mediaRepository.CreateMedias(newUser.Id, mediaList);
                    }
                }


                response.Success = true;
                response.Data = newUser.Id;
                response.Message = "User created successfully.";
                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something went wrong. Unable to create user.";
                response.Success = false;
                response.Data = 0;
                return response;
            }
        }

        public async Task<ResponseModel<bool>> DeleteUser(int id)
        {
            var response = new ResponseModel<bool> { Success = false, Data = false };

            var user = await _userAccountRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                response.Message = "User not found.";
                return response;
            }
            try
            {
                await _mediaRepository.DeleteMediaBySourceId(user.Id);
                await _userAccountRepository.DeleteUserAsync(id);

                response.Success = true;
                response.Data = true;
                response.Message = "User deleted successfully.";
                return response;

            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Delete The User";
                response.Success = false;
                return response;

            }
        }


        public async Task<ResponseModel<bool>> updateUser(int id, UpdateUserDTO userInfo, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<bool> { Success = false, Data = false };
            try
            {
                var user = await _userAccountRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    response.Message = "User not found.";
                    return response;
                }

                var userRole = userClaims.FindFirst(ClaimTypes.Name)?.Value;

                var CreateUser = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.Email);
                if (CreateUser != null && CreateUser.Id != id)
                {
                    response.Message = "Email already taken.";
                    return response;
                }

                var existingUser = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.UserName);
                if (existingUser != null && existingUser.Id != id)
                {
                    response.Message = "Username already taken.";
                    return response;
                }

                user.Name = string.IsNullOrWhiteSpace(userInfo.Name) ? user.Name : userInfo.Name;
                user.Username = string.IsNullOrWhiteSpace(userInfo.UserName) ? user.Username : userInfo.UserName;
                user.RoleId = userInfo.RoleId < 1 ? user.RoleId : userInfo.RoleId;
                user.Status = string.IsNullOrWhiteSpace(userInfo.Status) ? user.Status : userInfo.Status;
                user.UpdatedAt = DateTime.UtcNow;

                var userDetails = user.UserDetails.FirstOrDefault();
                if (userDetails != null)
                {
                    userDetails.Gender = userInfo.Gender;
                    userDetails.Email = userInfo.Email;
                    userDetails.Age = userInfo.Age;
                    userDetails.BillingAddress = userInfo.BillingAddress;
                    userDetails.PhoneNumber = userInfo.PhoneNumber;
                    userDetails.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    response.Message = "User details not found.";
                    return response;
                }

                if (!string.IsNullOrEmpty(userInfo.Avatar) || !string.IsNullOrEmpty(userInfo.UserCover))
                {
                    var mediaList = new List<Media>();

                    if (!string.IsNullOrEmpty(userInfo.Avatar))
                    {
                        var existingProfileMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                        if (existingProfileMedia == null)
                        {
                            var newMediaProfile = new Media
                            {
                                CreatedAt = DateTime.UtcNow,
                                SourceType = "User_Profile",
                                SourceId = user.Id,
                                MediaUrl = userInfo.Avatar
                            };
                            mediaList.Add(newMediaProfile);
                        }
                        else
                        {
                            existingProfileMedia.MediaUrl = userInfo.Avatar;
                            existingProfileMedia.UpdatedAt = DateTime.UtcNow;
                            mediaList.Add(existingProfileMedia);
                        }
                    }

                    // Handle Cover Image (User_Cover)
                    if (!string.IsNullOrEmpty(userInfo.UserCover))
                    {
                        var existingCoverMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Cover");
                        if (existingCoverMedia == null)
                        {
                            var newMediaCover = new Media
                            {
                                CreatedAt = DateTime.UtcNow,
                                SourceType = "User_Cover",
                                SourceId = user.Id,
                                MediaUrl = userInfo.UserCover
                            };
                            mediaList.Add(newMediaCover);
                        }
                        else
                        {
                            existingCoverMedia.MediaUrl = userInfo.UserCover;
                            existingCoverMedia.UpdatedAt = DateTime.UtcNow;
                            mediaList.Add(existingCoverMedia);
                        }
                    }

                    // Separate mediaList into existing media to update and new media to create
                    var mediasToUpdate = mediaList.Where(m => m.Id != 0).ToList();
                    var mediasToCreate = mediaList.Where(m => m.Id == 0).ToList();

                    if (mediasToUpdate.Any())
                    {
                        await _mediaRepository.UpdateMedias(user.Id, mediasToUpdate);
                    }

                    if (mediasToCreate.Any())
                    {
                        await _mediaRepository.CreateMedias(user.Id, mediasToCreate);
                    }
                }
                await _userAccountRepository.UpdateUserAsync(user);
                response.Success = true;
                response.Data = true;
                response.Message = "User updated successfully.";
                return response;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error updating user with ID {id}: {e.Message}");
                response.Message = "Something went wrong. Unable to update user information.";
                response.Success = false;
                response.Data = false;
                return response;
            }

        }


        public async Task<ResponseModel<GetUserDTO>> GetUserProfile(int id)
        {
            var response = new ResponseModel<GetUserDTO>();

            try
            {
                var user = await _userAccountRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                var userInfo = new GetUserDTO
                {
                    Id = user.Id,
                    UserName = user.Username,
                    Name = user.Name,
                    Status = user.Status,
                    Avatar = string.Empty
                };

                var avatarMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                userInfo.Avatar = avatarMedia?.MediaUrl ?? string.Empty;

                response.Data = userInfo;
                response.Success = true;
                response.Message = "User profile retrieved successfully.";

                return response;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving the user profile.";
                return response;
            }
        }


    }

}

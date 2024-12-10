using System.Data;
using System.Security.Claims;
using PotatoTrade.DTO.MediaDTO;
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
                    Name = user.Name,
                    Username = user.Username,
                    Email = user.UserDetails.FirstOrDefault()?.Email ?? string.Empty,
                    RoleName = user.Role.RoleName,
                    RoleType = user.Role.RoleType,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    Medias = new List<HandleMedia>() //
                }).ToList();
                foreach (var user in users)
                {
                    // Fetch the avatar (User_Profile)
                    var avatar = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                    if (avatar != null)
                    {
                        user.Medias.Add(new HandleMedia
                        {
                            Id = avatar.Id,
                            Type = "User_Profile",
                            MediaUrl = avatar.MediaUrl,
                            CreatedAt = avatar.CreatedAt,
                            UpdatedAt = avatar.UpdatedAt
                        });
                    }

                    // Fetch the cover (User_Cover)
                    var cover = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Cover");
                    if (cover != null)
                    {
                        user.Medias.Add(new HandleMedia
                        {
                            Id = cover.Id,
                            Type = "User_Cover",
                            MediaUrl = cover.MediaUrl,
                            CreatedAt = cover.CreatedAt,
                            UpdatedAt = cover.UpdatedAt
                        });
                    }
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

                // Initialize GetUserDTO with basic user information
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
                    RoleName = user.Role.RoleName,
                    RoleType = user.Role.RoleType,
                    CreatedAt = user.CreatedAt,
                    ProductList = new List<ProductDTO>(),
                    Roles = new List<UpdateViewRoleDTO>(),
                    Medias = new List<HandleMedia>() // Initialize Medias list
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
                            Image = productImage?.MediaUrl ?? string.Empty
                        };

                        userInfo.ProductList.Add(productDto);
                    }
                }

                // Fetch Roles
                var roles = await _roleRepository.GetRolesAsync();
                var usersRole = roles
                    .Where(r => r.RoleType != "SuperAdmin" && r.RoleType != "User")
                    .Select(r => new UpdateViewRoleDTO
                    {
                        Id = r.Id,
                        RoleType = r.RoleType,
                        RoleName = r.RoleName
                    })
                    .ToList();

                userInfo.Roles = usersRole;

                // Fetch and add User_Profile and User_Cover media
                var userProfileMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                var userCoverMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Cover");

                if (userProfileMedia != null)
                {
                    userInfo.Medias.Add(new HandleMedia
                    {
                        Id = userProfileMedia.Id,
                        Type = "User_Profile",
                        MediaUrl = userProfileMedia.MediaUrl,
                        CreatedAt = userProfileMedia.CreatedAt,
                        UpdatedAt = userProfileMedia.UpdatedAt
                    });
                }

                if (userCoverMedia != null)
                {
                    userInfo.Medias.Add(new HandleMedia
                    {
                        Id = userCoverMedia.Id,
                        Type = "User_Cover",
                        MediaUrl = userCoverMedia.MediaUrl,
                        CreatedAt = userCoverMedia.CreatedAt,
                        UpdatedAt = userCoverMedia.UpdatedAt
                    });
                }

                response.Data = userInfo;
                response.Success = true;
                response.Message = "User fetched successfully.";

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "Error fetching user with ID {UserId}", id);

                response.Message = "Something went wrong. Unable to fetch the user information.";
                response.Success = false;
                return response;
            }
        }

        public async Task<ResponseModel<int>> CreateUser(CreateUserDTO userInfo, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<int> { Success = false, Data = 0 };

            try
            {
                // Input Validation
                if (string.IsNullOrWhiteSpace(userInfo.Name) ||
                    string.IsNullOrWhiteSpace(userInfo.Email) ||
                    string.IsNullOrWhiteSpace(userInfo.Gender) ||
                    userInfo.Age <= 0 ||
                    userInfo.RoleId < 1)
                {
                    response.Message = "Invalid input data. Please ensure all required fields are filled correctly.";
                    return response;
                }

                // Check for existing email, username, and phone number
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
                    response.Message = "Phone number already used by another account.";
                    return response;
                }

                // Create new user
                var newUser = new UserAccount
                {
                    Name = userInfo.Name,
                    Username = userInfo.UserName,
                    Status = userInfo.Status,
                    RoleId = userInfo.RoleId,
                    PasswordHash = _hashing.Hash("password"), // Consider hashing a real password or setting a default
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

                // Save the new user to the database
                await _userAccountRepository.CreateNewUser(newUser);

                // Handle media
                if (userInfo.Medias != null && userInfo.Medias.Any())
                {
                    var mediaList = new List<Media>();

                    // Assign media types based on their positions in the array
                    // Assuming first media is User_Profile (Avatar), second is User_Cover (Cover)
                    for (int i = 0; i < userInfo.Medias.Count; i++)
                    {
                        var mediaDTO = userInfo.Medias[i];
                        var objectKey = mediaDTO.MediaUrl;

                        if (string.IsNullOrEmpty(objectKey))
                        {
                            continue; // Skip invalid URLs
                        }

                        string mediaType = mediaDTO.Type ?? "";
                        if (mediaType == "")
                        {
                            continue;
                        }

                        var media = new Media
                        {
                            CreatedAt = DateTime.UtcNow,
                            SourceType = mediaType,
                            SourceId = newUser.Id,
                            MediaUrl = objectKey
                        };

                        mediaList.Add(media);
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
            catch (Exception ex)
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
                await _mediaRepository.DeleteMediaBySourceIdAndType(user.Id, "User_Profile");
                await _mediaRepository.DeleteMediaBySourceIdAndType(user.Id, "User_Cover");
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

        public async Task<ResponseModel<bool>> UpdateUser(int id, UpdateUserDTO userInfo, ClaimsPrincipal userClaims)
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

                var emailUser = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.Email);
                if (emailUser != null && emailUser.Id != id)
                {
                    response.Message = "Email already taken.";
                    return response;
                }

                var usernameUser = await _userAccountRepository.GetUserByUserNameOrEmailAsync(userInfo.UserName);
                if (usernameUser != null && usernameUser.Id != id)
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

                // Handle media updates
                var coverMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Cover");
                var profileMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");

                foreach (var mediaInput in userInfo.Medias)
                {
                    if (mediaInput.Type == "User_Cover")
                    {
                        if (mediaInput.MediaUrl == "" && coverMedia != null)
                        {
                            await _mediaRepository.DeleteMedia(coverMedia);
                        }
                        else if (coverMedia != null)
                        {
                            coverMedia.MediaUrl = mediaInput.MediaUrl;
                            coverMedia.UpdatedAt = DateTime.UtcNow;
                            await _mediaRepository.UpdateMedias(user.Id, new List<Media> { coverMedia });
                        }
                        else if(mediaInput.MediaUrl != "" && coverMedia== null)
                        {
                            var newCoverMedia = new Media
                            {
                                SourceId = user.Id,
                                SourceType = "User_Cover",
                                MediaUrl = mediaInput.MediaUrl,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _mediaRepository.CreateMedias(user.Id, new List<Media> { newCoverMedia });
                        }
                    }
                    else if (mediaInput.Type == "User_Profile")
                    {
                        if (mediaInput.MediaUrl == "" && profileMedia != null)
                        {
                            await _mediaRepository.DeleteMedia(profileMedia);
                        }
                        else if (profileMedia != null)
                        {
                            profileMedia.MediaUrl = mediaInput.MediaUrl;
                            profileMedia.UpdatedAt = DateTime.UtcNow;
                            await _mediaRepository.UpdateMedias(user.Id, new List<Media> { profileMedia });
                        }
                        else if(mediaInput.MediaUrl != "" && profileMedia == null)
                        {
                            var newProfileMedia = new Media
                            {
                                SourceId = user.Id,
                                SourceType = "User_Profile",
                                MediaUrl = mediaInput.MediaUrl,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _mediaRepository.CreateMedias(user.Id, new List<Media> { newProfileMedia });
                        }
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

                // Initialize GetUserDTO with basic user information
                var userInfo = new GetUserDTO
                {
                    Id = user.Id,
                    UserName = user.Username,
                    Name = user.Name,
                    Status = user.Status,
                    Gender = user.UserDetails.FirstOrDefault()?.Gender ?? string.Empty,
                    Email = user.UserDetails.FirstOrDefault()?.Email ?? string.Empty,
                    Age = user.UserDetails.FirstOrDefault()?.Age ?? 0,
                    BillingAddress = user.UserDetails.FirstOrDefault()?.BillingAddress ?? string.Empty,
                    PhoneNumber = user.UserDetails.FirstOrDefault()?.PhoneNumber ?? string.Empty,
                    RoleName = user.Role.RoleName,
                    RoleType = user.Role.RoleType,
                    CreatedAt = user.CreatedAt,
                    ProductList = new List<ProductDTO>(),
                    Roles = new List<UpdateViewRoleDTO>(),
                    Medias = new List<HandleMedia>() // Initialize Medias list
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
                            Image = productImage?.MediaUrl ?? string.Empty
                        };

                        userInfo.ProductList.Add(productDto);
                    }
                }

                // Fetch Roles
                var roles = await _roleRepository.GetRolesAsync();
                var usersRole = roles
                    .Where(r => r.RoleType != "SuperAdmin" && r.RoleType != "User")
                    .Select(r => new UpdateViewRoleDTO
                    {
                        Id = r.Id,
                        RoleType = r.RoleType,
                        RoleName = r.RoleName
                    })
                    .ToList();

                userInfo.Roles = usersRole;

                var userProfileMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");
                var userCoverMedia = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Cover");

                if (userProfileMedia != null)
                {
                    userInfo.Medias.Add(new HandleMedia
                    {
                        Id = userProfileMedia.Id,
                        Type = "User_Profile",
                        MediaUrl = userProfileMedia.MediaUrl,
                        CreatedAt = userProfileMedia.CreatedAt,
                        UpdatedAt = userProfileMedia.UpdatedAt
                    });
                }

                if (userCoverMedia != null)
                {
                    userInfo.Medias.Add(new HandleMedia
                    {
                        Id = userCoverMedia.Id,
                        Type = "User_Cover",
                        MediaUrl = userCoverMedia.MediaUrl,
                        CreatedAt = userCoverMedia.CreatedAt,
                        UpdatedAt = userCoverMedia.UpdatedAt
                    });
                }

                response.Data = userInfo;
                response.Success = true;
                response.Message = "User profile retrieved successfully.";

                return response;
            }
            catch (Exception ex)
            {

                response.Success = false;
                response.Message = "An error occurred while retrieving the user profile.";
                return response;
            }
        }

        public async Task<ResponseModel<List<int>>> GetUserIdsByRoleId(int roleId)
        {
            var response = new ResponseModel<List<int>>
            {
                Success = false,
                Data = new List<int>(),
                Message = "Failed to retrieve user IDs."
            };
            try
            {
                var userIds = await _userAccountRepository.GetUserIdsByRoleId(roleId);

                if (userIds == null || !userIds.Any())
                {
                    response.Message = "No users found with the specified role.";
                    return response;
                }

                response.Data = userIds;
                response.Success = true;
                response.Message = "User IDs retrieved successfully.";
                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something went wrong. Unable to fetch user IDs.";
                response.Success = false;
                return response;
            }
        }

        // public async Task<ResponseModel<List<string>>> GetUserNamesByUserIds(List<int> userIds)
        // {
        //     var response = new ResponseModel<List<int>>
        //     {
        //         Success = false,
        //         Data = new List<int>(),
        //         Message = "Failed to retrieve usernames."
        //     };
        //     try
        //     {
        //        //logic

        //         response.Data = userIds;
        //         response.Success = true;
        //         response.Message = "Usernames retrieved successfully.";
        //         return response;
        //     }
        //     catch (Exception e)
        //     {
        //         response.Message = "Something went wrong. Unable to fetch Usernames.";
        //         response.Success = false;
        //         return response;
        //     }

        // }

    }
    

}

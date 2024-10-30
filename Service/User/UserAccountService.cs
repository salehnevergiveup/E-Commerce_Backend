using System.Data;
using System.Security.Claims;
using PototoTrade.DTO.AdminPermission;
using PototoTrade.DTO.MediaDTO;
using PototoTrade.DTO.Role;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.DTO.User;
using PototoTrade.Enums;
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

        public async Task<ResponseModel<List<ViewUserAccount>>> GetUserList(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<List<ViewUserAccount>>
            {
                Success = false, // Default to false
                Data = new List<ViewUserAccount>(), // Initialize Data as an empty list
                Message = "Failed to retrieve users." // Default message
            };
            try
            {
                var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;

                var usersList = await _userAccountRepository.GetUsersList();
                if (usersList == null || !usersList.Any())
                {
                    response.Message = "No users found.";
                    return response; // Return with message if no users are found
                }

                var users = usersList.Select(user => new UpdateViewUserAccountDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Username = user.Username,
                    Status = user.Status,
                    RoleId = user.RoleId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                })
                    .ToList();

                var userAccounts = new List<ViewUserAccount>();

                foreach (var user in users)
                {
                    var media = await _mediaRepository.GetMediaBySourceIdAndType(user.Id, "User_Profile");

                    var mediaDto = media != null
                        ? new List<UpdateViewMediaDTO>
                        {
            new UpdateViewMediaDTO
            {
                Id = media.Id,
                MediaUrl = media.MediaUrl,
                SourceType = media.SourceType,
                SourceId = media.SourceId
            }
                        }
                        : new List<UpdateViewMediaDTO>();

                    var userAccount = new ViewUserAccount
                    {
                        UserAccount = user,
                        AvailableRoles = null,  // Set if needed
                        MediaItems = mediaDto,
                        UserDetails = null      // Set if needed
                    };

                    userAccounts.Add(userAccount);
                }

                response.Data = userAccounts.ToList();
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
        public async Task<ResponseModel<ViewUserAccount>> GetUser(int id, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<ViewUserAccount>();

            var userRole = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _userAccountRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            //  if(user.Role.RoleName != UserRolesEnum.User.ToString() && userRole ==  UserRolesEnum.Admin.ToString()) return null; 

            // Fetch user details
            var details = await _userDetailRepository.GetUserDetailByUserId(id);
            if (details == null)
            {
                response.Success = false;
                response.Message = "User details not found.";
                return response;
            }
            try
            {
                var media = await _mediaRepository.GetMediaBySourceId(id);
                var mediaDtos = media?.Where(m => m.SourceId == id)
                    .Select(m => new UpdateViewMediaDTO
                    {
                        Id = m.Id,
                        MediaUrl = m.MediaUrl,
                        SourceType = m.SourceType,
                        SourceId = m.SourceId
                    })
                    .ToList() ?? null; // Handle null media list

                // Fetch roles and permissions
                var roles = await _roleRepository.GetRolesAsync();
                var rolesDto = roles.Select(role => new UpdateViewRoleAdminPermission
                {
                    Role = new UpdateViewRoleDTO
                    {
                        Id = role.Id,
                        RoleName = role.RoleName,
                        RoleType = role.RoleType,
                        Description = role.Description,
                    },
                    Permission = role.AdminPermissions.FirstOrDefault() != null
                        ? new UpdateViewAdminPermissionDTO
                        {
                            CanCreate = role.AdminPermissions.First().CanCreate,
                            CanDelete = role.AdminPermissions.First().CanDelete,
                            CanEdit = role.AdminPermissions.First().CanEdit,
                            CanView = role.AdminPermissions.First().CanView,
                        }
                        : null
                }).ToList();

                response.Success = true;
                response.Message = "User retrieved successfully.";
                response.Data = new ViewUserAccount
                {
                    AvailableRoles = rolesDto,
                    MediaItems = mediaDtos,
                    UserAccount = new UpdateViewUserAccountDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Username = user.Username,
                        Status = user.Status,
                        RoleId = user.RoleId,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                    },
                    UserDetails = new UpdateViewUserDetailDTO
                    {
                        Id = details.Id,
                        Age = details.Age,
                        BillingAddress = details.BillingAddress,
                        Email = details.Email,
                        Gender = details.Gender,
                        PhoneNumber = details.PhoneNumber,
                    }
                };
                return response;

            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Fetch The User Information";
                response.Success = false;
                return response;
            }
        }

        public async Task<ResponseModel<bool>> CreateUser(CreateNewAccount newUser, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<bool> { Success = false, Data = false }; // Initialize Data as null

            var user = newUser.UserAccount;
            var userDetail = newUser.UserDetail;
            var userMedia = newUser.MediaItems;

            if (await _userAccountRepository.GetUserByUserNameOrEmailAsync(user.Username) != null)
            {
                response.Message = "Username is already taken.";
                return response;
            };

            if (await _userDetailRepository.GetDetailByEmail(userDetail.Email) != null)
            {
                response.Message = "Email is already registered.";
                return response; // Return the response with the message
            }

            var checkRole = await _roleRepository.GetRoleAsync(user.RoleId);

            if (checkRole == null || checkRole.RoleType == UserRolesEnum.SuperAdmin.ToString())
            {
                response.Message = "Can Not Create User With Provided Role.";
                return response; // Return the response with the message
            }

            var userRole = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            var role = ""; //await _roleRepository.GetRoleAsync(user.RoleId);

            // if(role  ==  null ||  role.RoleName == UserRolesEnum.SuperAdmin.ToString()) return false;  

            // if(role.RoleName == UserRolesEnum.Admin.ToString() &&  !(userRole == UserRolesEnum.User.ToString())) return false;  

            var newUserAccount = new UserAccount
            {
                Name = user.Name,
                Username = user.Username,
                PasswordHash = _hashing.Hash(user.Password),
                RoleId = user.RoleId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var newUserDetail = new UserDetail
            {
                PhoneNumber = userDetail.PhoneNumber,
                Age = userDetail.Age,
                BillingAddress = userDetail.BillingAddress,
                CreatedAt = DateTime.UtcNow,
                Email = userDetail.Email,
                Gender = userDetail.Gender
            };

            var medias = userMedia.Select(m => new Media
            {
                MediaUrl = m.MediaUrl,
                SourceType = m.SourceType,
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<Media>(); ;
            try
            {
                int userId = await _userAccountRepository.CreateNewUser(newUserAccount);

                await _userDetailRepository.CreateUserDetails(userId, newUserDetail);

                if (medias.Count > 0)
                {
                    await _mediaRepository.CreateMedias(userId, medias);
                }

                response.Success = true;
                response.Data = true;
                response.Message = "User created successfully.";

                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Create User";
                response.Success = false;
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

        public async Task<ResponseModel<bool>> updateUser(int id, UpdateUserAccountDTO updateUserAccount, ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<bool> { Success = false, Data = false }; // Default Data to false

            var userAccount = updateUserAccount.UserAccount;
            var userDetails = updateUserAccount.UserDetail;
            var userMedia = updateUserAccount.MediaItems;

            var user = await _userAccountRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                response.Message = "User not found.";
                return response;
            }

            var userRole = userClaims.FindFirst(ClaimTypes.Name)?.Value;

            // Check for unique username
            if (await _userAccountRepository.GetUserByUserNameOrEmailAsync(userAccount.Username) != null && user.Username != userAccount.Username)
            {
                response.Message = "Username already taken.";
                return response;
            }

            // Update the user account
            user.Name = string.IsNullOrWhiteSpace(userAccount.Name) ? user.Name : userAccount.Name;
            user.RoleId = userAccount.RoleId < 1 ? user.RoleId : userAccount.RoleId;
            user.Status = string.IsNullOrWhiteSpace(userAccount.Status) ? user.Status : userAccount.Status;
            user.UpdatedAt = DateTime.UtcNow;

            // Update user details  
            var userDetail = await _userDetailRepository.GetUserDetailByUserId(id);
            if (userDetail != null)
            {
                if (await _userAccountRepository.GetUserByUserNameOrEmailAsync(userDetails.Email) != null && userDetails.Email != userDetail.Email)
                {
                    userDetail.Email = userDetail.Email;
                }
                else
                {
                    userDetail.Email = string.IsNullOrWhiteSpace(userDetails.Email) ? userDetail.Email : userDetails.Email;
                }
                userDetail.Age = userDetails.Age < 18 ? userDetail.Age : userDetails.Age;
                userDetail.BillingAddress = string.IsNullOrWhiteSpace(userDetails.BillingAddress) ? userDetail.BillingAddress : userDetails.BillingAddress;
                userDetail.Gender = string.IsNullOrWhiteSpace(userDetails.Gender) || (userDetails.Gender != "M" && userDetails.Gender != "F") ? userDetail.Gender : userDetails.Gender;
                userDetail.UpdatedAt = DateTime.UtcNow;
                userDetail.PhoneNumber = string.IsNullOrWhiteSpace(userDetails.PhoneNumber) ? userDetail.PhoneNumber : userDetail.PhoneNumber;
            }
            try
            {

                if (userMedia != null)
                {
                    var existingMedias = await _mediaRepository.GetMediaBySourceId(id);
                    var mediasToAdd = new List<Media>();


                    foreach (var mediaDto in userMedia)
                    {
                        if (mediaDto != null && !string.IsNullOrWhiteSpace(mediaDto.SourceType) && mediaDto.SourceId == id 
                        &&( mediaDto.SourceType == "User_Profile"
                        || mediaDto.SourceType == "User_Cover"))
                        {
                            var existingMedia = existingMedias.FirstOrDefault(m => m.SourceType == mediaDto.SourceType);

                            if (existingMedia != null)
                            {
                                if (!string.IsNullOrWhiteSpace(mediaDto.MediaUrl))
                                {
                                    existingMedia.MediaUrl = mediaDto.MediaUrl;
                                }
                            }
                            else
                            {
                                var newMedia = new Media
                                {
                                    SourceId = id,
                                    SourceType = mediaDto.SourceType,
                                    MediaUrl = mediaDto.MediaUrl,
                                    CreatedAt = DateTime.UtcNow
                                };
                                mediasToAdd.Add(newMedia);
                            }
                        }
                    }

                    if (mediasToAdd.Count > 0)
                    {
                        await _mediaRepository.CreateMedias(id, mediasToAdd);
                    }
                }

                await _userAccountRepository.UpdateUserAsync(id, user);
                await _userDetailRepository.UpdateUserDetails(id, userDetail);

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (Exception e)
            {
                response.Message = "Something Went Wrong Unable to Update User Information";
                response.Success = false;
                return response;

            }
        }

    }

}

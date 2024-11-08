using System.Security.Claims;
using PototoTrade.DTO.Role;
using PototoTrade.Models.Role;
using PototoTrade.Repository.Role;
using PototoTrade.Repository.Users;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.Role;

public class RoleService
{
    private readonly RoleRepository _roleRepository;
    private readonly UserAccountRepository _userAccountRepository;

    public RoleService(RoleRepository roleRepository, UserAccountRepository userAccountRepository)
    {
        this._roleRepository = roleRepository;
        _userAccountRepository = userAccountRepository;
    }

    public async Task<ResponseModel<List<GetRolesDTO>>> GetRolesList(ClaimsPrincipal claims, bool includeUsers)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<List<GetRolesDTO>>
        {
            Message = "No Roles Found",
            Data = null,
            Success = false
        };

        // if (userRole != UserRolesEnum.SuperAdmin.ToString())
        // {
        //     response.Message = "Unathorized Access";
        //     return response;
        // }

        var rolesPer = await _roleRepository.GetRolesAsync();

        if (!rolesPer.Any()) return response;

        response.Data = rolesPer.Select(r =>
        {
            var permissions = r.AdminPermissions.ToList().First();
            var users = new List<UserRoleDTO>();
            if (includeUsers)
            {
                users = r.UserAccounts.Count > 0 ? r.UserAccounts.Select(u => new UserRoleDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    RoleId = u.RoleId,
                    Status = u.Status
                }).ToList() : new List<UserRoleDTO>();
            }
            return new GetRolesDTO
            {
                Id = r.Id,
                RoleName = r.RoleName,
                RoleType = r.RoleType,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                Permissions = new PermissionsRoleDTO
                {
                    CanCreate = permissions.CanCreate,
                    CanDelete = permissions.CanDelete,
                    CanEdit = permissions.CanEdit,
                    CanView = permissions.CanView
                },
                Users = users
            };
        }).ToList();

        if (response.Data.Any())
        {
            response.Success = true;
            response.Message = "Roles data fetched successfully";
        }

        return response;
    }
    public async Task<ResponseModel<GetRolesDTO>> GetRole(int id, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<GetRolesDTO>
        {
            Message = "No Roles Found",
            Data = null,
            Success = false
        };

        // if (userRole != UserRolesEnum.SuperAdmin.ToString())
        // {
        //     response.Message = "Unathorized Access";
        //     return response;
        // }

        var rolesPer = await _roleRepository.GetRoleAsync(id);

        if (rolesPer == null) return response;

        var permissions = rolesPer.AdminPermissions.ToList().First();

        var users = rolesPer.UserAccounts.Count > 0 ? rolesPer.UserAccounts.Select(u => new UserRoleDTO
        {
            Id = u.Id,
            Name = u.Name,
            RoleId = u.RoleId,
            Status = u.Status
        }).ToList() : new List<UserRoleDTO>();

        response.Data = new GetRolesDTO
        {
            Id = rolesPer.Id,
            RoleName = rolesPer.RoleName,
            RoleType = rolesPer.RoleType,
            Description = rolesPer.Description,
            CreatedAt = rolesPer.CreatedAt,
            Permissions = new PermissionsRoleDTO
            {
                CanCreate = permissions.CanCreate,
                CanDelete = permissions.CanDelete,
                CanEdit = permissions.CanEdit,
                CanView = permissions.CanView
            },
            Users = users

        };

        if (response.Data != null)
        {
            response.Success = true;
            response.Message = "Roles data fetched successfully";
        }

        return response;
    }

    public async Task<ResponseModel<int>> CreateRole(CreateRoleDTO rolePer, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<int>
        {
            Message = "Failed to create role",
            Data = 0,
            Success = false
        };

        // if (userRole != UserRolesEnum.SuperAdmin.ToString())
        // {
        //     response.Message = "Unauthorized Access";
        //     return response;
        // }

        if (rolePer == null || rolePer.Permission == null)
        {
            response.Message = "No Role or Permission Provided";
            return response;
        }

        if (rolePer.RoleName == "Default_Admin")
        {
            response.Message = "Not Allowed To Create Role From This Type";
            return response;
        }

        var newRole = new Roles
        {
            RoleName = rolePer.RoleName,
            Description = rolePer.Description?.Trim(),
            RoleType = "Admin",
            CreatedAt = DateTime.UtcNow,
            AdminPermissions = new List<AdminPermission>
            {
                new AdminPermission
                {
                    CanCreate = rolePer.Permission.CanCreate,
                    CanView = rolePer.Permission.CanView,
                    CanEdit = rolePer.Permission.CanEdit,
                    CanDelete = rolePer.Permission.CanDelete
                }
            }
        };

        try
        {
            if (await _roleRepository.GetRoleByName(newRole.RoleName) != null)
            {
                response.Message = $"{newRole.RoleName} Name Is already Exist Try Anthoer Name Please";
                return response;
            }
            var roleId = await _roleRepository.CreateRole(newRole);
            response.Data = roleId;
            response.Success = true;
            response.Message = "Role created successfully";
        }
        catch (Exception ex)
        {
            response.Message = $"Error occurred: {ex.Message}";
        }

        return response;
    }

    public async Task<ResponseModel<bool>> DeleteRole(int roleId, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<bool>
        {
            Message = "Failed to delete role",
            Data = false,
            Success = false
        };

        // if (userRole != UserRolesEnum.SuperAdmin.ToString())
        // {
        //     response.Message = "Unauthorized Access";
        //     return response;
        // }

        var roleToDelete = await _roleRepository.GetRoleAsync(roleId);

        if (roleToDelete == null)
        {
            response.Message = "Role not found";
            return response;
        }

        if (roleToDelete.RoleName == "Default_Admin" || roleToDelete.RoleType == "SuperAdmin" || roleToDelete.RoleType == "User")
        {
            response.Message = "Role Can Not Be Deleted";
            return response;
        }

        var defaultRole = await _roleRepository.GetDefaultRoleAsync();
        if (defaultRole == null)
        {
            response.Message = "Default role not found";
            return response;
        }

        try
        {
            var deletionSuccess = await _roleRepository.DeleteRole(roleToDelete);

            if (!deletionSuccess)
            {
                response.Message = "Failed to delete role and permissions";
                return response;
            }

            var adminsToUpdate = await _userAccountRepository.GetAdminsByRoleIdAsync(roleId);

            if (adminsToUpdate.Any())
            {
                foreach (var admin in adminsToUpdate)
                {
                    admin.RoleId = defaultRole.Id;
                }

                await _userAccountRepository.UpdateUserAccountsAsync(adminsToUpdate);
            }

            response.Data = true;
            response.Success = true;
            response.Message = "Role and permissions deleted successfully, admins reassigned to default role";
        }
        catch (Exception ex)
        {
            response.Message = $"Error occurred: {ex.Message}";
        }

        return response;
    }

    public async Task<ResponseModel<bool>> UpdateRole(int roleId, UpdateRoleDTO rolePer, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<bool>
        {
            Message = "Failed to update role",
            Data = false,
            Success = false
        };

        // Check if the user is an Admin
        // if (userRole != UserRolesEnum.Admin.ToString())
        // {
        //     response.Message = "Unauthorized Access";
        //     return response;
        // }
        try
        {
            // Fetch the role to update
            var roleToUpdate = await _roleRepository.GetRoleAsync(roleId);

            if (roleToUpdate == null)
            {
                response.Message = "Role not found";
                return response;
            }

            if (roleToUpdate.RoleName == "Default_Admin" || roleToUpdate.RoleType == "SuperAdmin" || roleToUpdate.RoleType == "User")
            {
                response.Message = "Role Can Not Be Updated";
                return response;
            }

            var permissionToUpdate = roleToUpdate.AdminPermissions.FirstOrDefault();

            // Fetch the associated permissions
            if (permissionToUpdate == null)
            {
                response.Message = "Permissions not found";
                return response;
            }

            roleToUpdate.Description = string.IsNullOrWhiteSpace(rolePer.Description) ? roleToUpdate.Description : rolePer.Description;

            if (rolePer.Permissions != null)
            {
                permissionToUpdate.CanCreate = rolePer.Permissions.CanCreate;
                permissionToUpdate.CanView = rolePer.Permissions.CanView;
                permissionToUpdate.CanEdit = rolePer.Permissions.CanEdit;
                permissionToUpdate.CanDelete = rolePer.Permissions.CanDelete;
            }
            await _roleRepository.UpdateRole(roleToUpdate);
            response.Data = true;
            response.Success = true;
            response.Message = "Role updated successfully";
        }
        catch (Exception ex)
        {
            response.Message = $"Error occurred: {ex.Message}";
        }

        return response;
    }

}

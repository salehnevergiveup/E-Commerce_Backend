using System.Security.Claims;
using PototoTrade.DTO.AdminPermission;
using PototoTrade.DTO.Role;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.Enums;
using PototoTrade.Models.Role;
using PototoTrade.Models.Role.Role;
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

    public async Task<ResponseModel<List<UpdateViewRoleAdminPermission>>> GetRolesList(ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<List<UpdateViewRoleAdminPermission>>
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
            return new UpdateViewRoleAdminPermission
            {
                Role = new UpdateViewRoleDTO
                {
                    Description = r.Description,
                    RoleName = r.RoleName,
                    RoleType = r.RoleType,
                    Id = r.Id
                },
                Permission = r.AdminPermissions != null
                ? new UpdateViewAdminPermissionDTO
                {
                    RoleId = r.Id,
                    CanCreate = r.AdminPermissions.FirstOrDefault()?.CanCreate ?? false,
                    CanView = r.AdminPermissions.FirstOrDefault()?.CanView ?? false,
                    CanEdit = r.AdminPermissions.FirstOrDefault()?.CanEdit ?? false,
                    CanDelete = r.AdminPermissions.FirstOrDefault()?.CanDelete ?? false
                }
                : null
            };
        }).ToList();

        if (response.Data.Any())
        {
            response.Success = true;
            response.Message = "Roles data fetched successfully";
        }

        return response;
    }
    public async Task<ResponseModel<UpdateViewRoleAdminPermission>> GetRole(int id, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var response = new ResponseModel<UpdateViewRoleAdminPermission>
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

        response.Data = new UpdateViewRoleAdminPermission
        {
            Role = new UpdateViewRoleDTO
            {
                Description = rolesPer.Description,
                RoleName = rolesPer.RoleName,
                RoleType = rolesPer.RoleType,
                Id = rolesPer.Id
            },
            Permission = rolesPer.AdminPermissions != null
                ? new UpdateViewAdminPermissionDTO
                {
                    RoleId = rolesPer.Id,
                    CanCreate = rolesPer.AdminPermissions.FirstOrDefault()?.CanCreate ?? false,
                    CanView = rolesPer.AdminPermissions.FirstOrDefault()?.CanView ?? false,
                    CanEdit = rolesPer.AdminPermissions.FirstOrDefault()?.CanEdit ?? false,
                    CanDelete = rolesPer.AdminPermissions.FirstOrDefault()?.CanDelete ?? false
                }
                : null
        };

        if (response.Data != null)
        {
            response.Success = true;
            response.Message = "Roles data fetched successfully";
        }

        return response;
    }

    public async Task<ResponseModel<bool>> CreateRole(RoleAdminPermissionCreate rolePer, ClaimsPrincipal claims)
    {
        var userRole = claims.FindFirst(ClaimTypes.Name)?.Value;

        var role = rolePer.Role;
        var permission = rolePer.Permission;

        var response = new ResponseModel<bool>
        {
            Message = "Failed to create role",
            Data = false,
            Success = false
        };

        // if (userRole != UserRolesEnum.SuperAdmin.ToString())
        // {
        //     response.Message = "Unauthorized Access";
        //     return response;
        // }
        
        if (role.RoleName == "Default_Admin" || role.RoleType == "SuperAdmin" || role.RoleType == "User")
        {
            response.Message = "Not Allowed To Create Role From This Type";
            return response;
        }

        if (role == null || permission == null)
        {
            response.Message = "No Role or Permission Provided";
            return response;
        }

        if (role.RoleType != UserRolesEnum.Admin.ToString())
        {
            response.Message = "Role type must be Admin";
            return response;
        }

        var newRole = new Roles
        {
            RoleName = role.RoleName?.Trim(),
            Description = role.Description?.Trim(),
            RoleType = role.RoleType,
            CreatedAt = DateTime.UtcNow
        };

        var newPermission = new AdminPermission
        {
            CanCreate = permission.CanCreate,
            CanView = permission.CanView,
            CanEdit = permission.CanEdit,
            CanDelete = permission.CanDelete
        };

        try
        {
            await _roleRepository.CreateRole(newRole, newPermission);
            response.Data = true;
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

    public async Task<ResponseModel<bool>> UpdateRole(int roleId, UpdateViewRoleAdminPermission rolePer, ClaimsPrincipal claims)
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

        var roleData = rolePer.Role;
        var permissionData = rolePer.Permission;

        // Update Role Data
        if (roleData != null)
        {
            if (!string.IsNullOrWhiteSpace(roleData.RoleName))
            {
                roleToUpdate.RoleName = roleData.RoleName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(roleData.Description))
            {
                roleToUpdate.Description = roleData.Description.Trim();
            }
        }

        // Update Permissions
        if (permissionData != null)
        {
            // Update permissions only if provided
            permissionToUpdate.CanCreate = permissionData.CanCreate;
            permissionToUpdate.CanView = permissionData.CanView;
            permissionToUpdate.CanEdit = permissionData.CanEdit;
            permissionToUpdate.CanDelete = permissionData.CanDelete;
        }

        try
        {
            await _roleRepository.UpdateRole(roleToUpdate, permissionToUpdate);
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

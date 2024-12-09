
using System.Data;
using PototoTrade.Enums;
using PototoTrade.Models;
using PototoTrade.Models.Role;
using PototoTrade.Models.User;
using PototoTrade.Service.Utilites.Hash;

namespace PototoTrade.Data.Seeders;

public class SystemInti : Seeder
{

    public SystemInti(DBC _dataContext, IHashing _hash) : base(_dataContext, _hash)
    { }

    private SystemInti SeedRoles()
    {
        if (!this._dataContext.Roles.Any())
        {
            var roles = Enum.GetValues(typeof(UserRolesEnum))
                .Cast<UserRolesEnum>()
                .Select(role => new Roles
                {
                    RoleType = role.ToString(),
                    Description = role switch
                    {
                        UserRolesEnum.SuperAdmin => "Super Admin with all access and privileges",
                        UserRolesEnum.Admin => "Administrator with elevated privileges",
                        UserRolesEnum.User => "Regular user with standard access",
                        _ => "No description available"
                    },
                    RoleName = "Default_" + role,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            this._dataContext.Roles.AddRange(roles);
            this._dataContext.SaveChanges();

            AssignPermissionsToRoles(roles);

            this._dataContext.SaveChanges();
        }

        return this; // Assuming fluent interface is desired
    }

    // Helper method to assign permissions to roles
    private void AssignPermissionsToRoles(List<Roles> roles)
    {
        foreach (var role in roles)
        {
            switch (role.RoleName)
            {
                case "Default_Admin":
                    this._dataContext.AdminPermissions.Add(new AdminPermission
                    {
                        RoleId = role.Id,
                        CanCreate = false,
                        CanDelete = false,
                        CanEdit = false,
                        CanView = true
                    });
                    break;
                case "Default_SuperAdmin":
                case "Default_User":
                    this._dataContext.AdminPermissions.Add(new AdminPermission
                    {
                        RoleId = role.Id,
                        CanCreate = true,
                        CanDelete = true,
                        CanEdit = true,
                        CanView = true
                    });
                    break;
                default:
                    break;
            }
        }
    }


    public SystemInti SeedSuperAdmin()
    {
        if (this._dataContext.UserAccounts.FirstOrDefault(u => u.Username == "SuperAdmin") == null)
        {
            Roles adminRole = this._dataContext.Roles.FirstOrDefault(r => r.RoleType == UserRolesEnum.SuperAdmin.ToString());
            Roles userRole = this._dataContext.Roles.FirstOrDefault(r => r.RoleType == UserRolesEnum.User.ToString());

            if (adminRole == null || userRole == null)
            {
                throw new Exception("SuperAdmin or User role is missing.");
            }


            UserAccount superAdmin = new UserAccount
            {
                Name = "HuaXuen",
                Username = "SuperAdmin",
                PasswordHash = this._hash.Hash("password"),
                RoleId = adminRole.Id,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
            };

            UserAccount user = new UserAccount
            {
                Name = "ELTON",
                Username = "User",
                PasswordHash = this._hash.Hash("password"),
                RoleId = userRole.Id,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
            };

            this._dataContext.UserAccounts.Add(superAdmin);
            this._dataContext.UserAccounts.Add(user);
            this._dataContext.SaveChanges();

            UserDetail superAdminDetails = new UserDetail
            {
                UserId = superAdmin.Id,
                PhoneNumber = "601112810427",
                Gender = "M",
                Email = "Saleh@gamil.com",
                Age = 27,
                BillingAddress = "",
                CreatedAt = DateTime.Now,
            };

            UserDetail userDetails = new UserDetail
            {
                UserId = user.Id,
                PhoneNumber = "601121615114",
                Gender = "M",
                Email = "elton@gamil.com",
                Age = 22,
                BillingAddress = "",
                CreatedAt = DateTime.Now,
            };

            this._dataContext.UserDetails.Add(superAdminDetails);
            this._dataContext.UserDetails.Add(userDetails);
            this._dataContext.SaveChanges();
        }

        return this;
    }

    public override void seed()
    {
        this.SeedSuperAdmin();

    }
}

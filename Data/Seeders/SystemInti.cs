
using System.Data;
using PototoTrade.Enums;
using PototoTrade.Models;
using PototoTrade.Models.Role;
using PototoTrade.Models.User;
using PototoTrade.Service.Utilites.Hash;

namespace PototoTrade.Data.Seeders;

public class SystemInti : Seeder
{
    
    public SystemInti (DBC _dataContext, IHashing _hash) : base(_dataContext, _hash)
    { } 

    private SystemInti SeedRoles()
{
    if (!this._dataContext.Roles.Any())
    {
        var roles = Enum.GetValues(typeof(UserRolesEnum))
            .Cast<UserRolesEnum>()
            .Select(role => new Roles
            {
                RoleName = role.ToString(),
                Description = role switch
                {
                    UserRolesEnum.SuperAdmin => "Super Admin with all access and privileges",
                    UserRolesEnum.Admin => "Administrator with elevated privileges",
                    UserRolesEnum.User => "Regular user with standard access",
                    _ => "No description available"  
                },  
             RoleType = "superAdmin", //temp
             CreatedAt = DateTime.UtcNow
            })
            .ToList();

        this._dataContext.AddRange(roles);
        this._dataContext.SaveChanges();
    }

    return this;
}

   public SystemInti SeedSuperAdmin()
{
    if (this._dataContext.UserAccounts.FirstOrDefault(u => u.Username == "SuperAdmin") == null)
    {
        Roles adminRole = this._dataContext.Roles.FirstOrDefault(r => r.RoleName == UserRolesEnum.SuperAdmin.ToString());

        if (adminRole == null)
        {
            throw new Exception("SuperAdmin role is missing.");
        }

         
        UserAccount superAdmin = new UserAccount
        {
            Username = "SuperAdmin",
            PasswordHash = this._hash.Hash("password"),  
            RoleId = adminRole.Id,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
        };

        this._dataContext.UserAccounts.Add(superAdmin);
        this._dataContext.SaveChanges();

         UserDetail superAdminDetails = new  UserDetail  
        {  
            UserId = superAdmin.Id,  
            PhoneNumber= "601112810427",  
            Gender ="M",  
            Email = "Saleh@gamil.com",  
            Age =27,    
            BillingAddress = "",  
            CreatedAt = DateTime.Now,  
        };

        this._dataContext.UserDetails.Add(superAdminDetails);
        this._dataContext.SaveChanges();
    }

    return this;
}

    public override void seed()
    {
        this.SeedRoles().SeedSuperAdmin(); 

    }
}

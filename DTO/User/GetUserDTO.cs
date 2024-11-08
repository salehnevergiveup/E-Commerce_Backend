using PototoTrade.DTO.Product;
using PototoTrade.DTO.Role;
using PototoTrade.DTO.ShoppingCart;

namespace PototoTrade.DTO.User;

        
public class  GetUserDTO

{
    // User Information
    public int Id {get; set; }
    public int RoleId {get; set;} 
    public string UserName {get; set;}
    public string UserCover { get; set; }
    public string Avatar { get; set; }
    public string Name { get; set; }
    public string Gender {get; set;}
    public int Age {get; set;}
    public string Email { get; set; }
    public string Status { get; set; }
    public string? RoleName { get; set; }
    public string? RoleType  { get; set; }
    public string PhoneNumber { get; set; }
    public string BillingAddress { get; set; }
    public DateTime? CreatedAt { get; set; }

    public List<ProductDTO>? ProductList { get; set; } = new List<ProductDTO>();
    public List<UpdateViewRoleDTO>? Roles { get; set; } = new List<UpdateViewRoleDTO>();
}
using PototoTrade.DTO.Product;
using PototoTrade.DTO.Role;
using PototoTrade.DTO.ShoppingCart;
using PotatoTrade.DTO.MediaDTO;


namespace PototoTrade.DTO.User;

        
// Models/GetUserDTO.cs

public class GetUserDTO
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Gender { get; set; } // "M" or "F"
    public string Email { get; set; }
    public int Age { get; set; }
    public string BillingAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleName { get; set; }
    public string RoleType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductDTO> ProductList { get; set; }
    public List<UpdateViewRoleDTO> Roles { get; set; }
    public List<HandleMedia> Medias { get; set; } = new List<HandleMedia>(); // New Medias Array
}

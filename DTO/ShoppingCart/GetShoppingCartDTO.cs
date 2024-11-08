using PototoTrade.DTO.CartItem;
using PototoTrade.DTO.Role;

namespace PototoTrade.DTO.ShoppingCart;

public record class GetShoppingCartDTO
{
   public int Id {get; set;}
   public DateTime created_at {get;}
   public List<GetShoppingCartItemDTO> shoppingCartItems {get; set;} = new  List<GetShoppingCartItemDTO>(); 
   public UserRoleDTO? user {get; set;}
}

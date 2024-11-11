using PototoTrade.DTO.Product;

namespace PototoTrade.DTO.CartItem;

public record class GetShoppingCartItemDTO
{
    public int Id { get; set; }
    public string Status { get; set; }
    public GetProductDTO Product { get; set; }
}

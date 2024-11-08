using PototoTrade.DTO.Product;

namespace PototoTrade.DTO.CartItem;

public record class CreateCartItemDTO
{
    public int ProductId { get; set; }
}

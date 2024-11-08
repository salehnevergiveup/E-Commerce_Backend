namespace PototoTrade.DTO.ShoppingCart;

public record class ShoppingCartInfoDTO
{
    public int NumberOfItems {get; set;}
    public decimal TotalPrice {get; set;} 

}

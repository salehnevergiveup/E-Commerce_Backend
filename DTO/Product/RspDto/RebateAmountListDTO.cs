public class RebateAmountDTO
{
    public string ProductName { get; set; } = null!;

    public int  ProductId { get; set; } 
    public decimal RebateRate { get; set; } 
    public decimal FinalPrice { get; set; } 

    public decimal DeliveryFee {get; set;}
}

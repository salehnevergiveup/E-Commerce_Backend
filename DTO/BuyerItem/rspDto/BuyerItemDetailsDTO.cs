public class BuyerItemDetailsDTO
{
    public int BuyerItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductUrl { get; set; } 
    public string ProductOwner { get; set; } = null!;
    public string BuyerItemStatus { get; set; } = null!;

    public int PurchaseOrderId { get; set;} 

    public bool RefundableBoolean { get; set; } 

    public int RemainingRefundDays { get; set; }     
    public List<BuyerItemDeliveryDTO> BuyerItemDelivery { get; set; } = new List<BuyerItemDeliveryDTO>();
}

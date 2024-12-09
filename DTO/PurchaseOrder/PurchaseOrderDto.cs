public class PurchaseOrderDTO
{
    public int PurchaseOrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderCreatedAt { get; set; }
    public List<BuyerItemDTO> BuyerItems { get; set; } = new List<BuyerItemDTO>();
}

public class RebateAmountListDTO
{
    public int PurchaseOrderId { get; set; }
    public decimal FinalPrice { get; set; } 
    public List<RebateAmountDTO> Items { get; set; } = new List<RebateAmountDTO>();
}

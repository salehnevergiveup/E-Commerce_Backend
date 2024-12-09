public class MakePaymentRequestDTO
{
    public int PurchaseOrderId { get; set; }
    public decimal FinalPrice { get; set; }
    public List<RebateAmountDTO> RebateItems { get; set; } = new List<RebateAmountDTO>();
}

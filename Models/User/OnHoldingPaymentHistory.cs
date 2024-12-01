public class OnHoldingPaymentHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int BuyerItemId { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

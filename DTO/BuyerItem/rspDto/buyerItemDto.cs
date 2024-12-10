public class BuyerItemDTO
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }

    public string? ProductUrl { get; set; } = null!;

    public string ProductOwner { get; set; } = null!;

    public string  BuyerItemStatus { get; set; } = null!;
}

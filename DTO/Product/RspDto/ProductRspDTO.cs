public class ProductResponseDTO
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductTitle { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public string ProductStatus { get; set; } = null!;
    public string ProductPaymentStatus { get; set; } = null!;

    public string? MediaUrl { get; set; } 
}

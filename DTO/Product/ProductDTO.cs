namespace PototoTrade.DTO.Product;

public record class GetProductDTO
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; } // Assuming the image URL or path
    public DateTime CreatedAt { get; set; }
    public int RefundGuaranteedDuration { get; set; }
    public string Status { get; set; }
    public string Title { get; set; }
}

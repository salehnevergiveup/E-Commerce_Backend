public class ProductDetailsDTO
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int RefundGuaranteedDuration { get; set; }

    public string ProductCategoryName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } // From UserAccount
    public List<MediaDTO>? Media { get; set; } = new List<MediaDTO>();
}

public class CreateProductDTO
{
    

    public int CategoryId { get; set; }
    public bool MediaBoolean { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int RefundGuaranteedDuration { get; set; }
    public List<MediaDTO>? Media { get; set; }
}

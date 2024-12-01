public class ProductCategoryListDTO
{
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; }
    public double ChargeRate { get; set; } // Percentage, e.g., 10 for 10%
    public double RebateRate { get; set; } // Percentage, e.g., 5 for 5%
    public int NumberOfItems { get; set; } // Count of products with status "available"
}

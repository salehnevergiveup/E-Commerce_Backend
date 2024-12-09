namespace PototoTrade.DTO.Product
{
    public class CreateProductCategoryDTO
    {
        public string ProductCategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public double ChargeRate { get; set; } // percentage, e.g., 10 for 10%
        public double RebateRate { get; set; } // percentage, e.g., 5 for 5%
    }
}

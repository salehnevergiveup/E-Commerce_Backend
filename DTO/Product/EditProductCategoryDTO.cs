namespace PototoTrade.DTO.Product
{
    public class EditProductCategoryDTO
    {
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; } = null!;
        public double ChargeRate { get; set; } 
        public double RebateRate { get; set; } 
    }
}

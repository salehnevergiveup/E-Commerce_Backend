namespace PototoTrade.DTO.ReportDTO;

public record class ProductCategoryReportDTO
{
    public string CategoryName { get; set; }
    public int TotalProducts { get; set; }
    public decimal AveragePrice { get; set; }
}

namespace PototoTrade.DTO.ReportDTO;

public record class SalesReportDTO
{
    public string GroupingKey { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalSalesAmount { get; set; }

}

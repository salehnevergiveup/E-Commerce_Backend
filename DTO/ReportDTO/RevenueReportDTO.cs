namespace PototoTrade.DTO.ReportDTO;

public record class RevenueReportDTO
{
    public string GroupingKey { get; set; }
    public decimal TotalRevenue { get; set; }
}

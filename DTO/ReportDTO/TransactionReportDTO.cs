namespace PototoTrade.DTO.ReportDTO;

public record class TransactionReportDTO
{
    public string GroupingKey { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalTransactionAmount { get; set; }
}

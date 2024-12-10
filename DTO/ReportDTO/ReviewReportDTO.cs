namespace PototoTrade.DTO.ReportDTO;

public record class ReviewReportDTO
{
    public string GroupingKey { get; set; }
    public int GoodReviews { get; set; }  
    public int BadReviews { get; set; }
}

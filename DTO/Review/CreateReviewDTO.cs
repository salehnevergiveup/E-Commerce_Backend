namespace PotatoTrade.DTO.Review
{
    public class CreateReviewDTO
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? ReviewComment { get; set; }
        public string? MediaUrl { get; set; }
    }
}

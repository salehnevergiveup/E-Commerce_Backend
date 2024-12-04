namespace PotatoTrade.DTO.Review
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public bool MediaBoolean { get; set; }
        public int Rating { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? MediaUrl { get; set; }
    }
}

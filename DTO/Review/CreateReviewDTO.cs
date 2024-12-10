using PotatoTrade.DTO.MediaDTO;

namespace PotatoTrade.DTO.Review
{
    public class CreateReviewDTO
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? ReviewComment { get; set; }
        public List<HandleMedia> Medias { get; set; } = new List<HandleMedia>(); 

    }
}

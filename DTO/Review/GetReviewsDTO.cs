using PotatoTrade.DTO.MediaDTO;
using PotatoTrade.DTO.User;
using PototoTrade.DTO.Product;

namespace PotatoTrade.DTO.Review
{
    public class GetReviewDTO
    {
        public int Id {get; set;}
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? ReviewComment { get; set; }

        public GetUserListDTO User { get; set; }
        public ProductDTO Product { get; set; }
        public List<HandleMedia> Medias { get; set; } = new List<HandleMedia>();

    }
}

// PotatoTrade.DTO.Media.MediaDTO.cs

using System;

namespace PotatoTrade.DTO.MediaDTO
{
    public class HandleMedia
    {
        public int ?Id { get; set; } // Unique identifier for the media
        public string? Type { get; set; } 
        public string MediaUrl { get; set; } // URL to the media
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

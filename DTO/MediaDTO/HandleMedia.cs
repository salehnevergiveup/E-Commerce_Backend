// PotatoTrade.DTO.Media.MediaDTO.cs

using System;

namespace PotatoTrade.DTO.MediaDTO
{
    public class HandleMedia
    {
        public int ?Id { get; set; } 
        public int ?SourceId {get; set;} =  0;  
        public string? Type { get; set; } 
        public string MediaUrl { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

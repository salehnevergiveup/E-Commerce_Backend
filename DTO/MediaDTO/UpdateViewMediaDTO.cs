using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.MediaDTO;

public class UpdateViewMediaDTO
{
    [Required]
    public int Id {get; set;}

    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; } = 0;

    public string MediaUrl { get; set; } = string.Empty;

}

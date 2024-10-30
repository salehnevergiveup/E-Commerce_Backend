using System;
using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Asn1.Cms;

namespace PototoTrade.DTO.MediaDTO;

public class CreateMediaDTO
{
    [Required]
    public string SourceType { get; set; } = string.Empty;

    [Required]
    public string MediaUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

}

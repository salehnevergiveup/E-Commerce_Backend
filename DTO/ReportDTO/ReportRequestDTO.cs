using System.ComponentModel.DataAnnotations;
using PototoTrade.Repositories;

namespace PototoTrade.DTO.ReportDTO;

public record class ReportRequestDTO
{
        [Required]
        public string TimeFrame { get; set; }
}

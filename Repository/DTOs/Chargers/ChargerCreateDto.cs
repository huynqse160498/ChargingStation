using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Chargers
{
    public class ChargerCreateDto
    {
        [Required]
        public int StationId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        public decimal? PowerKw { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime? InstalledAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}

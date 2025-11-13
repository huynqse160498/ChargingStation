using System;
using System.Collections.Generic;   
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Ports
{
    public class PortCreateDto
    {
        [Required]
        public int ChargerId { get; set; }

        [Required, MaxLength(50)]
        public string ConnectorType { get; set; } = string.Empty; // CCS/CHAdeMO/Type2/...

        public decimal? MaxPowerKw { get; set; }
        public string Status { get; set; } = "Available";

        //[MaxLength(255)]
        //public string? ImageUrl { get; set; }
    }
}

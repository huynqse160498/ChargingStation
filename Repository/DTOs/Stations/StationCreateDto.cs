using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Stations
{
    public class StationCreateDto
    {
        [Required, MaxLength(200)]
        public string StationName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Db: decimal(9,6)
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(255)]
        public string? ImageUrl { get; set; }
    }
}

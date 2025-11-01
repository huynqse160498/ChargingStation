using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Feedbacks
{
    public class FeedbackCreateDto
    {
        [Required] public int CustomerId { get; set; }
        public int? StationId { get; set; }
        public int? ChargerId { get; set; }
        public int? PortId { get; set; }

        [Range(1, 5)] public int Rating { get; set; }
        [Required, MinLength(1)] public string Comment { get; set; } = null!;
    }
}

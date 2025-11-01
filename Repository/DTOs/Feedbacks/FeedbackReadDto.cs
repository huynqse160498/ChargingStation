using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Feedbacks
{
    public class FeedbackReadDto
    {
        public int FeedbackId { get; set; }
        public int CustomerId { get; set; }
        public int? StationId { get; set; }
        public int? ChargerId { get; set; }
        public int? PortId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // convenience for UI
        public string? CustomerName { get; set; }
        public string? StationName { get; set; }
        public string? ChargerCode { get; set; }
        public string? PortCode { get; set; }
    }
}

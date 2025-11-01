using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        public int CustomerId { get; set; }

        public int? StationId { get; set; }
        public int? ChargerId { get; set; }
        public int? PortId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Customer Customer { get; set; }

        public virtual Station Station { get; set; }

        public virtual Charger Charger { get; set; }

        public virtual Port Port { get; set; }
    }
}

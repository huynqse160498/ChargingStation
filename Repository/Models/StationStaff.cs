using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models
{
    public class StationStaff
    {
        public int StationId { get; set; }
        public int StaffId { get; set; }

        public virtual Station Station { get; set; }

        public virtual Account Staff { get; set; }
    }
}

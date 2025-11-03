using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.StationStaff
{
    public class StationStaffReadDto
    {
        public int StationId { get; set; }
        public int StaffId { get; set; }

       
        public string? StaffName { get; set; }
        public string? StaffEmail { get; set; }
    }
}

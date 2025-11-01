using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Reports
{
    public class ReportUpdateDto
    {
        // cho phép cập nhật tiêu đề/mô tả nếu cần
        public string? Title { get; set; }
        public string? Description { get; set; }

        // cập nhật mức độ
        public string? Severity { get; set; }

        // cập nhật trạng thái: Pending/InProgress/Resolved/Closed...
        public string? Status { get; set; }

        // set khi resolve
        public DateTime? ResolvedAt { get; set; }
    }
}

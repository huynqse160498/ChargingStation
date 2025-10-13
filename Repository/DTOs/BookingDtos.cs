using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public static class BookingDtos
    {
        // ===== CREATE =====
        public class Create
        {
            [Required(ErrorMessage = "CustomerId không được bỏ trống")]
            public int CustomerId { get; set; }

            [Required(ErrorMessage = "VehicleId không được bỏ trống")]
            public int VehicleId { get; set; }

            [Required(ErrorMessage = "PortId không được bỏ trống")]
            public int PortId { get; set; }

            [Required(ErrorMessage = "Thời gian bắt đầu không được bỏ trống")]
            public DateTime? StartTime { get; set; }

            [Required(ErrorMessage = "Thời gian kết thúc không được bỏ trống")]
            public DateTime? EndTime { get; set; }

            [Required(ErrorMessage = "Trạng thái không được bỏ trống")]
            [RegularExpression("Pending|Confirmed|Cancelled|Completed",
                ErrorMessage = "Trạng thái chỉ được là Pending, Confirmed, Cancelled hoặc Completed")]
            public string Status { get; set; } = "Pending";
        }

        // ===== UPDATE =====
        public class Update
        {
            [Required(ErrorMessage = "VehicleId không được bỏ trống")]
            public int VehicleId { get; set; }

            [Required(ErrorMessage = "PortId không được bỏ trống")]
            public int PortId { get; set; }

            [Required(ErrorMessage = "Thời gian bắt đầu không được bỏ trống")]
            public DateTime? StartTime { get; set; }

            [Required(ErrorMessage = "Thời gian kết thúc không được bỏ trống")]
            public DateTime? EndTime { get; set; }

            [Required(ErrorMessage = "Trạng thái không được bỏ trống")]
            [RegularExpression("Pending|Confirmed|Cancelled|Completed",
                ErrorMessage = "Trạng thái chỉ được là Pending, Confirmed, Cancelled hoặc Completed")]
            public string Status { get; set; } = "Pending";
        }
        public class ChangeStatus
        {
            [Required(ErrorMessage = "Trạng thái không được bỏ trống")]
            [RegularExpression("Pending|Confirmed|InProgress|Completed|Cancelled",
                ErrorMessage = "Trạng thái chỉ được là Pending, Confirmed, InProgress, Completed hoặc Cancelled")]
            public string Status { get; set; }
        }
        // ===== QUERY (Phân trang & tìm kiếm) =====
        public class Query
        {
            [Range(1, int.MaxValue, ErrorMessage = "Số trang phải >= 1")]
            public int Page { get; set; } = 1;

            [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 - 100")]
            public int PageSize { get; set; } = 10;

            public int? CustomerId { get; set; }
            public int? VehicleId { get; set; }
            public int? PortId { get; set; }
            public string? Status { get; set; }
            public string? Search { get; set; }

            public string? SortBy { get; set; } = "CreatedAt";
            public string? SortDir { get; set; } = "desc";
        }

        // ===== LIST ITEM =====
        public class ListItem
        {
            public int BookingId { get; set; }
            public int CustomerId { get; set; }
            public int VehicleId { get; set; }
            public int PortId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal? Price { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        // ===== DETAIL =====
        public class Detail : ListItem
        {
            public DateTime? UpdatedAt { get; set; }
        }
    }

    // ===== PHÂN TRANG CHUNG =====
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new();
    }
        
}

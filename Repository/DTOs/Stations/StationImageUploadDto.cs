using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Stations
{
    public class StationImageUploadDto
    {
        [Required]
        public int StationId { get; set; } // ID trạm cần upload ảnh

        [Required]
        public IFormFile File { get; set; } = default!; // file hình (Swagger sẽ hiện Choose File)

        [MaxLength(255)]
        public string? FileName { get; set; } // tùy chọn - tên hiển thị file

        public bool? IsMain { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Vehicles
{
    public class VehicleImageUploadDto
    {
        [Required]
        public int VehicleId { get; set; } // ID xe cần upload ảnh

        [Required]
        public IFormFile File { get; set; } = default!; // sẽ hiện "Choose File" trên Swagger

        [MaxLength(255)]
        public string? FileName { get; set; }

        public bool? IsMain { get; set; }
    }
}

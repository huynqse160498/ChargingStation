using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Ports
{
    public class PortImageUploadDto
    {
        [Required]
        public int PortId { get; set; }                 

        [Required]
        public IFormFile File { get; set; } = default!; // NEW (Swagger hiện Choose File)

        [MaxLength(255)]
        public string? FileName { get; set; }           // optional

        public bool? IsMain { get; set; }
    }
}

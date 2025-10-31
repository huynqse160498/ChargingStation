using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "Thiếu mã IdToken từ Google.")]
        public string IdToken { get; set; }
    }
}

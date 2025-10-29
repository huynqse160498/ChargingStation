using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; }

        [Required, MinLength(8)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}

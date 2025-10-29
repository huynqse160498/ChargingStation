using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc email là bắt buộc")]
        public string UserNameOrEmail { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Mật khẩu cũ không được bỏ trống")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được bỏ trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}

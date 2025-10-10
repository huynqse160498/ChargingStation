using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Họ tên không được bỏ trống")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được bỏ trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "CustomerId là bắt buộc")]
        public int CustomerId { get; set; }

        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string Address { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } // ✅ thêm dòng này

    }
}

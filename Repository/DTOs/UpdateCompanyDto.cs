using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class UpdateCompanyDto
    {
        [Required(ErrorMessage = "CompanyId là bắt buộc")]
        public int CompanyId { get; set; }

        [StringLength(150, ErrorMessage = "Tên công ty không được vượt quá 150 ký tự")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessage = "Mã số thuế không được vượt quá 20 ký tự")]
        public string TaxCode { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string Address { get; set; }

        [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ")]
        public string ImageUrl { get; set; }

     
    }
}

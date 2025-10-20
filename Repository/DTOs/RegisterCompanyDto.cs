using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class RegisterCompanyDto
    {
        [Required, MinLength(3)]
        public string UserName { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Required, Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Required] public string CompanyName { get; set; }
        [Required] public string TaxCode { get; set; }
        [Required, EmailAddress] public string CompanyEmail { get; set; }
        [Required, Phone] public string CompanyPhone { get; set; }
        [Required] public string Address { get; set; }

        public string ImageUrl { get; set; }
    }
}

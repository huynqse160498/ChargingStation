using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class ChargingSessionPaymentCreateDto
    {
        [Required] public int ChargingSessionId { get; set; }
        [Required, RegularExpression("^(Cash|VNPAY)$")]
        public string Method { get; set; } = "Cash";
    }
}

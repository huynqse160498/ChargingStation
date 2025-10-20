using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.SubscriptionPlans
{
    public class SubscriptionPlanCreateDto
    {
        [Required, MaxLength(100)]
        public string PlanName { get; set; } = string.Empty;  

        [MaxLength(500)]
        public string? Description { get; set; }              

        [Required, RegularExpression("Individual|Business")]
        public string Category { get; set; } = "Individual";  

        [Range(0, double.MaxValue)]
        public decimal PriceMonthly { get; set; }            

        [Range(0, double.MaxValue)]
        public decimal? PriceYearly { get; set; }           

        [Range(0, 100)]
        public decimal? DiscountPercent { get; set; }        

        [Range(0, int.MaxValue)]
        public int? FreeIdleMinutes { get; set; }            

        public string? Benefits { get; set; }                
        public bool IsForCompany { get; set; }               

        [RegularExpression("Active|Inactive")]
        public string Status { get; set; } = "Active";       
    }
}

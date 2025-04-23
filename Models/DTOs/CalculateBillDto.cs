using System.ComponentModel.DataAnnotations;

namespace SE4458_Midterm_20070006074.Models.DTOs
{
    public class CalculateBillDto
    {
        [Required]
        public int SubscriberNo { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }
    }

    public class BillCalculationResult
    {
        public decimal PhoneUsage { get; set; }
        public decimal InternetUsage { get; set; }
        public decimal PhoneCost { get; set; }
        public decimal InternetCost { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 
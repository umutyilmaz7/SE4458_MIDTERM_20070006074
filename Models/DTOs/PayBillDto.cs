using System.ComponentModel.DataAnnotations;

namespace SE4458_Midterm_20070006074.Models.DTOs
{
    public class PayBillDto
    {
        [Required]
        public int SubscriberNo { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal PaymentAmount { get; set; }
    }

    public class PaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 
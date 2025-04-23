using System.ComponentModel.DataAnnotations;

namespace SE4458_Midterm_20070006074.Models.DTOs
{
    public class AddUsageDto
    {
        [Required]
        public int SubscriberNo { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        [RegularExpression("^(Phone|Internet)$", ErrorMessage = "Usage Type must be either 'Phone' or 'Internet'")]
        public string UsageType { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }
    }
} 
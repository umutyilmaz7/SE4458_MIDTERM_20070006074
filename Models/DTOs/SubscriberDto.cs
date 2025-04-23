using System.ComponentModel.DataAnnotations;

namespace SE4458_Midterm_20070006074.Models.DTOs
{
    public class CreateSubscriberDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
    }
} 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE4458_Midterm_20070006074.Models
{
    public class Usage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsageId { get; set; }

        [Required]
        public int SubscriberId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(20)]
        public string UsageType { get; set; } = string.Empty; // "Phone" or "Internet"

        [Required]
        public decimal Amount { get; set; } // Minutes for Phone, MB for Internet

        // Navigation property
        public virtual Subscriber? Subscriber { get; set; }
    }
} 
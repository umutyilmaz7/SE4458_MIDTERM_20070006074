using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE4458_Midterm_20070006074.Models
{
    public class Bill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillId { get; set; }

        [Required]
        public int SubscriberId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public virtual Subscriber? Subscriber { get; set; }
        public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
} 
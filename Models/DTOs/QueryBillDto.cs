using System.ComponentModel.DataAnnotations;

namespace SE4458_Midterm_20070006074.Models.DTOs
{
    public class QueryBillDto
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

    public class PagedQueryBillDto : QueryBillDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class CreateBillDto
    {
        [Required]
        public int SubscriberId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public List<CreateBillDetailDto> BillDetails { get; set; } = new List<CreateBillDetailDto>();
    }

    public class CreateBillDetailDto
    {
        [Required]
        public string UsageType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
        public decimal TotalPrice { get; set; }
    }

    public class BillQueryResult
    {
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Detailed version
    public class BillDetailedQueryResult
    {
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<BillDetailResult> Details { get; set; } = new List<BillDetailResult>();
    }

    public class PagedBillDetailedQueryResult : BillDetailedQueryResult
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    public class BillDetailResult
    {
        public string UsageType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
} 
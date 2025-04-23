using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using SE4458_Midterm_20070006074.Models;
using SE4458_Midterm_20070006074.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SE4458_Midterm_20070006074.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillController> _logger;

        public BillController(ApplicationDbContext context, ILogger<BillController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetAll()
        {
            try
            {
                _logger.LogInformation("V2: Fetching all bills with related data");
                
                var bills = await _context.Bills
                    .Include(b => b.Subscriber)
                    .Include(b => b.BillDetails)
                    .Include(b => b.Payments)
                    .OrderByDescending(b => b.IssueDate) // V2: Added sorting
                    .ToListAsync();

                _logger.LogInformation($"V2: Found {bills.Count} bills");
                
                if (!bills.Any())
                {
                    _logger.LogWarning("No bills found in the database");
                    return new List<Bill>();
                }

                return bills;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bills");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // V2: New endpoint for getting bills by date range
        [HttpGet("ByDateRange")]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBillsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                _logger.LogInformation($"V2: Fetching bills between {startDate} and {endDate}");

                var bills = await _context.Bills
                    .Include(b => b.Subscriber)
                    .Include(b => b.BillDetails)
                    .Include(b => b.Payments)
                    .Where(b => b.IssueDate >= startDate && b.IssueDate <= endDate)
                    .OrderByDescending(b => b.IssueDate)
                    .ToListAsync();

                _logger.LogInformation($"V2: Found {bills.Count} bills in date range");

                return bills;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bills by date range");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // V2: New endpoint for getting bills summary
        [HttpGet("Summary")]
        public async Task<ActionResult<BillsSummaryResult>> GetBillsSummary()
        {
            try
            {
                var summary = new BillsSummaryResult
                {
                    TotalBillCount = await _context.Bills.CountAsync(),
                    TotalPaidAmount = await _context.Bills.SumAsync(b => b.PaidAmount),
                    TotalUnpaidAmount = await _context.Bills.SumAsync(b => b.TotalAmount - b.PaidAmount),
                    BillsByStatus = await _context.Bills
                        .GroupBy(b => b.Status)
                        .Select(g => new BillStatusSummary
                        {
                            Status = g.Key,
                            Count = g.Count(),
                            TotalAmount = g.Sum(b => b.TotalAmount)
                        })
                        .ToListAsync()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bills summary");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

// V2: New DTOs for the summary endpoint
public class BillsSummaryResult
{
    public int TotalBillCount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalUnpaidAmount { get; set; }
    public List<BillStatusSummary> BillsByStatus { get; set; } = new List<BillStatusSummary>();
}

public class BillStatusSummary
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
} 
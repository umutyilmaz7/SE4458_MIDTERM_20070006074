using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using SE4458_Midterm_20070006074.Models;
using SE4458_Midterm_20070006074.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SE4458_Midterm_20070006074.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillController> _logger;
        private const decimal FREE_PHONE_MINUTES = 1000;
        private const decimal PHONE_RATE_PER_1000_MINUTES = 10;
        private const decimal INTERNET_BASE_RATE = 50;
        private const decimal INTERNET_ADDITIONAL_RATE = 10;
        private const decimal INTERNET_BASE_LIMIT_GB = 20;
        private const decimal INTERNET_ADDITIONAL_BLOCK_GB = 10;

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
                _logger.LogInformation("Fetching all bills with related data");
                
                var bills = await _context.Bills
                    .Include(b => b.Subscriber)
                    .Include(b => b.BillDetails)
                    .Include(b => b.Payments)
                    .ToListAsync();

                _logger.LogInformation($"Found {bills.Count} bills");
                
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

        [HttpGet("QueryBill")]
        public async Task<ActionResult<BillQueryResult>> QueryBill([FromQuery] QueryBillDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberNo);
                if (subscriber == null)
                {
                    return NotFound($"Subscriber with ID {request.SubscriberNo} not found");
                }

                // Get bill for the specified month and year
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1);

                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.SubscriberId == request.SubscriberNo &&
                                            b.IssueDate >= startDate &&
                                            b.IssueDate < endDate);

                if (bill == null)
                {
                    return NotFound($"No bill found for subscriber {request.SubscriberNo} for {request.Month}/{request.Year}");
                }

                var result = new BillQueryResult
                {
                    TotalAmount = bill.TotalAmount,
                    Status = bill.Status
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("QueryBillDetailed")]
        public async Task<ActionResult<PagedBillDetailedQueryResult>> QueryBillDetailed([FromQuery] PagedQueryBillDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberNo);
                if (subscriber == null)
                {
                    return NotFound($"Subscriber with ID {request.SubscriberNo} not found");
                }

                // Get bill for the specified month and year
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1);

                var bill = await _context.Bills
                    .Include(b => b.BillDetails)
                    .FirstOrDefaultAsync(b => b.SubscriberId == request.SubscriberNo &&
                                            b.IssueDate >= startDate &&
                                            b.IssueDate < endDate);

                if (bill == null)
                {
                    return NotFound($"No bill found for subscriber {request.SubscriberNo} for {request.Month}/{request.Year}");
                }

                // Get total count of bill details
                var totalCount = bill.BillDetails.Count;

                // Calculate pagination
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
                var currentPage = Math.Max(1, Math.Min(request.PageNumber, totalPages));

                // Get paginated bill details
                var paginatedDetails = bill.BillDetails
                    .Skip((currentPage - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(bd => new BillDetailResult
                    {
                        UsageType = bd.UsageType,
                        Amount = bd.Amount,
                        UnitPrice = bd.UnitPrice,
                        TotalPrice = bd.TotalPrice
                    })
                    .ToList();

                var result = new PagedBillDetailedQueryResult
                {
                    TotalAmount = bill.TotalAmount,
                    Status = bill.Status,
                    Details = paginatedDetails,
                    CurrentPage = currentPage,
                    TotalPages = totalPages,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CalculateBill")]
        public async Task<ActionResult<BillCalculationResult>> CalculateBill(CalculateBillDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberNo);
                if (subscriber == null)
                {
                    return NotFound($"Subscriber with ID {request.SubscriberNo} not found");
                }

                // Get usages for the specified month and year
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1);

                var usages = await _context.Usages
                    .Where(u => u.SubscriberId == request.SubscriberNo &&
                               u.Date >= startDate &&
                               u.Date < endDate)
                    .ToListAsync();

                var result = new BillCalculationResult();

                // Calculate phone usage and cost
                var phoneUsage = usages
                    .Where(u => u.UsageType == "Phone")
                    .Sum(u => u.Amount);

                result.PhoneUsage = phoneUsage;
                result.PhoneCost = phoneUsage <= FREE_PHONE_MINUTES ? 0 :
                    Math.Ceiling((phoneUsage - FREE_PHONE_MINUTES) / 1000) * PHONE_RATE_PER_1000_MINUTES;

                // Calculate internet usage and cost
                var internetUsageBytes = usages
                    .Where(u => u.UsageType == "Internet")
                    .Sum(u => u.Amount);
                
                // Convert bytes to GB
                var internetUsageGB = internetUsageBytes / (1024 * 1024 * 1024);
                result.InternetUsage = internetUsageGB;

                // Calculate internet cost
                if (internetUsageGB <= INTERNET_BASE_LIMIT_GB)
                {
                    result.InternetCost = INTERNET_BASE_RATE;
                }
                else
                {
                    var additionalGB = internetUsageGB - INTERNET_BASE_LIMIT_GB;
                    var additionalBlocks = Math.Ceiling(additionalGB / INTERNET_ADDITIONAL_BLOCK_GB);
                    result.InternetCost = INTERNET_BASE_RATE + (additionalBlocks * INTERNET_ADDITIONAL_RATE);
                }

                result.TotalAmount = result.PhoneCost + result.InternetCost;

                // Create bill record
                var bill = new Bill
                {
                    SubscriberId = request.SubscriberNo,
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    TotalAmount = result.TotalAmount,
                    Status = "Pending"
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                // Create bill details
                if (phoneUsage > 0)
                {
                    var phoneDetail = new BillDetail
                    {
                        BillId = bill.BillId,
                        UsageType = "Phone",
                        Amount = phoneUsage,
                        UnitPrice = PHONE_RATE_PER_1000_MINUTES / 1000,
                        TotalPrice = result.PhoneCost
                    };
                    _context.BillDetails.Add(phoneDetail);
                }

                if (internetUsageGB > 0)
                {
                    var internetDetail = new BillDetail
                    {
                        BillId = bill.BillId,
                        UsageType = "Internet",
                        Amount = internetUsageGB,
                        UnitPrice = INTERNET_BASE_RATE / INTERNET_BASE_LIMIT_GB,
                        TotalPrice = result.InternetCost
                    };
                    _context.BillDetails.Add(internetDetail);
                }

                await _context.SaveChangesAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("PayBill")]
        public async Task<ActionResult<PaymentResult>> PayBill(PayBillDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberNo);
                if (subscriber == null)
                {
                    return NotFound(new PaymentResult 
                    { 
                        IsSuccessful = false,
                        Status = "Error",
                        Message = $"Subscriber with ID {request.SubscriberNo} not found"
                    });
                }

                // Get bill for the specified month and year
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1);

                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.SubscriberId == request.SubscriberNo &&
                                            b.IssueDate >= startDate &&
                                            b.IssueDate < endDate);

                if (bill == null)
                {
                    return NotFound(new PaymentResult 
                    { 
                        IsSuccessful = false,
                        Status = "Error",
                        Message = $"No bill found for subscriber {request.SubscriberNo} for {request.Month}/{request.Year}"
                    });
                }

                // Calculate remaining amount after payment
                var remainingAmount = bill.TotalAmount - bill.PaidAmount;
                if (remainingAmount <= 0)
                {
                    return BadRequest(new PaymentResult 
                    { 
                        IsSuccessful = false,
                        Status = "Error",
                        Message = "Bill is already fully paid",
                        PaidAmount = bill.PaidAmount,
                        RemainingAmount = 0
                    });
                }

                // Update bill payment status
                bill.PaidAmount += request.PaymentAmount;
                bill.Status = bill.PaidAmount >= bill.TotalAmount ? "Paid" : "Partially Paid";

                await _context.SaveChangesAsync();

                // Calculate new remaining amount
                var newRemainingAmount = bill.TotalAmount - bill.PaidAmount;

                return Ok(new PaymentResult 
                { 
                    IsSuccessful = true,
                    Status = bill.Status,
                    PaidAmount = bill.PaidAmount,
                    RemainingAmount = newRemainingAmount >= 0 ? newRemainingAmount : 0,
                    Message = bill.Status == "Paid" ? 
                        "Bill has been fully paid" : 
                        $"Payment successful. Remaining amount: {newRemainingAmount:C2}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentResult 
                { 
                    IsSuccessful = false,
                    Status = "Error",
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Bill>> CreateBill(CreateBillDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberId);
                if (subscriber == null)
                {
                    return NotFound($"Subscriber with ID {request.SubscriberId} not found");
                }

                // Validate bill details
                if (request.BillDetails.Any())
                {
                    var totalAmountFromDetails = request.BillDetails.Sum(bd => bd.TotalPrice);
                    if (Math.Abs(totalAmountFromDetails - request.TotalAmount) > 0.01m)
                    {
                        return BadRequest("Total amount does not match the sum of bill details");
                    }
                }

                // Create new bill
                var bill = new Bill
                {
                    SubscriberId = request.SubscriberId,
                    IssueDate = request.IssueDate,
                    DueDate = request.DueDate,
                    TotalAmount = request.TotalAmount,
                    Status = "Pending",
                    PaidAmount = 0
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                // Add bill details if any
                if (request.BillDetails.Any())
                {
                    var billDetails = request.BillDetails.Select(bd => new BillDetail
                    {
                        BillId = bill.BillId,
                        UsageType = bd.UsageType,
                        Amount = bd.Amount,
                        UnitPrice = bd.UnitPrice,
                        TotalPrice = bd.TotalPrice
                    }).ToList();

                    _context.BillDetails.AddRange(billDetails);
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetAll), new { id = bill.BillId }, bill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 
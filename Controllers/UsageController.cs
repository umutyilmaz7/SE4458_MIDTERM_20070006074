using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using SE4458_Midterm_20070006074.Models;
using SE4458_Midterm_20070006074.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SE4458_Midterm_20070006074.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usage>>> GetAll()
        {
            return await _context.Usages.ToListAsync();
        }

        [HttpPost("AddUsage")]
        public async Task<ActionResult<string>> AddUsage(AddUsageDto request)
        {
            try
            {
                // Check if subscriber exists
                var subscriber = await _context.Subscribers.FindAsync(request.SubscriberNo);
                if (subscriber == null)
                {
                    return NotFound($"Subscriber with ID {request.SubscriberNo} not found");
                }

                // Create usage record
                var usage = new Usage
                {
                    SubscriberId = request.SubscriberNo,
                    UsageType = request.UsageType,
                    Date = new DateTime(DateTime.Now.Year, request.Month, 1),
                    Amount = request.UsageType == "Phone" ? request.Amount * 10 : // Convert to minutes for phone
                            request.UsageType == "Internet" ? request.Amount * 1024 * 1024 : // Convert to MB for internet
                            request.Amount
                };

                _context.Usages.Add(usage);
                await _context.SaveChangesAsync();

                return Ok("Usage added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 
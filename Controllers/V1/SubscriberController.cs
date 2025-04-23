using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using SE4458_Midterm_20070006074.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SE4458_Midterm_20070006074.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SubscriberController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriberController> _logger;

        public SubscriberController(ApplicationDbContext context, ILogger<SubscriberController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscriber>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all subscribers with related data");
                
                var subscribers = await _context.Subscribers
                    .Include(s => s.Bills)
                        .ThenInclude(b => b.BillDetails)
                    .Include(s => s.Usages)
                    .ToListAsync();

                _logger.LogInformation($"Found {subscribers.Count} subscribers");
                
                if (!subscribers.Any())
                {
                    _logger.LogWarning("No subscribers found in the database");
                    return new List<Subscriber>();
                }

                return subscribers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching subscribers");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 
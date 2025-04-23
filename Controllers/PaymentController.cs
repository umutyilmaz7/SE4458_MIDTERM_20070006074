using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SE4458_Midterm_20070006074.Data;
using SE4458_Midterm_20070006074.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SE4458_Midterm_20070006074.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAll()
        {
            return await _context.Payments.ToListAsync();
        }
    }
} 
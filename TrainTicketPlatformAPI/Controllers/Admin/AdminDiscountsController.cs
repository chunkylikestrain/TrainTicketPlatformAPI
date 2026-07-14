using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Security;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/discounts")]
    public class AdminDiscountsController : ControllerBase
    {
        private readonly TrainTicketDbContext _db;

        public AdminDiscountsController(TrainTicketDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminDiscountDto>>> GetAll()
        {
            var discounts = await _db.DiscountRules.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
            return Ok(discounts.Select(ToDto));
        }

        [HttpPost]
        public async Task<ActionResult<AdminDiscountDto>> Create(AdminDiscountDto request)
        {
            var discount = new DiscountRule
            {
                Name = request.Name.Trim(),
                Percent = request.Percent,
                EligibleClass = request.EligibleClass,
                DocumentHint = request.DocumentHint,
                Status = request.Status
            };

            _db.DiscountRules.Add(discount);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = discount.Id }, ToDto(discount));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminDiscountDto>> GetById(int id)
        {
            var discount = await _db.DiscountRules.FindAsync(id)
                ?? throw new KeyNotFoundException("Discount not found");

            return Ok(ToDto(discount));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminDiscountDto>> Update(int id, AdminDiscountDto request)
        {
            var discount = await _db.DiscountRules.FindAsync(id)
                ?? throw new KeyNotFoundException("Discount not found");

            discount.Name = request.Name.Trim();
            discount.Percent = request.Percent;
            discount.EligibleClass = request.EligibleClass;
            discount.DocumentHint = request.DocumentHint;
            discount.Status = request.Status;

            await _db.SaveChangesAsync();
            return Ok(ToDto(discount));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Delete) is { } headerError)
                return headerError;

            var discount = await _db.DiscountRules.FindAsync(id)
                ?? throw new KeyNotFoundException("Discount not found");

            _db.DiscountRules.Remove(discount);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static AdminDiscountDto ToDto(DiscountRule discount) => new()
        {
            Id = discount.Id,
            Name = discount.Name,
            Percent = discount.Percent,
            EligibleClass = discount.EligibleClass,
            DocumentHint = discount.DocumentHint,
            Status = discount.Status
        };
    }
}

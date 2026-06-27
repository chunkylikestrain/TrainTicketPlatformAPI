using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Loyalty;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<LoyaltyAccountDto>> GetMine()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                return Ok(await _loyaltyService.GetAccountAsync(userId.Value));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("me/transactions")]
        public async Task<ActionResult<IEnumerable<LoyaltyTransactionDto>>> GetMyTransactions()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                return Ok(await _loyaltyService.GetTransactionsAsync(userId.Value));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private int? GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(subject, out var userId) ? userId : null;
        }
    }
}

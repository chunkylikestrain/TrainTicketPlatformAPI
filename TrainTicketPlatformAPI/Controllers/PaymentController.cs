using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        public PaymentsController(IPaymentService paymentService, IBookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        // GET: api/Payments
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAll()
            => Ok(await _paymentService.GetAllPaymentsAsync());

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetById(int id)
        {
            try
            {
                var p = await _paymentService.GetPaymentByIdAsync(id);
                var booking = await _bookingService.GetBookingByIdAsync(p.BookingId);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                return Ok(p);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Payments/booking/7
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetByBooking(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (!CanAccessUserResource(booking.UserId))
                return Forbid();

            var list = await _paymentService.GetPaymentsByBookingAsync(bookingId);
            return Ok(list);
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult<Payment>> Create(
            [FromBody] PaymentCreateDto dto)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(dto.BookingId);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                var payment = await _paymentService
                    .ProcessPaymentAsync(dto.BookingId, dto.Amount, dto.CardNumber);
                return CreatedAtAction(nameof(GetById),
                                       new { id = payment.Id },
                                       payment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found");
            }
        }

        private bool CanAccessUserResource(int userId)
        {
            if (User.IsInRole("Admin"))
                return true;

            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(subject, out var currentUserId) && currentUserId == userId;
        }
    }
    // Models/PaymentCreateDto.cs
    public class PaymentCreateDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string CardNumber { get; set; }
            = string.Empty;    
    }
}





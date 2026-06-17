using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Payments;
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
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments.Select(ToDto));
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetById(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                var booking = await _bookingService.GetBookingByIdAsync(payment.BookingId);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                return Ok(ToDto(payment));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Payments/booking/7
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByBooking(int bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                var payments = await _paymentService.GetPaymentsByBookingAsync(bookingId);
                return Ok(payments.Select(ToDto));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] ConfirmPaymentRequest request)
        {
            return await Confirm(request);
        }

        // POST: api/Payments/confirm
        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentDto>> Confirm([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                var payment = await _paymentService.ProcessPaymentAsync(
                    request.BookingId,
                    request.Amount,
                    request.CardNumber);

                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, ToDto(payment));
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Booking not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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

        private static PaymentDto ToDto(Payment payment) => new()
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status,
            Amount = payment.Amount
        };
    }
}

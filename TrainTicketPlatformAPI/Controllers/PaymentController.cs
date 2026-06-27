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
                if (payment.BookingOrderId.HasValue)
                {
                    var order = await _bookingService.GetBookingOrderByIdAsync(payment.BookingOrderId.Value);
                    if (!CanAccessOrder(order))
                        return Forbid();
                }
                else if (payment.BookingId.HasValue)
                {
                    var booking = await _bookingService.GetBookingByIdAsync(payment.BookingId.Value);
                    if (!CanAccessBooking(booking))
                        return Forbid();
                }

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
                if (!CanAccessBooking(booking))
                    return Forbid();

                var payments = await _paymentService.GetPaymentsByBookingAsync(bookingId);
                return Ok(payments.Select(ToDto));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/Payments/intent
        [AllowAnonymous]
        [HttpPost("intent")]
        public async Task<ActionResult<PaymentIntentDto>> CreateIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                if (request.BookingId.HasValue == request.BookingOrderId.HasValue)
                    return BadRequest("Provide either bookingId or bookingOrderId");

                PaymentIntentDto intent;
                if (request.BookingOrderId.HasValue)
                {
                    var order = await _bookingService.GetBookingOrderByIdAsync(request.BookingOrderId.Value);
                    if (!CanAccessOrder(order))
                        return Forbid();

                    intent = await _paymentService.CreatePaymentIntentForOrderAsync(
                        request.BookingOrderId.Value,
                        request.RedeemLoyaltyPoints);
                }
                else
                {
                    var booking = await _bookingService.GetBookingByIdAsync(request.BookingId!.Value);
                    if (!CanAccessBooking(booking))
                        return Forbid();

                    intent = await _paymentService.CreatePaymentIntentAsync(
                        request.BookingId.Value,
                        request.RedeemLoyaltyPoints);
                }

                return Ok(intent);
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

        // POST: api/Payments
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] ConfirmPaymentRequest request)
        {
            return await Confirm(request);
        }

        // POST: api/Payments/confirm
        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentDto>> Confirm([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                if (request.PaymentIntentId.StartsWith("pi_order_", StringComparison.OrdinalIgnoreCase))
                {
                    var order = await _bookingService.GetBookingOrderByIdAsync(GetOrderIdFromPaymentIntent(request.PaymentIntentId));
                    if (!CanAccessOrder(order))
                        return Forbid();
                }
                else
                {
                    var intentBookingId = GetBookingIdFromPaymentIntent(request.PaymentIntentId);
                    var booking = await _bookingService.GetBookingByIdAsync(intentBookingId);
                    if (!CanAccessBooking(booking))
                        return Forbid();
                }

                var payment = await _paymentService.ConfirmPaymentAsync(
                    request.PaymentIntentId,
                    request.PaymentMethodToken);

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

        private bool CanAccessUserResource(int? userId)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (!userId.HasValue)
                return false;

            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(subject, out var currentUserId) && currentUserId == userId.Value;
        }

        private bool CanAccessBooking(Booking booking)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (booking.UserId.HasValue)
                return CanAccessUserResource(booking.UserId);

            return true;
        }

        private bool CanAccessOrder(BookingOrder order)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (order.UserId.HasValue)
                return CanAccessUserResource(order.UserId);

            return true;
        }

        private static int GetBookingIdFromPaymentIntent(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId) ||
                !paymentIntentId.StartsWith("pi_", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(paymentIntentId[3..], out var bookingId))
            {
                throw new InvalidOperationException("Payment intent is invalid");
            }

            return bookingId;
        }

        private static int GetOrderIdFromPaymentIntent(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId) ||
                !paymentIntentId.StartsWith("pi_order_", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(paymentIntentId["pi_order_".Length..], out var bookingOrderId))
            {
                throw new InvalidOperationException("Payment intent is invalid");
            }

            return bookingOrderId;
        }

        private static PaymentDto ToDto(Payment payment) => new()
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            BookingOrderId = payment.BookingOrderId,
            BookingIds = payment.BookingOrder?.Bookings.Select(b => b.Id).Order().ToList() ?? Enumerable.Empty<int>(),
            PaymentIntentId = payment.PaymentIntentId,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status,
            Amount = payment.Amount,
            LoyaltyPointsRedeemed = payment.LoyaltyPointsRedeemed,
            LoyaltyDiscountAmount = payment.LoyaltyDiscountAmount
        };
    }
}

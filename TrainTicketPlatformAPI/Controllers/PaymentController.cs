using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
            => _paymentService = paymentService;

        // GET: api/Payments
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






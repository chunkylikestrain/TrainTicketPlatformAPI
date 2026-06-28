using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Invoices;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IBookingService _bookingService;

        public InvoicesController(
            IInvoiceService invoiceService,
            IBookingService bookingService)
        {
            _invoiceService = invoiceService;
            _bookingService = bookingService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetMine()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Forbid();

            var invoices = await _invoiceService.GetInvoicesForUserAsync(currentUserId.Value);
            return Ok(invoices.Select(ToDto));
        }

        [HttpPost("bookings/{bookingId}")]
        public async Task<ActionResult<InvoiceDto>> GenerateForBooking(
            int bookingId,
            [FromBody] CreateInvoiceRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (!CanAccessBooking(booking))
                    return Forbid();

                var invoice = await _invoiceService.GenerateForBookingAsync(bookingId, request);
                return Ok(ToDto(invoice));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("orders/{bookingOrderId}")]
        public async Task<ActionResult<InvoiceDto>> GenerateForOrder(
            int bookingOrderId,
            [FromBody] CreateInvoiceRequest request)
        {
            try
            {
                var order = await _bookingService.GetBookingOrderByIdAsync(bookingOrderId);
                if (!CanAccessOrder(order))
                    return Forbid();

                var invoice = await _invoiceService.GenerateForOrderAsync(bookingOrderId, request);
                return Ok(ToDto(invoice));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (!CanAccessInvoice(invoice))
                    return Forbid();

                var pdf = await _invoiceService.GetInvoicePdfAsync(id);
                return File(pdf, "application/pdf", $"invoice-{invoice.InvoiceNumber.Replace("/", "-")}.pdf");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private bool CanAccessInvoice(Invoice invoice)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && invoice.UserId == currentUserId.Value;
        }

        private bool CanAccessBooking(Booking booking)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && booking.UserId == currentUserId.Value;
        }

        private bool CanAccessOrder(BookingOrder order)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && order.UserId == currentUserId.Value;
        }

        private int? GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(subject, out var currentUserId)
                ? currentUserId
                : null;
        }

        private static InvoiceDto ToDto(Invoice invoice) => new()
        {
            Id = invoice.Id,
            BookingId = invoice.BookingId,
            BookingOrderId = invoice.BookingOrderId,
            InvoiceNumber = invoice.InvoiceNumber,
            BuyerName = invoice.BuyerName,
            BuyerEmail = invoice.BuyerEmail,
            BuyerTaxId = invoice.BuyerTaxId,
            BillingAddress = invoice.BillingAddress,
            NetAmount = invoice.NetAmount,
            VatAmount = invoice.VatAmount,
            TotalAmount = invoice.TotalAmount,
            Currency = invoice.Currency,
            Status = invoice.Status,
            IssuedAtUtc = invoice.IssuedAtUtc,
            PdfUrl = $"/api/Invoices/{invoice.Id}/pdf"
        };
    }
}

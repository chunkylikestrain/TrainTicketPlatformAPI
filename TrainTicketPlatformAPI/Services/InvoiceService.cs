using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Invoices;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class InvoiceService : IInvoiceService
    {
        private const decimal VatRate = 0.08m;
        private readonly TrainTicketDbContext _db;

        public InvoiceService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<Invoice> GenerateForBookingAsync(int bookingId, CreateInvoiceRequest request)
        {
            var booking = await LoadBookingAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found");

            EnsureInvoiceCanBeIssued(booking);

            var existing = await _db.Invoices
                .Include(i => i.Booking)
                .Include(i => i.BookingOrder)
                .FirstOrDefaultAsync(i => i.BookingId == booking.Id);

            if (existing != null)
                return existing;

            var invoice = BuildInvoice(
                request,
                booking.UserId,
                booking.Id,
                null,
                booking.Amount,
                booking.Currency,
                booking.PassengerName ?? booking.User?.DisplayName ?? booking.User?.Email ?? booking.GuestEmail ?? "Passenger",
                booking.User?.Email ?? booking.GuestEmail ?? string.Empty);

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            return await GetInvoiceByIdAsync(invoice.Id);
        }

        public async Task<Invoice> GenerateForOrderAsync(int bookingOrderId, CreateInvoiceRequest request)
        {
            var order = await LoadOrderAsync(bookingOrderId)
                ?? throw new KeyNotFoundException("Booking order not found");

            if (order.BookingStatus != "Confirmed" || order.PaymentStatus != "Successful")
                throw new InvalidOperationException("Invoices are available only for paid confirmed orders");

            var existing = await _db.Invoices
                .Include(i => i.Booking)
                .Include(i => i.BookingOrder)
                .FirstOrDefaultAsync(i => i.BookingOrderId == order.Id);

            if (existing != null)
                return existing;

            var total = order.Bookings.Sum(booking => booking.Amount);
            var currency = order.Bookings.Select(booking => booking.Currency).FirstOrDefault(currency => !string.IsNullOrWhiteSpace(currency)) ?? "PLN";
            var fallbackName = order.Bookings.Select(booking => booking.PassengerName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                ?? order.User?.DisplayName
                ?? order.User?.Email
                ?? order.GuestEmail
                ?? "Passenger";
            var fallbackEmail = order.User?.Email ?? order.GuestEmail ?? string.Empty;
            var invoice = BuildInvoice(request, order.UserId, null, order.Id, total, currency, fallbackName, fallbackEmail);

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            return await GetInvoiceByIdAsync(invoice.Id);
        }

        public async Task<IReadOnlyList<Invoice>> GetInvoicesForUserAsync(int userId)
        {
            return await _db.Invoices
                .Include(i => i.Booking)
                .Include(i => i.BookingOrder)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.IssuedAtUtc)
                .ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _db.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Train)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.SegmentDepartureStation)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.SegmentArrivalStation)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Trip)
                        .ThenInclude(t => t!.TrainRoute)
                            .ThenInclude(r => r.DepartureStation)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Trip)
                        .ThenInclude(t => t!.TrainRoute)
                            .ThenInclude(r => r.ArrivalStation)
                .Include(i => i.BookingOrder)
                    .ThenInclude(o => o!.Bookings)
                        .ThenInclude(b => b.Train)
                .Include(i => i.BookingOrder)
                    .ThenInclude(o => o!.Bookings)
                        .ThenInclude(b => b.SegmentDepartureStation)
                .Include(i => i.BookingOrder)
                    .ThenInclude(o => o!.Bookings)
                        .ThenInclude(b => b.SegmentArrivalStation)
                .Include(i => i.BookingOrder)
                    .ThenInclude(o => o!.Bookings)
                        .ThenInclude(b => b.Trip)
                            .ThenInclude(t => t!.TrainRoute)
                                .ThenInclude(r => r.DepartureStation)
                .Include(i => i.BookingOrder)
                    .ThenInclude(o => o!.Bookings)
                        .ThenInclude(b => b.Trip)
                            .ThenInclude(t => t!.TrainRoute)
                                .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(i => i.Id == invoiceId)
                ?? throw new KeyNotFoundException("Invoice not found");
        }

        public async Task<byte[]> GetInvoicePdfAsync(int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            return SimpleInvoicePdfBuilder.Build(invoice);
        }

        private async Task<Booking?> LoadBookingAsync(int bookingId)
        {
            return await _db.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        private async Task<BookingOrder?> LoadOrderAsync(int bookingOrderId)
        {
            return await _db.BookingOrders
                .Include(o => o.User)
                .Include(o => o.Bookings)
                .FirstOrDefaultAsync(o => o.Id == bookingOrderId);
        }

        private static void EnsureInvoiceCanBeIssued(Booking booking)
        {
            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                throw new InvalidOperationException("Invoices are available only for paid confirmed bookings");

            if (booking.IsCancelled)
                throw new InvalidOperationException("Invoices are not available for cancelled bookings");
        }

        private static Invoice BuildInvoice(
            CreateInvoiceRequest request,
            int? userId,
            int? bookingId,
            int? bookingOrderId,
            decimal total,
            string currency,
            string fallbackName,
            string fallbackEmail)
        {
            total = Math.Max(0m, total);
            var net = Math.Round(total / (1 + VatRate), 2, MidpointRounding.AwayFromZero);
            var vat = total - net;

            return new Invoice
            {
                UserId = userId,
                BookingId = bookingId,
                BookingOrderId = bookingOrderId,
                InvoiceNumber = GenerateInvoiceNumber(),
                BuyerName = Normalize(request.BuyerName, fallbackName),
                BuyerEmail = Normalize(request.BuyerEmail, fallbackEmail),
                BuyerTaxId = request.BuyerTaxId?.Trim() ?? string.Empty,
                BillingAddress = request.BillingAddress?.Trim() ?? string.Empty,
                NetAmount = net,
                VatAmount = vat,
                TotalAmount = total,
                Currency = string.IsNullOrWhiteSpace(currency) ? "PLN" : currency.Trim(),
                Status = "Issued",
                IssuedAtUtc = DateTime.UtcNow
            };
        }

        private static string Normalize(string? value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback.Trim() : value.Trim();

        private static string GenerateInvoiceNumber()
            => $"FV/{DateTime.UtcNow:yyyyMMdd}/{Random.Shared.Next(1000, 9999)}";
    }
}

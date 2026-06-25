using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TrainTicketPlatformAPI.Contracts.Tickets;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TicketArtifactService : ITicketArtifactService
    {
        private readonly TrainTicketDbContext _db;
        private readonly IConfiguration _configuration;

        public TicketArtifactService(TrainTicketDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<Booking> EnsureTicketIssuedAsync(int bookingId)
        {
            var booking = await LoadBookingAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found");

            EnsureConfirmedTicket(booking);

            var changed = false;
            if (string.IsNullOrWhiteSpace(booking.TicketNumber))
            {
                booking.TicketNumber = GenerateTicketNumber();
                changed = true;
            }

            if (!booking.TicketIssuedAtUtc.HasValue)
            {
                booking.TicketIssuedAtUtc = DateTime.UtcNow;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(booking.TicketQrPayload) ||
                !booking.TicketQrPayload.Contains("|sig=", StringComparison.OrdinalIgnoreCase))
            {
                booking.TicketQrPayload = BuildQrPayload(booking);
                changed = true;
            }

            if (changed)
                await _db.SaveChangesAsync();

            return booking;
        }

        public async Task<TicketArtifactDto> GetTicketAsync(int bookingId)
        {
            var booking = await EnsureTicketIssuedAsync(bookingId);
            return ToTicketDto(booking);
        }

        public async Task<string> GetQrSvgAsync(int bookingId)
        {
            var booking = await EnsureTicketIssuedAsync(bookingId);

            using var qrData = QRCodeGenerator.GenerateQrCode(
                booking.TicketQrPayload,
                QRCodeGenerator.ECCLevel.Q);
            var qrCode = new SvgQRCode(qrData);
            return qrCode.GetGraphic(6);
        }

        public async Task<byte[]> GetTicketPdfAsync(int bookingId)
        {
            var booking = await EnsureTicketIssuedAsync(bookingId);

            using var qrData = QRCodeGenerator.GenerateQrCode(
                booking.TicketQrPayload,
                QRCodeGenerator.ECCLevel.Q);

            return SimpleTicketPdfBuilder.Build(ToTicketDto(booking), qrData.ModuleMatrix);
        }

        public async Task<byte[]> GetOrderTicketPdfAsync(int bookingOrderId)
        {
            var order = await _db.BookingOrders
                .Include(o => o.Bookings)
                .FirstOrDefaultAsync(o => o.Id == bookingOrderId)
                ?? throw new KeyNotFoundException("Booking order not found");

            if (order.Bookings.Count == 0)
                throw new InvalidOperationException("Booking order has no tickets");

            var pages = new List<(TicketArtifactDto Ticket, IReadOnlyList<System.Collections.BitArray> QrMatrix)>();
            foreach (var booking in order.Bookings.OrderBy(booking => booking.Id))
            {
                var issued = await EnsureTicketIssuedAsync(booking.Id);
                using var qrData = QRCodeGenerator.GenerateQrCode(
                    issued.TicketQrPayload,
                    QRCodeGenerator.ECCLevel.Q);
                pages.Add((ToTicketDto(issued), qrData.ModuleMatrix));
            }

            return SimpleTicketPdfBuilder.Build(pages);
        }

        public async Task<TicketEmailDeliveryDto> SendTicketEmailAsync(int bookingId, string? recipientEmail)
        {
            var booking = await EnsureTicketIssuedAsync(bookingId);
            var recipient = NormalizeEmail(recipientEmail)
                ?? NormalizeEmail(booking.GuestEmail)
                ?? NormalizeEmail(booking.User?.Email)
                ?? throw new InvalidOperationException("A ticket email recipient is required");

            var now = DateTime.UtcNow;
            var delivery = new TicketEmailDelivery
            {
                BookingId = booking.Id,
                RecipientEmail = recipient,
                Status = "Sent",
                RequestedAtUtc = now,
                SentAtUtc = now,
                ProviderMessageId = $"demo_email_{booking.Id}_{now:yyyyMMddHHmmss}"
            };

            booking.TicketEmailStatus = delivery.Status;
            booking.TicketEmailSentAtUtc = delivery.SentAtUtc;
            booking.TicketEmailRecipient = recipient;

            _db.TicketEmailDeliveries.Add(delivery);
            await _db.SaveChangesAsync();

            return ToDeliveryDto(delivery);
        }

        private async Task<Booking?> LoadBookingAsync(int bookingId)
        {
            return await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Train)
                .Include(b => b.Seat)
                .Include(b => b.SegmentDepartureStation)
                .Include(b => b.SegmentArrivalStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.DepartureStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        private static void EnsureConfirmedTicket(Booking booking)
        {
            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                throw new InvalidOperationException("Ticket artifacts are available only for paid confirmed bookings");

            if (booking.IsCancelled)
                throw new InvalidOperationException("Ticket artifacts are not available for cancelled bookings");
        }

        private string BuildQrPayload(Booking booking)
        {
            var issuedAt = booking.TicketIssuedAtUtc ?? DateTime.UtcNow;
            var basePayload = string.Join("|",
                "railway-ticket-v1",
                $"ticket={booking.TicketNumber}",
                $"booking={booking.BookingReference}",
                $"trip={booking.TripId?.ToString(CultureInfo.InvariantCulture) ?? "legacy"}",
                $"seat={booking.Seat.Coach}-{booking.Seat.Number}",
                $"segment={booking.SegmentDepartureOrder?.ToString(CultureInfo.InvariantCulture) ?? "origin"}-{booking.SegmentArrivalOrder?.ToString(CultureInfo.InvariantCulture) ?? "destination"}",
                $"date={(booking.SegmentDepartureTime ?? booking.TravelDate):yyyy-MM-dd}",
                $"issued={issuedAt:O}");

            return $"{basePayload}|sig={BuildSignature(basePayload)}";
        }

        private string BuildSignature(string payload)
        {
            var key = _configuration["Ticketing:SigningKey"];
            if (string.IsNullOrWhiteSpace(key))
                key = _configuration["Jwt:Key"] ?? "development-ticket-signing-key";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash)[..24].ToLowerInvariant();
        }

        private static TicketArtifactDto ToTicketDto(Booking booking)
        {
            return new TicketArtifactDto
            {
                BookingId = booking.Id,
                BookingReference = booking.BookingReference,
                TicketNumber = booking.TicketNumber,
                PassengerName = booking.PassengerName ?? booking.User?.Email ?? booking.GuestEmail ?? "Passenger",
                RecipientEmail = booking.GuestEmail ?? booking.User?.Email ?? string.Empty,
                TrainName = string.IsNullOrWhiteSpace(booking.Train.Code) ? booking.Train.Name : booking.Train.Code,
                Route = GetRouteLabel(booking),
                SeatLabel = $"Coach {booking.Seat.Coach}, seat {booking.Seat.Number}",
                TravelDate = booking.TravelDate,
                DepartureTime = booking.SegmentDepartureTime ?? booking.Trip?.DepartureTime ?? booking.Train.DepartureTime,
                ArrivalTime = booking.SegmentArrivalTime ?? booking.Trip?.ArrivalTime ?? booking.Train.ArrivalTime,
                IssuedAtUtc = booking.TicketIssuedAtUtc ?? DateTime.UtcNow,
                QrPayload = booking.TicketQrPayload,
                QrSvgUrl = $"/api/Bookings/{booking.Id}/ticket/qr",
                PdfUrl = $"/api/Bookings/{booking.Id}/ticket/pdf",
                EmailDeliveryStatus = booking.TicketEmailStatus,
                EmailSentAtUtc = booking.TicketEmailSentAtUtc
            };
        }

        private static string GetRouteLabel(Booking booking)
        {
            if (booking.Trip?.TrainRoute != null)
            {
                var departure = booking.SegmentDepartureStation?.Name ?? booking.Trip.TrainRoute.DepartureStation.Name;
                var arrival = booking.SegmentArrivalStation?.Name ?? booking.Trip.TrainRoute.ArrivalStation.Name;
                return $"{departure} -> {arrival}";
            }

            return $"{booking.Train.DepartureStation} -> {booking.Train.ArrivalStation}";
        }

        private static TicketEmailDeliveryDto ToDeliveryDto(TicketEmailDelivery delivery) => new()
        {
            Id = delivery.Id,
            BookingId = delivery.BookingId,
            RecipientEmail = delivery.RecipientEmail,
            Status = delivery.Status,
            RequestedAtUtc = delivery.RequestedAtUtc,
            SentAtUtc = delivery.SentAtUtc,
            ProviderMessageId = delivery.ProviderMessageId,
            ErrorMessage = delivery.ErrorMessage
        };

        private static string GenerateTicketNumber()
            => $"WH{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(1000, 9999)}";

        private static string? NormalizeEmail(string? email)
            => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}

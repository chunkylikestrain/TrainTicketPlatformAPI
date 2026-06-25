using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Contracts.Tickets;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ITicketArtifactService _ticketArtifactService;

        public BookingsController(
            IBookingService bookingService,
            ITicketArtifactService ticketArtifactService)
        {
            _bookingService = bookingService;
            _ticketArtifactService = ticketArtifactService;
        }

        // GET: api/Bookings
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings.Select(ToDto));
        }

        // GET: api/Bookings/me
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetMine([FromQuery] string section = "tickets")
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Forbid();

            var bookings = await _bookingService.GetBookingsByUserAsync(currentUserId.Value);
            return Ok(FilterTicketSection(bookings, section).Select(ToDto));
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            try
            {
                var b = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessBooking(b))
                    return Forbid();

                return Ok(ToDto(b));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Bookings/user/42
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetByUser(int userId)
        {
            if (!CanAccessUserResource(userId))
                return Forbid();

            var list = await _bookingService.GetBookingsByUserAsync(userId);
            return Ok(list.Select(ToDto));
        }

        // GET: api/Bookings/availability?trainId=1&seatId=5&travelDate=2025-06-20
        [AllowAnonymous]
        [HttpGet("availability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromQuery] int trainId,
            [FromQuery] int seatId,
            [FromQuery] DateTime travelDate)
        {
            var available = await _bookingService
                .CheckSeatAvailabilityAsync(trainId, seatId, travelDate);
            return Ok(available);
        }

        // POST: api/Bookings
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var booking = new Booking
                {
                    UserId = currentUserId,
                    TrainId = request.TrainId,
                    TripId = request.TripId,
                    SeatId = request.SeatId,
                    SegmentDepartureStationId = request.SegmentDepartureStationId,
                    SegmentArrivalStationId = request.SegmentArrivalStationId,
                    TravelDate = request.TravelDate,
                    GuestEmail = request.GuestEmail,
                    PassengerName = request.PassengerName,
                    PassengerType = request.PassengerType ?? "Adult",
                    DiscountCode = request.DiscountCode ?? "normal",
                    BookingStatus = "PendingPayment",
                    PaymentStatus = "Pending"
                };

                var created = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetById),
                                       new { id = created.Id },
                                       ToDto(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Bookings/5/guest-data
        [AllowAnonymous]
        [HttpPut("{id}/guest-data")]
        public async Task<ActionResult<BookingDto>> UpdateGuestData(
            int id,
            [FromBody] UpdateGuestBookingDataRequest request)
        {
            try
            {
                var booking = await _bookingService.UpdateGuestBookingDataAsync(
                    id,
                    request.GuestEmail,
                    request.PassengerName,
                    request.AcceptedTerms);

                return Ok(ToDto(booking));
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

        // GET: api/Bookings/guest?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("guest")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetGuestTickets([FromQuery] string email)
        {
            try
            {
                var bookings = await _bookingService.GetGuestTicketsByEmailAsync(email);
                return Ok(bookings.Select(ToDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Bookings/tickets/WH123/refund
        [AllowAnonymous]
        [HttpPost("tickets/{ticketNumber}/refund")]
        public async Task<ActionResult<BookingDto>> RefundTicket(
            string ticketNumber,
            [FromBody] RefundTicketRequest request)
        {
            try
            {
                var booking = await _bookingService.RefundTicketAsync(ticketNumber, request.Email);
                return Ok(ToDto(booking));
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

        // POST: api/Bookings/orders
        [AllowAnonymous]
        [HttpPost("orders")]
        public async Task<ActionResult<BookingOrderDto>> CreateOrder([FromBody] CreateBookingOrderRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var order = new BookingOrder
                {
                    UserId = currentUserId,
                    GuestEmail = request.GuestEmail,
                    BookingStatus = "PendingPayment",
                    PaymentStatus = "Pending"
                };

                var bookings = request.Passengers.Select(passenger => new Booking
                {
                    UserId = currentUserId,
                    TrainId = request.TrainId,
                    TripId = request.TripId,
                    SeatId = passenger.SeatId,
                    SegmentDepartureStationId = request.SegmentDepartureStationId,
                    SegmentArrivalStationId = request.SegmentArrivalStationId,
                    TravelDate = request.TravelDate,
                    GuestEmail = request.GuestEmail,
                    PassengerName = passenger.PassengerName,
                    PassengerType = passenger.PassengerType ?? "Adult",
                    DiscountCode = passenger.DiscountCode ?? "normal",
                    BookingStatus = "PendingPayment",
                    PaymentStatus = "Pending"
                });

                var created = await _bookingService.CreateBookingOrderAsync(order, bookings);
                return CreatedAtAction(nameof(GetOrderById),
                    new { id = created.Id },
                    ToOrderDto(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Bookings/orders/5
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<BookingOrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _bookingService.GetBookingOrderByIdAsync(id);
                if (!CanAccessOrder(order))
                    return Forbid();

                return Ok(ToOrderDto(order));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Bookings/orders/5/tickets?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("orders/{id}/tickets")]
        public async Task<ActionResult<BookingOrderTicketsDto>> GetOrderTickets(
            int id,
            [FromQuery] string? email = null)
        {
            try
            {
                var order = await _bookingService.GetBookingOrderByIdAsync(id);
                if (!CanAccessOrderTickets(order, email))
                    return Forbid();

                var tickets = new List<TicketArtifactDto>();
                foreach (var booking in order.Bookings.OrderBy(booking => booking.Id))
                    tickets.Add(await _ticketArtifactService.GetTicketAsync(booking.Id));

                return Ok(new BookingOrderTicketsDto
                {
                    BookingOrderId = order.Id,
                    OrderReference = order.OrderReference,
                    TicketCount = tickets.Count,
                    Tickets = tickets
                });
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

        // POST: api/Bookings/orders/5/tickets/email
        [AllowAnonymous]
        [HttpPost("orders/{id}/tickets/email")]
        public async Task<ActionResult<BookingOrderEmailDeliveryDto>> SendOrderTicketsEmail(
            int id,
            [FromBody] SendTicketEmailRequest request)
        {
            try
            {
                var order = await _bookingService.GetBookingOrderByIdAsync(id);
                if (!CanAccessOrderTickets(order, request.Email))
                    return Forbid();

                var deliveries = new List<TicketEmailDeliveryDto>();
                foreach (var booking in order.Bookings.OrderBy(booking => booking.Id))
                    deliveries.Add(await _ticketArtifactService.SendTicketEmailAsync(booking.Id, request.Email));

                return Ok(new BookingOrderEmailDeliveryDto
                {
                    BookingOrderId = order.Id,
                    OrderReference = order.OrderReference,
                    RequestedCount = order.Bookings.Count,
                    SentCount = deliveries.Count(delivery => delivery.Status == "Sent"),
                    Deliveries = deliveries
                });
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

        // GET: api/Bookings/orders/5/tickets/pdf?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("orders/{id}/tickets/pdf")]
        public async Task<IActionResult> GetOrderTicketsPdf(
            int id,
            [FromQuery] string? email = null)
        {
            try
            {
                var order = await _bookingService.GetBookingOrderByIdAsync(id);
                if (!CanAccessOrderTickets(order, email))
                    return Forbid();

                var pdf = await _ticketArtifactService.GetOrderTicketPdfAsync(id);
                var fileReference = string.IsNullOrWhiteSpace(order.OrderReference)
                    ? id.ToString()
                    : order.OrderReference;
                return File(pdf, "application/pdf", $"tickets-{fileReference}.pdf");
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

        // POST: api/Bookings/5/refund
        [HttpPost("{id}/refund")]
        public async Task<ActionResult<BookingDto>> RefundMyTicket(
            int id,
            [FromBody] ReturnTicketRequest? request = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Forbid();

                var booking = await _bookingService.RefundUserBookingAsync(
                    id,
                    currentUserId.Value,
                    request?.Reason);

                return Ok(ToDto(booking));
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

        // POST: api/Bookings/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            return await Cancel(id);
        }

        // POST: api/Bookings/5/confirm
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<BookingDto>> Confirm(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessBooking(booking))
                    return Forbid();

                var confirmed = await _bookingService.ConfirmBookingAsync(id);
                return Ok(ToDto(confirmed));
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

        // GET: api/Bookings/5/ticket?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("{id}/ticket")]
        public async Task<ActionResult<TicketArtifactDto>> GetTicket(
            int id,
            [FromQuery] string? email = null)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessTicket(booking, email))
                    return Forbid();

                return Ok(await _ticketArtifactService.GetTicketAsync(id));
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

        // GET: api/Bookings/5/ticket/qr?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("{id}/ticket/qr")]
        public async Task<IActionResult> GetTicketQr(
            int id,
            [FromQuery] string? email = null)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessTicket(booking, email))
                    return Forbid();

                var svg = await _ticketArtifactService.GetQrSvgAsync(id);
                return Content(svg, "image/svg+xml", System.Text.Encoding.UTF8);
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

        // GET: api/Bookings/5/ticket/pdf?email=guest@example.com
        [AllowAnonymous]
        [HttpGet("{id}/ticket/pdf")]
        public async Task<IActionResult> GetTicketPdf(
            int id,
            [FromQuery] string? email = null)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessTicket(booking, email))
                    return Forbid();

                var pdf = await _ticketArtifactService.GetTicketPdfAsync(id);
                var ticketNumber = string.IsNullOrWhiteSpace(booking.TicketNumber)
                    ? id.ToString()
                    : booking.TicketNumber;
                return File(pdf, "application/pdf", $"ticket-{ticketNumber}.pdf");
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

        // POST: api/Bookings/5/ticket/email
        [AllowAnonymous]
        [HttpPost("{id}/ticket/email")]
        public async Task<ActionResult<TicketEmailDeliveryDto>> SendTicketEmail(
            int id,
            [FromBody] SendTicketEmailRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessTicket(booking, request.Email))
                    return Forbid();

                return Ok(await _ticketArtifactService.SendTicketEmailAsync(id, request.Email));
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

        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Booking booking)
        {
            if (id != booking.Id)
                return BadRequest("ID mismatch");

            try
            {
                var existing = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessBooking(existing))
                    return Forbid();

                var updated = await _bookingService.UpdateBookingAsync(booking);
                return Ok(updated);
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

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessBooking(booking))
                    return Forbid();

                await _bookingService.CancelBookingAsync(id);
                return NoContent();
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
        // GET: api/Bookings/report?from=2025-06-01&to=2025-06-30
        [Authorize(Roles = "Admin")]
        [HttpGet("report")]
        public async Task<ActionResult<BookingReport>> GetReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (to < from)
                return BadRequest("'to' must be on or after 'from'.");

            var report = await _bookingService.GenerateBookingReportAsync(from, to);
            return Ok(report);
        }

        private bool CanAccessUserResource(int? userId)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (!userId.HasValue)
                return false;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && currentUserId.Value == userId.Value;
        }

        private bool CanAccessBooking(Booking booking)
        {
            if (User.IsInRole("Admin"))
                return true;

            return booking.UserId.HasValue && CanAccessUserResource(booking.UserId);
        }

        private bool CanAccessTicket(Booking booking, string? guestEmail)
        {
            if (CanAccessBooking(booking))
                return true;

            return !string.IsNullOrWhiteSpace(booking.GuestEmail) &&
                !string.IsNullOrWhiteSpace(guestEmail) &&
                string.Equals(
                    booking.GuestEmail.Trim(),
                    guestEmail.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private int? GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(subject, out var currentUserId)
                ? currentUserId
                : null;
        }

        private static IEnumerable<Booking> FilterTicketSection(IEnumerable<Booking> bookings, string section)
        {
            var normalizedSection = section.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;

            return normalizedSection switch
            {
                "history" or "travel-history" => bookings.Where(booking =>
                    IsPaidConfirmed(booking) &&
                    !IsReturned(booking) &&
                    GetEffectiveArrivalTime(booking) < now),

                "returned" => bookings.Where(IsReturned),

                "season" or "season-tickets" => Enumerable.Empty<Booking>(),

                _ => bookings.Where(booking =>
                    IsPaidConfirmed(booking) &&
                    !IsReturned(booking) &&
                    GetEffectiveArrivalTime(booking) >= now)
                    .OrderBy(GetEffectiveDepartureTime)
                    .ThenBy(GetEffectiveArrivalTime)
                    .ThenBy(booking => booking.BookingDate)
            };
        }

        private static bool IsPaidConfirmed(Booking booking)
            => booking.BookingStatus == "Confirmed" && booking.PaymentStatus == "Successful";

        private static bool IsReturned(Booking booking)
            => booking.BookingStatus == "Refunded" || booking.PaymentStatus == "Refunded";

        private static DateTime GetEffectiveArrivalTime(Booking booking)
            => booking.SegmentArrivalTime
               ?? booking.Trip?.ArrivalTime
               ?? booking.Train.ArrivalTime;

        private static DateTime GetEffectiveDepartureTime(Booking booking)
            => booking.SegmentDepartureTime
               ?? booking.Trip?.DepartureTime
               ?? booking.Train.DepartureTime;

        private static BookingDto ToDto(Booking booking) => new()
        {
            Id = booking.Id,
            UserId = booking.UserId,
            TrainId = booking.TrainId,
            TripId = booking.TripId,
            BookingOrderId = booking.BookingOrderId,
            SeatId = booking.SeatId,
            SegmentDepartureStationId = booking.SegmentDepartureStationId,
            SegmentArrivalStationId = booking.SegmentArrivalStationId,
            SegmentDepartureOrder = booking.SegmentDepartureOrder,
            SegmentArrivalOrder = booking.SegmentArrivalOrder,
            SegmentDepartureTime = booking.SegmentDepartureTime,
            SegmentArrivalTime = booking.SegmentArrivalTime,
            BookingReference = booking.BookingReference,
            TicketNumber = booking.TicketNumber,
            GuestEmail = booking.GuestEmail,
            PassengerName = booking.PassengerName,
            PassengerType = booking.PassengerType,
            DiscountCode = booking.DiscountCode,
            DiscountName = booking.DiscountName,
            DiscountPercent = booking.DiscountPercent,
            BaseAmount = booking.BaseAmount,
            BookingDate = booking.BookingDate,
            TravelDate = booking.TravelDate,
            ExpiresAtUtc = booking.ExpiresAtUtc,
            BookingStatus = booking.BookingStatus,
            PaymentStatus = booking.PaymentStatus,
            IsCancelled = booking.IsCancelled,
            CancellationDate = booking.CancellationDate,
            CancellationReason = booking.CancellationReason,
            ConfirmedAtUtc = booking.ConfirmedAtUtc,
            RefundedAtUtc = booking.RefundedAtUtc,
            TicketIssuedAtUtc = booking.TicketIssuedAtUtc,
            HasTicketArtifact = !string.IsNullOrWhiteSpace(booking.TicketQrPayload),
            TicketEmailStatus = booking.TicketEmailStatus,
            TicketEmailSentAtUtc = booking.TicketEmailSentAtUtc,
            TicketEmailRecipient = booking.TicketEmailRecipient,
            TrainName = booking.Train == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(booking.Train.Code) ? booking.Train.Name : booking.Train.Code,
            Route = GetRouteLabel(booking),
            SeatLabel = booking.Seat == null ? string.Empty : $"Coach {booking.Seat.Coach}, seat {booking.Seat.Number}",
            DepartureTime = booking.SegmentDepartureTime ?? booking.Trip?.DepartureTime ?? booking.Train?.DepartureTime,
            ArrivalTime = booking.SegmentArrivalTime ?? booking.Trip?.ArrivalTime ?? booking.Train?.ArrivalTime,
            Platform = booking.Trip?.Platform ?? string.Empty,
            Track = booking.Trip?.Track ?? string.Empty,
            DelayMinutes = booking.Trip?.DelayMinutes ?? 0,
            TripCancellationReason = booking.Trip?.CancellationReason ?? string.Empty,
            OriginalPlatform = booking.Trip?.OriginalPlatform ?? string.Empty,
            OriginalTrack = booking.Trip?.OriginalTrack ?? string.Empty,
            DisruptionMessage = GetDisruptionMessage(booking.Trip),
            DisruptionSeverity = GetDisruptionSeverity(booking.Trip),
            HasPlatformChange = HasPlatformChange(booking.Trip),
            HasDisruption = HasDisruption(booking.Trip),
            Amount = booking.Amount > 0m ? booking.Amount : booking.Trip?.Fares
                .OrderByDescending(f => booking.Seat != null && f.ClassType == booking.Seat.ClassType)
                .ThenBy(f => f.Price)
                .FirstOrDefault()?.Price ?? 0m,
            Currency = !string.IsNullOrWhiteSpace(booking.Currency)
                ? booking.Currency
                : booking.Trip?.Fares.FirstOrDefault()?.Currency ?? "PLN"
        };

        private static string GetRouteLabel(Booking booking)
        {
            if (booking.SegmentDepartureStation != null && booking.SegmentArrivalStation != null)
                return $"{booking.SegmentDepartureStation.Name} -> {booking.SegmentArrivalStation.Name}";

            return booking.Trip?.TrainRoute == null || booking.Train == null
                ? booking.Train == null ? string.Empty : $"{booking.Train.DepartureStation} -> {booking.Train.ArrivalStation}"
                : $"{booking.Trip.TrainRoute.DepartureStation.Name} -> {booking.Trip.TrainRoute.ArrivalStation.Name}";
        }

        private static BookingOrderDto ToOrderDto(BookingOrder order) => new()
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderReference = order.OrderReference,
            GuestEmail = order.GuestEmail,
            CreatedAtUtc = order.CreatedAtUtc,
            ExpiresAtUtc = order.ExpiresAtUtc,
            BookingStatus = order.BookingStatus,
            PaymentStatus = order.PaymentStatus,
            ConfirmedAtUtc = order.ConfirmedAtUtc,
            TicketCount = order.Bookings.Count,
            HasTicketArtifacts = order.Bookings.Count > 0 &&
                order.Bookings.All(booking => !string.IsNullOrWhiteSpace(booking.TicketQrPayload)),
            Bookings = order.Bookings
                .OrderBy(booking => booking.Id)
                .Select(ToDto)
                .ToList(),
            Amount = order.Bookings.Sum(booking => booking.Amount > 0m ? booking.Amount : booking.Trip?.Fares
                .OrderByDescending(f => booking.Seat != null && f.ClassType == booking.Seat.ClassType)
                .ThenBy(f => f.Price)
                .FirstOrDefault()?.Price ?? 0m)
        };

        private bool CanAccessOrder(BookingOrder order)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (order.UserId.HasValue)
                return CanAccessUserResource(order.UserId);

            return true;
        }

        private bool CanAccessOrderTickets(BookingOrder order, string? email)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (order.UserId.HasValue)
                return CanAccessUserResource(order.UserId);

            return !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(order.GuestEmail) &&
                string.Equals(
                    order.GuestEmail.Trim(),
                    email.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasPlatformChange(Trip? trip)
        {
            if (trip == null)
                return false;

            var originalPlatform = string.IsNullOrWhiteSpace(trip.OriginalPlatform)
                ? trip.Platform
                : trip.OriginalPlatform;
            var originalTrack = string.IsNullOrWhiteSpace(trip.OriginalTrack)
                ? trip.Track
                : trip.OriginalTrack;

            return !string.Equals(originalPlatform, trip.Platform, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(originalTrack, trip.Track, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasDisruption(Trip? trip)
        {
            return trip != null &&
                (trip.DelayMinutes > 0 ||
                 HasPlatformChange(trip) ||
                 !string.Equals(trip.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) ||
                 !string.IsNullOrWhiteSpace(trip.CancellationReason) ||
                 !string.IsNullOrWhiteSpace(trip.DisruptionMessage));
        }

        private static string GetDisruptionSeverity(Trip? trip)
        {
            if (trip == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(trip.DisruptionSeverity))
                return trip.DisruptionSeverity;

            if (string.Equals(trip.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                return "Critical";

            if (trip.DelayMinutes >= 30)
                return "Major";

            return HasDisruption(trip) ? "Notice" : string.Empty;
        }

        private static string GetDisruptionMessage(Trip? trip)
        {
            if (trip == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(trip.DisruptionMessage))
                return trip.DisruptionMessage;

            if (string.Equals(trip.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(trip.CancellationReason)
                    ? "This train has been cancelled."
                    : $"This train has been cancelled: {trip.CancellationReason}";
            }

            if (trip.DelayMinutes > 0)
                return $"This train is delayed by {trip.DelayMinutes} minutes.";

            if (HasPlatformChange(trip))
            {
                var platform = string.IsNullOrWhiteSpace(trip.Platform) ? "-" : trip.Platform;
                var track = string.IsNullOrWhiteSpace(trip.Track) ? "-" : trip.Track;
                return $"Platform changed. Please use platform {platform}, track {track}.";
            }

            return string.Empty;
        }
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Contracts.Payments;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class ApiIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task Login_ReturnsJwtToken()
        {
            var token = await LoginAsync();

            Assert.That(token, Is.Not.Empty);
        }

        [Test]
        public async Task TripSearch_ReturnsSeededTrip()
        {
            var response = await _client.GetAsync("/api/Trips/search?from=WAW&to=KRK&date=2026-07-01");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var trips = await response.Content.ReadFromJsonAsync<List<TripSearchResultDto>>();
            Assert.That(trips, Is.Not.Null);
            Assert.That(trips!, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(trips![0].DepartureStationCode, Is.EqualTo("WAW"));
            Assert.That(trips[0].ArrivalStationCode, Is.EqualTo("KRK"));
        }

        [Test]
        public async Task SeatAvailability_ReturnsAvailableSeatsForSeededTrip()
        {
            var trip = await GetFirstSeededTripAsync();

            var response = await _client.GetAsync($"/api/Trips/{trip.TripId}/seats");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var seats = await response.Content.ReadFromJsonAsync<List<TripSeatAvailabilityDto>>();
            Assert.That(seats, Is.Not.Null);
            Assert.That(seats!, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(seats!.Any(s => s.IsAvailable), Is.True);
        }

        [Test]
        public async Task BookingCreation_CreatesPendingBooking()
        {
            await AuthorizeAsPassengerAsync();
            var trip = await GetFirstSeededTripAsync();
            var seat = await GetFirstAvailableSeatAsync(trip.TripId);

            var booking = await CreateBookingAsync(trip, seat);

            Assert.That(booking.Id, Is.GreaterThan(0));
            Assert.That(booking.BookingStatus, Is.EqualTo("PendingPayment"));
            Assert.That(booking.ExpiresAtUtc, Is.Not.Null);
        }

        [Test]
        public async Task BookingCreation_RejectsDoubleBooking()
        {
            await AuthorizeAsPassengerAsync();
            var trip = await GetFirstSeededTripAsync();
            var seat = await GetFirstAvailableSeatAsync(trip.TripId);

            _ = await CreateBookingAsync(trip, seat);
            var secondResponse = await _client.PostAsJsonAsync("/api/Bookings", new CreateBookingRequest
            {
                TrainId = trip.TrainId,
                TripId = trip.TripId,
                SeatId = seat.SeatId,
                TravelDate = trip.DepartureTime.Date
            });

            Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var message = await secondResponse.Content.ReadAsStringAsync();
            Assert.That(message, Does.Contain("Seat already booked"));
        }

        [Test]
        public async Task PaymentConfirmation_ConfirmsBookingWithToken()
        {
            await AuthorizeAsPassengerAsync();
            var trip = await GetFirstSeededTripAsync();
            var seat = await GetFirstAvailableSeatAsync(trip.TripId);
            var booking = await CreateBookingAsync(trip, seat);

            var intentResponse = await _client.PostAsJsonAsync("/api/Payments/intent", new CreatePaymentIntentRequest
            {
                BookingId = booking.Id
            });
            Assert.That(intentResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var intent = await intentResponse.Content.ReadFromJsonAsync<PaymentIntentDto>();
            Assert.That(intent, Is.Not.Null);

            var confirmResponse = await _client.PostAsJsonAsync("/api/Payments/confirm", new ConfirmPaymentRequest
            {
                PaymentIntentId = intent!.PaymentIntentId,
                PaymentMethodToken = PaymentService.SuccessToken
            });

            Assert.That(confirmResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var payment = await confirmResponse.Content.ReadFromJsonAsync<PaymentDto>();
            Assert.That(payment, Is.Not.Null);
            Assert.That(payment!.Status, Is.EqualTo("Successful"));
            Assert.That(payment.PaymentIntentId, Is.EqualTo(intent.PaymentIntentId));
        }

        [Test]
        public async Task BookingCancellation_CancelsPendingBooking()
        {
            await AuthorizeAsPassengerAsync();
            var trip = await GetFirstSeededTripAsync();
            var seat = await GetFirstAvailableSeatAsync(trip.TripId);
            var booking = await CreateBookingAsync(trip, seat);

            var response = await _client.PostAsync($"/api/Bookings/{booking.Id}/cancel", content: null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        private async Task AuthorizeAsPassengerAsync()
        {
            var token = await LoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<string> LoginAsync()
        {
            var response = await _client.PostAsJsonAsync("/api/Auth/login", new LoginDto
            {
                Email = DevelopmentSeedData.PassengerEmail,
                Password = DevelopmentSeedData.DefaultPassword
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.That(login, Is.Not.Null);
            return login!.Token;
        }

        private async Task<TripSearchResultDto> GetFirstSeededTripAsync()
        {
            var response = await _client.GetAsync("/api/Trips/search?from=WAW&to=KRK&date=2026-07-01");
            response.EnsureSuccessStatusCode();
            var trips = await response.Content.ReadFromJsonAsync<List<TripSearchResultDto>>();
            return trips!.First();
        }

        private async Task<TripSeatAvailabilityDto> GetFirstAvailableSeatAsync(int tripId)
        {
            var response = await _client.GetAsync($"/api/Trips/{tripId}/seats");
            response.EnsureSuccessStatusCode();
            var seats = await response.Content.ReadFromJsonAsync<List<TripSeatAvailabilityDto>>();
            return seats!.First(s => s.IsAvailable);
        }

        private async Task<BookingDto> CreateBookingAsync(TripSearchResultDto trip, TripSeatAvailabilityDto seat)
        {
            var response = await _client.PostAsJsonAsync("/api/Bookings", new CreateBookingRequest
            {
                TrainId = trip.TrainId,
                TripId = trip.TripId,
                SeatId = seat.SeatId,
                TravelDate = trip.DepartureTime.Date
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
            Assert.That(booking, Is.Not.Null);
            return booking!;
        }
    }
}

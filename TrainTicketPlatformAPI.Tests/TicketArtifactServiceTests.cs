using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class TicketArtifactServiceTests
    {
        private static TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private static IConfiguration NewConfiguration()
            => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "ticket-artifact-test-signing-key"
                })
                .Build();

        private static async Task SeedConfirmedBookingAsync(TrainTicketDbContext db)
        {
            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
            var arrival = departure.AddHours(2);

            var departureStation = new Station { Id = 1, Code = "WAW", Name = "Warszawa Centralna", City = "Warsaw" };
            var arrivalStation = new Station { Id = 2, Code = "KRK", Name = "Krakow Glowny", City = "Krakow" };
            var route = new TrainRoute
            {
                Id = 1,
                Code = "WAW-KRK",
                Name = "Warszawa Centralna - Krakow Glowny",
                DepartureStationId = departureStation.Id,
                DepartureStation = departureStation,
                ArrivalStationId = arrivalStation.Id,
                ArrivalStation = arrivalStation,
                DistanceKm = 293m,
                EstimatedDurationMinutes = 160
            };
            var train = new Train
            {
                Id = 1,
                Code = "EIP-3510",
                Name = "EIP 3510",
                DepartureStation = departureStation.Name,
                ArrivalStation = arrivalStation.Name,
                DepartureTime = departure,
                ArrivalTime = arrival
            };
            var seat = new Seat
            {
                Id = 1,
                TrainId = train.Id,
                Train = train,
                Coach = "5",
                Number = "42",
                ClassType = "Class 2",
                IsAvailable = true
            };
            var trip = new Trip
            {
                Id = 1,
                TrainId = train.Id,
                Train = train,
                TrainRouteId = route.Id,
                TrainRoute = route,
                DepartureTime = departure,
                ArrivalTime = arrival,
                Status = "Scheduled"
            };

            db.Stations.AddRange(departureStation, arrivalStation);
            db.TrainRoutes.Add(route);
            db.Trains.Add(train);
            db.Seats.Add(seat);
            db.Trips.Add(trip);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                TrainId = train.Id,
                Train = train,
                TripId = trip.Id,
                Trip = trip,
                SeatId = seat.Id,
                Seat = seat,
                BookingReference = "BKG-TEST-123",
                TicketNumber = "WH2601011234",
                GuestEmail = "guest@example.com",
                PassengerName = "Test Passenger",
                BookingDate = DateTime.UtcNow,
                TravelDate = departure.Date,
                BookingStatus = "Confirmed",
                PaymentStatus = "Successful",
                ConfirmedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        [Test]
        public async Task GetTicketAsync_IssuesQrPayload_ForConfirmedBooking()
        {
            var db = NewDb("TicketArtifactIssued");
            await SeedConfirmedBookingAsync(db);
            var service = new TicketArtifactService(db, NewConfiguration());

            var ticket = await service.GetTicketAsync(1);

            Assert.That(ticket.TicketNumber, Is.EqualTo("WH2601011234"));
            Assert.That(ticket.QrPayload, Does.StartWith("railway-ticket-v1|ticket=WH2601011234"));
            Assert.That(ticket.QrPayload, Does.Contain("|sig="));
            Assert.That(ticket.IssuedAtUtc, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task GetQrSvgAndPdfAsync_ReturnRenderableArtifacts()
        {
            var db = NewDb("TicketArtifactRenderable");
            await SeedConfirmedBookingAsync(db);
            var service = new TicketArtifactService(db, NewConfiguration());

            var svg = await service.GetQrSvgAsync(1);
            var pdf = await service.GetTicketPdfAsync(1);

            Assert.That(svg, Does.Contain("<svg"));
            Assert.That(System.Text.Encoding.ASCII.GetString(pdf, 0, 8), Is.EqualTo("%PDF-1.4"));
        }

        [Test]
        public async Task SendTicketEmailAsync_RecordsDemoDelivery()
        {
            var db = NewDb("TicketArtifactEmail");
            await SeedConfirmedBookingAsync(db);
            var service = new TicketArtifactService(db, NewConfiguration());

            var delivery = await service.SendTicketEmailAsync(1, null);
            var booking = await db.Bookings.FindAsync(1);

            Assert.That(delivery.Status, Is.EqualTo("Sent"));
            Assert.That(delivery.RecipientEmail, Is.EqualTo("guest@example.com"));
            Assert.That(booking!.TicketEmailStatus, Is.EqualTo("Sent"));
            Assert.That(booking.TicketEmailSentAtUtc, Is.Not.Null);
        }

        [Test]
        public void GetTicketAsync_Throws_WhenBookingIsNotConfirmed()
        {
            var db = NewDb("TicketArtifactPending");
            db.Trains.Add(new Train { Id = 1, Name = "T1" });
            db.Seats.Add(new Seat { Id = 1, TrainId = 1, Coach = "1", Number = "1", IsAvailable = true });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = DateTime.UtcNow.AddDays(1),
                BookingStatus = "PendingPayment",
                PaymentStatus = "Pending"
            });
            db.SaveChanges();
            var service = new TicketArtifactService(db, NewConfiguration());

            Assert.ThrowsAsync<InvalidOperationException>(() => service.GetTicketAsync(1));
        }
    }
}

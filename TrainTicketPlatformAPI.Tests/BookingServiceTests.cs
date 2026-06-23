using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private void SeedTrain(TrainTicketDbContext db)
        {
            db.Users.AddRange(
                new User
                {
                    Id = 42,
                    Email = "passenger42@trainticket.dev",
                    NormalizedEmail = "PASSENGER42@TRAINTICKET.DEV",
                    PasswordHash = "hash",
                    Phone = "000-000-0042",
                    Role = "Passenger"
                },
                new User
                {
                    Id = 43,
                    Email = "passenger43@trainticket.dev",
                    NormalizedEmail = "PASSENGER43@TRAINTICKET.DEV",
                    PasswordHash = "hash",
                    Phone = "000-000-0043",
                    Role = "Passenger"
                });

            db.Trains.Add(new Train
            {
                Id = 1,
                Name = "T1",
                DepartureStation = "StationA",
                ArrivalStation = "StationB",
                DepartureTime = DateTime.UtcNow.AddHours(-2),
                ArrivalTime = DateTime.UtcNow.AddHours(-1)
            });
        }

        private void SeedSeat(TrainTicketDbContext db, int seatId, bool isAvailable)
        {
            db.Seats.Add(new Seat
            {
                Id = seatId,
                TrainId = 1,
                Coach = "A",
                Number = seatId.ToString(),
                ClassType = "Economy",
                IsAvailable = isAvailable
            });
        }

        private static void SeedTripRoute(TrainTicketDbContext db)
        {
            var departure = new Station
            {
                Id = 1,
                Code = "STA",
                Name = "Station A",
                City = "Station A"
            };
            var arrival = new Station
            {
                Id = 2,
                Code = "STB",
                Name = "Station B",
                City = "Station B"
            };

            db.Stations.AddRange(departure, arrival);
            db.TrainRoutes.Add(new TrainRoute
            {
                Id = 1,
                Code = "STA-STB",
                Name = "Station A - Station B",
                DepartureStationId = departure.Id,
                ArrivalStationId = arrival.Id,
                DepartureStation = departure,
                ArrivalStation = arrival,
                DistanceKm = 100m,
                EstimatedDurationMinutes = 120,
                IsActive = true
            });
        }

        private static void SeedSegmentTripRoute(TrainTicketDbContext db)
        {
            var departure = new Station
            {
                Id = 1,
                Code = "STA",
                Name = "Station A",
                City = "Station A"
            };
            var middle = new Station
            {
                Id = 2,
                Code = "STM",
                Name = "Station Middle",
                City = "Station Middle"
            };
            var arrival = new Station
            {
                Id = 3,
                Code = "STB",
                Name = "Station B",
                City = "Station B"
            };

            db.Stations.AddRange(departure, middle, arrival);
            db.TrainRoutes.Add(new TrainRoute
            {
                Id = 1,
                Code = "STA-STB",
                Name = "Station A - Station B",
                DepartureStationId = departure.Id,
                ArrivalStationId = arrival.Id,
                DepartureStation = departure,
                ArrivalStation = arrival,
                DistanceKm = 100m,
                EstimatedDurationMinutes = 120,
                IsActive = true,
                RouteStops =
                [
                    new TrainRouteStop
                    {
                        Id = 1,
                        StationId = middle.Id,
                        Station = middle,
                        StopOrder = 1
                    }
                ]
            });
        }

        // 1) Creation Tests

        [Test]
        public async Task CreateBookingAsync_CreatesBooking_WhenSeatAvailableForTravelDate()
        {
            var db = NewDb("CreateBookingTest");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            // pass a fresh Booking object
            var toCreate = new Booking
            {
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            };

            var result = await svc.CreateBookingAsync(toCreate);

            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.BookingReference, Does.StartWith("BKG-"));
            Assert.That(result.BookingStatus, Is.EqualTo("PendingPayment"));
            Assert.That(result.PaymentStatus, Is.EqualTo("Pending"));
            Assert.That(result.ExpiresAtUtc, Is.Not.Null);
            Assert.That(result.ExpiresAtUtc!.Value, Is.GreaterThan(DateTime.UtcNow.AddMinutes(14)));
            var seatAfter = await db.Seats.FindAsync(1);
            Assert.That(seatAfter!.IsAvailable, Is.True);
        }

        [Test]
        public async Task CreateBookingAsync_AllowsSeat_WhenPreviousPendingHoldExpired()
        {
            var db = NewDb("CreateBookingExpiredHold");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddMinutes(-20),
                TravelDate = DateTime.UtcNow.AddDays(1),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5),
                BookingStatus = "PendingPayment",
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var created = await svc.CreateBookingAsync(new Booking
            {
                UserId = 43,
                TrainId = 1,
                SeatId = 1,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });

            Assert.That(created.Id, Is.Not.EqualTo(1));
            var expired = await db.Bookings.FindAsync(1);
            Assert.That(expired!.BookingStatus, Is.EqualTo("Expired"));
        }

        [Test]
        public void CreateBookingAsync_Throws_WhenSeatUnavailable()
        {
            var db = NewDb("UnavailableSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.SaveChanges();

            var svc = new BookingService(db);
            var toCreate = new Booking
            {
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                TravelDate = DateTime.UtcNow.AddDays(1)
            };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CreateBookingAsync(toCreate)
            );
        }

        [Test]
        public void CreateBookingAsync_Throws_WhenTripBelongsToDifferentTrain()
        {
            var db = NewDb("BookingTripDifferentTrain");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedTripRoute(db);
            db.Trips.Add(new Trip
            {
                Id = 10,
                TrainId = 2,
                TrainRouteId = 1,
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = "Scheduled"
            });
            db.SaveChanges();

            var svc = new BookingService(db);
            var toCreate = new Booking
            {
                UserId = 42,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CreateBookingAsync(toCreate));
        }

        [Test]
        public async Task CreateBookingAsync_UsesTripDepartureDate_WhenTripProvided()
        {
            var db = NewDb("BookingUsesTripDate");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedTripRoute(db);
            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
            db.Trips.Add(new Trip
            {
                Id = 10,
                TrainId = 1,
                TrainRouteId = 1,
                DepartureTime = departure,
                ArrivalTime = departure.AddHours(2),
                Status = "Scheduled"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var created = await svc.CreateBookingAsync(new Booking
            {
                UserId = 42,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                TravelDate = DateTime.UtcNow.Date.AddYears(1),
                PaymentStatus = "Pending"
            });

            Assert.That(created.TravelDate, Is.EqualTo(departure.Date));
        }

        [Test]
        public void CreateBookingAsync_Throws_WhenSeatAlreadyBookedForTrip()
        {
            var db = NewDb("BookingDuplicateTripSeat");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedTripRoute(db);
            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
            db.Trips.Add(new Trip
            {
                Id = 10,
                TrainId = 1,
                TrainRouteId = 1,
                DepartureTime = departure,
                ArrivalTime = departure.AddHours(2),
                Status = "Scheduled"
            });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = departure.Date,
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateBookingAsync(new Booking
                {
                    UserId = 43,
                    TrainId = 1,
                    TripId = 10,
                    SeatId = 1,
                    TravelDate = departure.Date,
                    PaymentStatus = "Pending"
                }));
        }

        [Test]
        public async Task CreateBookingAsync_AllowsSameSeatOnNonOverlappingTripSegment()
        {
            var db = NewDb("BookingAllowsNonOverlappingSegment");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedSegmentTripRoute(db);
            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
            db.Trips.Add(new Trip
            {
                Id = 10,
                TrainId = 1,
                TrainRouteId = 1,
                DepartureTime = departure,
                ArrivalTime = departure.AddHours(2),
                Status = "Scheduled"
            });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = departure.Date,
                PaymentStatus = "Successful",
                BookingStatus = "Confirmed",
                SegmentDepartureStationId = 1,
                SegmentArrivalStationId = 2,
                SegmentDepartureOrder = 0,
                SegmentArrivalOrder = 1,
                SegmentDepartureTime = departure,
                SegmentArrivalTime = departure.AddHours(1)
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var created = await svc.CreateBookingAsync(new Booking
            {
                UserId = 43,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                SegmentDepartureStationId = 2,
                SegmentArrivalStationId = 3,
                PaymentStatus = "Pending"
            });

            Assert.That(created.SegmentDepartureOrder, Is.EqualTo(1));
            Assert.That(created.SegmentArrivalOrder, Is.EqualTo(2));
            Assert.That(created.TravelDate, Is.EqualTo(departure.Date));
        }

        [Test]
        public void CreateBookingAsync_BlocksSameSeatOnOverlappingTripSegment()
        {
            var db = NewDb("BookingBlocksOverlappingSegment");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedSegmentTripRoute(db);
            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(8);
            db.Trips.Add(new Trip
            {
                Id = 10,
                TrainId = 1,
                TrainRouteId = 1,
                DepartureTime = departure,
                ArrivalTime = departure.AddHours(2),
                Status = "Scheduled"
            });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                TripId = 10,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = departure.Date,
                PaymentStatus = "Successful",
                BookingStatus = "Confirmed",
                SegmentDepartureStationId = 1,
                SegmentArrivalStationId = 2,
                SegmentDepartureOrder = 0,
                SegmentArrivalOrder = 1,
                SegmentDepartureTime = departure,
                SegmentArrivalTime = departure.AddHours(1)
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateBookingAsync(new Booking
                {
                    UserId = 43,
                    TrainId = 1,
                    TripId = 10,
                    SeatId = 1,
                    SegmentDepartureStationId = 1,
                    SegmentArrivalStationId = 3,
                    PaymentStatus = "Pending"
                }));
        }

        // 2) Cancellation Tests

        [Test]
        public void CancelBookingAsync_Throws_WhenTooCloseToTravel()
        {
            var db = NewDb("CancelTooLateTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddMinutes(30),
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CancelBookingAsync(1),
                "Cannot cancel booking within 1 hour of travel date"
            );
        }

        [Test]
        public async Task CancelBookingAsync_MarksCancelled_WithoutChangingOperationalSeatFlag()
        {
            var db = NewDb("CancelBookingTest");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);
            await svc.CancelBookingAsync(1);

            var b = await db.Bookings.FindAsync(1);
            Assert.That(b!.IsCancelled, Is.True);
            Assert.That(b.CancellationDate, Is.Not.Null);
            Assert.That(b.BookingStatus, Is.EqualTo("Cancelled"));
            var seat = await db.Seats.FindAsync(1);
            Assert.That(seat!.IsAvailable, Is.True);
        }

        // 3) Rescheduling Tests

        [Test]
        public async Task UpdateBookingAsync_AllowsSeatChange_WhenNewSeatAvailable()
        {
            var db = NewDb("RescheduleSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            SeedSeat(db, 2, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            // pass *new* Booking object with just the changes
            var toReschedule = new Booking
            {
                Id = 1,
                SeatId = 2,                           // new seat
                TravelDate = DateTime.UtcNow.AddDays(1)   // same date
            };

            var updated = await svc.UpdateBookingAsync(toReschedule);

            Assert.That(updated.SeatId, Is.EqualTo(2));
            var oldSeat = await db.Seats.FindAsync(1);
            var newSeat = await db.Seats.FindAsync(2);
            Assert.That(oldSeat!.IsAvailable, Is.True);
            Assert.That(newSeat!.IsAvailable, Is.True);
        }

        [Test]
        public async Task ConfirmBookingAsync_SetsSuccessful_WhenSuccessfulPaymentExists()
        {
            var db = NewDb("ConfirmBookingSuccessfulPayment");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            db.Payments.Add(new Payment
            {
                Id = 1,
                BookingId = 1,
                PaymentDate = DateTime.UtcNow,
                Amount = 50m,
                Status = "Successful"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var confirmed = await svc.ConfirmBookingAsync(1);

            Assert.That(confirmed.PaymentStatus, Is.EqualTo("Successful"));
            Assert.That(confirmed.BookingStatus, Is.EqualTo("Confirmed"));
        }

        [Test]
        public void ConfirmBookingAsync_Throws_WhenNoSuccessfulPaymentExists()
        {
            var db = NewDb("ConfirmBookingNoSuccessfulPayment");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.ConfirmBookingAsync(1));
        }

        [Test]
        public void UpdateBookingAsync_Throws_WhenNewSeatUnavailable()
        {
            var db = NewDb("RescheduleToBadSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            SeedSeat(db, 2, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(2),
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            var toReschedule = new Booking { Id = 1, SeatId = 2 };
            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.UpdateBookingAsync(toReschedule),
                "New seat unavailable"
            );
        }

        [Test]
        public async Task UpdateBookingAsync_AllowsDateChange_WhenSeatFreeOnNewDate()
        {
            var db = NewDb("RescheduleDateTest");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);
            var newTravelDate = DateTime.UtcNow.AddDays(2);

            var toReschedule = new Booking
            {
                Id = 1,
                SeatId = 1,
                TravelDate = newTravelDate
            };
            var updated = await svc.UpdateBookingAsync(toReschedule);

            Assert.That(updated.TravelDate.Date, Is.EqualTo(newTravelDate.Date));
            var seat = await db.Seats.FindAsync(1);
            Assert.That(seat!.IsAvailable, Is.True);
        }

        [Test]
        public async Task CheckSeatAvailabilityAsync_AllowsSameSeatOnDifferentDate()
        {
            var db = NewDb("SeatAvailableDifferentDate");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            var bookedDate = DateTime.UtcNow.AddDays(1);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = bookedDate,
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var available = await svc.CheckSeatAvailabilityAsync(
                trainId: 1,
                seatId: 1,
                travelDate: bookedDate.AddDays(1));

            Assert.That(available, Is.True);
        }

        [Test]
        public async Task CheckSeatAvailabilityAsync_BlocksSameSeatOnSameDate()
        {
            var db = NewDb("SeatUnavailableSameDate");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            var bookedDate = DateTime.UtcNow.AddDays(1);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = bookedDate,
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var available = await svc.CheckSeatAvailabilityAsync(
                trainId: 1,
                seatId: 1,
                travelDate: bookedDate);

            Assert.That(available, Is.False);
        }

        [Test]
        public async Task CheckSeatAvailabilityAsync_IgnoresSameSeatIdOnDifferentTrain()
        {
            var db = NewDb("SeatAvailableDifferentTrain");
            SeedTrain(db);
            db.Trains.Add(new Train
            {
                Id = 2,
                Name = "T2",
                DepartureStation = "StationA",
                ArrivalStation = "StationB",
                DepartureTime = DateTime.UtcNow.AddHours(-2),
                ArrivalTime = DateTime.UtcNow.AddHours(-1)
            });
            SeedSeat(db, 1, true);
            db.Seats.Add(new Seat
            {
                Id = 2,
                TrainId = 2,
                Coach = "A",
                Number = "1",
                ClassType = "Economy",
                IsAvailable = true
            });
            var bookedDate = DateTime.UtcNow.AddDays(1);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = bookedDate,
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            var available = await svc.CheckSeatAvailabilityAsync(
                trainId: 2,
                seatId: 2,
                travelDate: bookedDate);

            Assert.That(available, Is.True);
        }
    }
}

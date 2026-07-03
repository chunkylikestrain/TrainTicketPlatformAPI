using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using TrainTicketPlatformAPI.Contracts.OpenRailway;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class OpenRailwayImportServiceTests
    {
        [Test]
        public async Task ImportRouteAsync_ReusesRoute_WhenStopFingerprintMatches()
        {
            var db = NewImportDb("OpenRailway_ReusesRouteShape");
            var client = new FakeOpenRailwayClient(
                CreateRoute(scheduleId: 10, orderId: 100, trainOrderId: 1000, trainNumber: "100", departureHour: 8),
                CreateRoute(scheduleId: 11, orderId: 101, trainOrderId: 1001, trainNumber: "101", departureHour: 10));
            var service = new OpenRailwayImportService(
                client,
                db,
                Options.Create(new OpenRailwayOptions
                {
                    ExternalSource = "PLK"
                }));

            var first = await service.ImportRouteAsync(10, 100, new DateOnly(2026, 7, 3), CancellationToken.None);
            var second = await service.ImportRouteAsync(11, 101, new DateOnly(2026, 7, 3), CancellationToken.None);

            var routes = db.TrainRoutes.ToList();
            var trips = db.Trips.ToList();
            var distinctTripRouteIds = trips.Select(trip => trip.TrainRouteId).Distinct().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(first.RouteCreated, Is.True);
                Assert.That(second.RouteCreated, Is.False);
                Assert.That(first.RouteReused, Is.False);
                Assert.That(second.RouteReused, Is.True);
                Assert.That(second.RouteFingerprint, Is.EqualTo("ORIGIN>MIDDLE>DESTINATION"));
                Assert.That(second.AdminDisplayName, Is.EqualTo("Origin to Destination via Middle"));
                Assert.That(routes, Has.Count.EqualTo(1));
                Assert.That(trips, Has.Count.EqualTo(2));
                Assert.That(distinctTripRouteIds, Has.Count.EqualTo(1));
                Assert.That(routes[0].RouteFingerprint, Is.EqualTo("ORIGIN>MIDDLE>DESTINATION"));
                Assert.That(routes[0].AdminDisplayName, Is.EqualTo("Origin to Destination via Middle"));
                Assert.That(routes[0].Code, Does.StartWith("ORIGIN-DESTINAT-"));
            });
        }

        private static TrainTicketDbContext NewImportDb(string name)
        {
            var options = new DbContextOptionsBuilder<TrainTicketDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new TrainTicketDbContext(options);
        }

        private static OpenRailwayRouteDto CreateRoute(
            int scheduleId,
            int orderId,
            int trainOrderId,
            string trainNumber,
            int departureHour)
        {
            return new OpenRailwayRouteDto
            {
                ScheduleId = scheduleId,
                OrderId = orderId,
                TrainOrderId = trainOrderId,
                Name = "Test IC",
                CarrierCode = "IC",
                NationalNumber = trainNumber,
                CommercialCategorySymbol = "IC",
                OperatingDates = [new DateOnly(2026, 7, 3)],
                Stations =
                [
                    new OpenRailwayStationOnRouteDto
                    {
                        StationId = 1,
                        OrderNumber = 1,
                        DepartureDay = 0,
                        DepartureTime = new TimeSpan(departureHour, 0, 0)
                    },
                    new OpenRailwayStationOnRouteDto
                    {
                        StationId = 2,
                        OrderNumber = 2,
                        ArrivalDay = 0,
                        ArrivalTime = new TimeSpan(departureHour, 30, 0),
                        DepartureDay = 0,
                        DepartureTime = new TimeSpan(departureHour, 32, 0)
                    },
                    new OpenRailwayStationOnRouteDto
                    {
                        StationId = 3,
                        OrderNumber = 3,
                        ArrivalDay = 0,
                        ArrivalTime = new TimeSpan(departureHour + 1, 0, 0)
                    }
                ]
            };
        }

        private sealed class FakeOpenRailwayClient : IOpenRailwayClient
        {
            private readonly Dictionary<(int ScheduleId, int OrderId), OpenRailwayRouteDto> _routes;

            public FakeOpenRailwayClient(params OpenRailwayRouteDto[] routes)
            {
                _routes = routes.ToDictionary(route => (route.ScheduleId, route.OrderId));
            }

            public Task<OpenRailwayDataVersionDto> GetDataVersionAsync(CancellationToken cancellationToken)
                => Task.FromResult(new OpenRailwayDataVersionDto
                {
                    DataVersion = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    SchedulesVersion = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Timestamp = DateTime.UtcNow
                });

            public Task<OpenRailwayStationsResponseDto> SearchStationsAsync(
                string? search,
                int page,
                int pageSize,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new OpenRailwayStationsResponseDto
                {
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 3,
                    ReturnedCount = 3,
                    TotalPages = 1,
                    Stations =
                    [
                        new OpenRailwayStationDto { Id = 1, Name = "Origin" },
                        new OpenRailwayStationDto { Id = 2, Name = "Middle" },
                        new OpenRailwayStationDto { Id = 3, Name = "Destination" }
                    ]
                });
            }

            public Task<OpenRailwayRouteIdsResponseDto> GetRouteIdsAsync(
                DateOnly date,
                CancellationToken cancellationToken)
                => Task.FromResult(new OpenRailwayRouteIdsResponseDto());

            public Task<OpenRailwayRouteDto> GetRouteAsync(
                int scheduleId,
                int orderId,
                CancellationToken cancellationToken)
                => Task.FromResult(_routes[(scheduleId, orderId)]);
        }
    }
}

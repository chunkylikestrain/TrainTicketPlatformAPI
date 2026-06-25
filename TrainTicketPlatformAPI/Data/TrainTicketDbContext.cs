using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Data
{
    public class TrainTicketDbContext : DbContext
    {
        public TrainTicketDbContext(DbContextOptions<TrainTicketDbContext> options)
            : base(options) { }
        // Each DbSet<T> represents a table:
        public DbSet<User> Users { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<StateRegion> StateRegions { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<TrainRoute> TrainRoutes { get; set; }
        public DbSet<TrainRouteStop> TrainRouteStops { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<TrainCarriage> TrainCarriages { get; set; }
        public DbSet<RollingStockOption> RollingStockOptions { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Fare> Fares { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingOrder> BookingOrders { get; set; }
        public DbSet<BookingReport> bookingReports { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DiscountRule> DiscountRules { get; set; }
        public DbSet<TicketEmailDelivery> TicketEmailDeliveries { get; set; }

        // Optional: Fluent API configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Booking → Train: Restrict (no cascade)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Train)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TrainId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → Seat: restrict to preserve booking history
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Seat)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → User: restrict to preserve booking history
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingOrder>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.BookingOrder)
                .WithMany(o => o.Bookings)
                .HasForeignKey(b => b.BookingOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.SegmentDepartureStation)
                .WithMany()
                .HasForeignKey(b => b.SegmentDepartureStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.SegmentArrivalStation)
                .WithMany()
                .HasForeignKey(b => b.SegmentArrivalStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TripId, b.SeatId })
                .HasFilter("[IsCancelled] = 0 AND [TripId] IS NOT NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TrainId, b.SeatId, b.TravelDate })
                .IsUnique()
                .HasFilter("[IsCancelled] = 0 AND [TripId] IS NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.TicketNumber)
                .IsUnique()
                .HasFilter("[TicketNumber] <> ''");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.GuestEmail);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.UserId);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingOrderId);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TripId, b.TravelDate });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.SeatId, b.TravelDate });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TripId, b.SeatId, b.SegmentDepartureOrder, b.SegmentArrivalOrder });

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingReference)
                .HasMaxLength(40)
                .HasDefaultValueSql("'BKG-' + CONVERT(varchar(36), NEWID())");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TicketNumber)
                .HasMaxLength(40)
                .HasDefaultValue("");

            modelBuilder.Entity<Booking>()
                .Property(b => b.GuestEmail)
                .HasMaxLength(256);

            modelBuilder.Entity<Booking>()
                .Property(b => b.PassengerName)
                .HasMaxLength(200);

            modelBuilder.Entity<Booking>()
                .Property(b => b.PassengerType)
                .HasMaxLength(40)
                .HasDefaultValue("Adult");

            modelBuilder.Entity<Booking>()
                .Property(b => b.DiscountCode)
                .HasMaxLength(40)
                .HasDefaultValue("normal");

            modelBuilder.Entity<Booking>()
                .Property(b => b.DiscountName)
                .HasMaxLength(120)
                .HasDefaultValue("Normal Ticket");

            modelBuilder.Entity<Booking>()
                .Property(b => b.DiscountPercent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.BaseAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.Currency)
                .HasMaxLength(8)
                .HasDefaultValue("PLN");

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingStatus)
                .HasMaxLength(32)
                .HasDefaultValue("PendingPayment");

            modelBuilder.Entity<Booking>()
                .Property(b => b.PaymentStatus)
                .HasMaxLength(32)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Booking>()
                .Property(b => b.CancellationReason)
                .HasMaxLength(300);

            modelBuilder.Entity<Booking>()
                .Property(b => b.TicketQrPayload)
                .HasMaxLength(1200)
                .HasDefaultValue("");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TicketEmailStatus)
                .HasMaxLength(32)
                .HasDefaultValue("");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TicketEmailRecipient)
                .HasMaxLength(256)
                .HasDefaultValue("");

            modelBuilder.Entity<Booking>()
                .Property(b => b.ExpiresAtUtc);

            modelBuilder.Entity<Booking>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Bookings_BookingStatus",
                        "[BookingStatus] IN ('PendingPayment', 'Confirmed', 'Cancelled', 'Expired', 'Refunded')");
                    t.HasCheckConstraint(
                        "CK_Bookings_PaymentStatus",
                        "[PaymentStatus] IN ('Pending', 'Successful', 'Failed', 'Refunded')");
                });

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.BookingOrder)
                .WithMany()
                .HasForeignKey(p => p.BookingOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.BookingId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.BookingOrderId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentIntentId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentIntentId)
                .HasMaxLength(64);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethodToken)
                .HasMaxLength(64);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasMaxLength(32);

            modelBuilder.Entity<Payment>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Payments_Status",
                        "[Status] IN ('Successful', 'Failed', 'Refunded')");
                    t.HasCheckConstraint(
                        "CK_Payments_Target",
                        "([BookingId] IS NOT NULL AND [BookingOrderId] IS NULL) OR ([BookingId] IS NULL AND [BookingOrderId] IS NOT NULL)");
                });

            modelBuilder.Entity<BookingOrder>()
                .HasIndex(o => o.OrderReference)
                .IsUnique();

            modelBuilder.Entity<BookingOrder>()
                .HasIndex(o => o.GuestEmail);

            modelBuilder.Entity<BookingOrder>()
                .HasIndex(o => o.ItineraryId);

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.OrderReference)
                .HasMaxLength(40);

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.ItineraryId)
                .HasMaxLength(120);

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.GuestEmail)
                .HasMaxLength(256);

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.BookingStatus)
                .HasMaxLength(32)
                .HasDefaultValue("PendingPayment");

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.PaymentStatus)
                .HasMaxLength(32)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<BookingOrder>()
                .Property(o => o.SegmentCount)
                .HasDefaultValue(1);

            modelBuilder.Entity<BookingOrder>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_BookingOrders_BookingStatus",
                        "[BookingStatus] IN ('PendingPayment', 'Confirmed', 'Cancelled', 'Expired', 'Refunded')");
                    t.HasCheckConstraint(
                        "CK_BookingOrders_PaymentStatus",
                        "[PaymentStatus] IN ('Pending', 'Successful', 'Failed', 'Refunded')");
                });

            modelBuilder.Entity<TicketEmailDelivery>()
                .HasOne(d => d.Booking)
                .WithMany(b => b.TicketEmailDeliveries)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketEmailDelivery>()
                .HasIndex(d => d.BookingId);

            modelBuilder.Entity<TicketEmailDelivery>()
                .Property(d => d.RecipientEmail)
                .HasMaxLength(256);

            modelBuilder.Entity<TicketEmailDelivery>()
                .Property(d => d.Status)
                .HasMaxLength(32);

            modelBuilder.Entity<TicketEmailDelivery>()
                .Property(d => d.ProviderMessageId)
                .HasMaxLength(80);

            modelBuilder.Entity<TicketEmailDelivery>()
                .Property(d => d.ErrorMessage)
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<User>()
                .Property(u => u.DisplayName)
                .HasMaxLength(200)
                .HasDefaultValue("");

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasMaxLength(32)
                .HasDefaultValue("Active");

            modelBuilder.Entity<User>()
                .Property(u => u.NormalizedEmail)
                .HasMaxLength(256)
                .HasComputedColumnSql("UPPER(LTRIM(RTRIM([Email])))", stored: true);

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.NormalizedCode)
                .IsUnique();

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.NormalizedName);

            modelBuilder.Entity<Station>()
                .Property(s => s.Code)
                .HasMaxLength(32);

            modelBuilder.Entity<Station>()
                .Property(s => s.NormalizedCode)
                .HasMaxLength(32)
                .HasComputedColumnSql("UPPER(LTRIM(RTRIM([Code])))", stored: true);

            modelBuilder.Entity<Station>()
                .Property(s => s.Name)
                .HasMaxLength(200);

            modelBuilder.Entity<Station>()
                .Property(s => s.NormalizedName)
                .HasMaxLength(200)
                .HasComputedColumnSql("UPPER(LTRIM(RTRIM([Name])))", stored: true);

            modelBuilder.Entity<Station>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Stations_Code_NotBlank", "LEN(LTRIM(RTRIM([Code]))) > 0");
                    t.HasCheckConstraint("CK_Stations_Name_NotBlank", "LEN(LTRIM(RTRIM([Name]))) > 0");
                });

            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<StateRegion>()
                .HasOne(s => s.Country)
                .WithMany(c => c.StateRegions)
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StateRegion>()
                .HasIndex(s => new { s.CountryId, s.Code })
                .IsUnique();

            modelBuilder.Entity<Locality>()
                .HasOne(l => l.StateRegion)
                .WithMany(s => s.Localities)
                .HasForeignKey(l => l.StateRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Locality>()
                .HasIndex(l => new { l.StateRegionId, l.Name, l.Type });

            modelBuilder.Entity<Station>()
                .HasOne(s => s.Country)
                .WithMany(c => c.Stations)
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Station>()
                .HasOne(s => s.StateRegion)
                .WithMany(r => r.Stations)
                .HasForeignKey(s => s.StateRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Station>()
                .HasOne(s => s.Locality)
                .WithMany(l => l.Stations)
                .HasForeignKey(s => s.LocalityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainRoute>()
                .HasOne(r => r.DepartureStation)
                .WithMany(s => s.DepartureRoutes)
                .HasForeignKey(r => r.DepartureStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainRoute>()
                .HasOne(r => r.ArrivalStation)
                .WithMany(s => s.ArrivalRoutes)
                .HasForeignKey(r => r.ArrivalStationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainRouteStop>()
                .HasOne(s => s.TrainRoute)
                .WithMany(r => r.RouteStops)
                .HasForeignKey(s => s.TrainRouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainRouteStop>()
                .HasOne(s => s.Station)
                .WithMany()
                .HasForeignKey(s => s.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainRouteStop>()
                .HasIndex(s => new { s.TrainRouteId, s.StopOrder })
                .IsUnique();

            modelBuilder.Entity<TrainRouteStop>()
                .HasIndex(s => new { s.TrainRouteId, s.StationId })
                .IsUnique();

            modelBuilder.Entity<TrainRouteStop>()
                .Property(s => s.Platform)
                .HasMaxLength(20);

            modelBuilder.Entity<TrainRouteStop>()
                .Property(s => s.Track)
                .HasMaxLength(20);

            modelBuilder.Entity<TrainRouteStop>()
                .Property(s => s.StopType)
                .HasMaxLength(30);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Train)
                .WithMany(t => t.Trips)
                .HasForeignKey(t => t.TrainId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.TrainRoute)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.TrainRouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrainRoute>()
                .HasIndex(r => r.Code)
                .IsUnique()
                .HasFilter("[Code] <> ''");

            modelBuilder.Entity<TrainRoute>()
                .Property(r => r.Code)
                .HasMaxLength(32)
                .HasDefaultValue("");

            modelBuilder.Entity<TrainRoute>()
                .Property(r => r.Name)
                .HasMaxLength(240)
                .HasDefaultValue("");

            modelBuilder.Entity<TrainRoute>()
                .Property(r => r.OperatingDays)
                .HasMaxLength(80)
                .HasDefaultValue("Daily");

            modelBuilder.Entity<TrainRoute>()
                .Property(r => r.IntermediateStops)
                .HasMaxLength(1000)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .HasIndex(t => new { t.TrainRouteId, t.DepartureTime });

            modelBuilder.Entity<Trip>()
                .Property(t => t.Platform)
                .HasMaxLength(40)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.Track)
                .HasMaxLength(40)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.CancellationReason)
                .HasMaxLength(500)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.OriginalPlatform)
                .HasMaxLength(40)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.OriginalTrack)
                .HasMaxLength(40)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.DisruptionMessage)
                .HasMaxLength(500)
                .HasDefaultValue("");

            modelBuilder.Entity<Trip>()
                .Property(t => t.DisruptionSeverity)
                .HasMaxLength(30)
                .HasDefaultValue("");

            modelBuilder.Entity<Fare>()
                .HasOne(f => f.Trip)
                .WithMany(t => t.Fares)
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment.Amount → decimal(18,2)
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TrainRoute>()
                .Property(r => r.DistanceKm)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Train>()
                .HasIndex(t => t.Code)
                .IsUnique()
                .HasFilter("[Code] <> ''");

            modelBuilder.Entity<Train>()
                .Property(t => t.Code)
                .HasMaxLength(32)
                .HasDefaultValue("");

            modelBuilder.Entity<Train>()
                .Property(t => t.Type)
                .HasMaxLength(80)
                .HasDefaultValue("InterCity");

            modelBuilder.Entity<Train>()
                .Property(t => t.Status)
                .HasMaxLength(32)
                .HasDefaultValue("Active");

            modelBuilder.Entity<TrainCarriage>()
                .HasOne(c => c.Train)
                .WithMany(t => t.Carriages)
                .HasForeignKey(c => c.TrainId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainCarriage>()
                .HasIndex(c => new { c.TrainId, c.Coach })
                .IsUnique();

            modelBuilder.Entity<TrainCarriage>()
                .Property(c => c.Coach)
                .HasMaxLength(20);

            modelBuilder.Entity<TrainCarriage>()
                .Property(c => c.ClassType)
                .HasMaxLength(40);

            modelBuilder.Entity<TrainCarriage>()
                .Property(c => c.LayoutType)
                .HasMaxLength(60);

            modelBuilder.Entity<TrainCarriage>()
                .Property(c => c.VehicleType)
                .HasMaxLength(120);

            modelBuilder.Entity<TrainCarriage>()
                .Property(c => c.Notes)
                .HasMaxLength(500);

            modelBuilder.Entity<RollingStockOption>()
                .HasIndex(r => new { r.Category, r.Series })
                .IsUnique();

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.Category)
                .HasMaxLength(40);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.Series)
                .HasMaxLength(40);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.DisplayName)
                .HasMaxLength(120);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.Manufacturer)
                .HasMaxLength(120);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.MaxSpeed)
                .HasMaxLength(40);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.Notes)
                .HasMaxLength(500);

            modelBuilder.Entity<RollingStockOption>()
                .Property(r => r.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Active");

            modelBuilder.Entity<Fare>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.TrainId, s.Coach, s.Number })
                .IsUnique();

            // Makes bookinf report keyless for querying
            modelBuilder
                .Entity<BookingReport>()
                .HasNoKey();

            modelBuilder.Entity<BookingReport>()
                .Property(r => r.TotalRevenue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DiscountRule>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<DiscountRule>()
                .Property(d => d.Name)
                .HasMaxLength(120);

            modelBuilder.Entity<DiscountRule>()
                .Property(d => d.Percent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DiscountRule>()
                .Property(d => d.EligibleClass)
                .HasMaxLength(40);

            modelBuilder.Entity<DiscountRule>()
                .Property(d => d.DocumentHint)
                .HasMaxLength(300);

            modelBuilder.Entity<DiscountRule>()
                .Property(d => d.Status)
                .HasMaxLength(32)
                .HasDefaultValue("Active");
        }
    }
}

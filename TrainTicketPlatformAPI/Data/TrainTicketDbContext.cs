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
        public DbSet<Train> Trains { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Fare> Fares { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingReport> bookingReports { get; set; }
        public DbSet<Payment> Payments { get; set; }

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

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TripId, b.SeatId })
                .IsUnique()
                .HasFilter("[IsCancelled] = 0 AND [TripId] IS NOT NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TrainId, b.SeatId, b.TravelDate })
                .IsUnique()
                .HasFilter("[IsCancelled] = 0 AND [TripId] IS NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.UserId);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.TripId, b.TravelDate });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.SeatId, b.TravelDate });

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingReference)
                .HasMaxLength(40)
                .HasDefaultValueSql("'BKG-' + CONVERT(varchar(36), NEWID())");

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingStatus)
                .HasMaxLength(32)
                .HasDefaultValue("PendingPayment");

            modelBuilder.Entity<Booking>()
                .Property(b => b.PaymentStatus)
                .HasMaxLength(32)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Booking>()
                .Property(b => b.ExpiresAtUtc);

            modelBuilder.Entity<Booking>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Bookings_BookingStatus",
                        "[BookingStatus] IN ('PendingPayment', 'Confirmed', 'Cancelled', 'Expired')");
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
                .HasIndex(p => p.BookingId);

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
                });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256);

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

            modelBuilder.Entity<Trip>()
                .HasIndex(t => new { t.TrainRouteId, t.DepartureTime });

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
        }
    }
}

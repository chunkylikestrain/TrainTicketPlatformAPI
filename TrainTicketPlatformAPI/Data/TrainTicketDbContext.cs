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

            // Booking → Seat: cascade 
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Seat)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SeatId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking → User: Cascade 
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.Code)
                .IsUnique();

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

            modelBuilder.Entity<Fare>()
                .HasOne(f => f.Trip)
                .WithMany(t => t.Fares)
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Train.Price → decimal(18,2)
            modelBuilder.Entity<Train>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

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

            // Makes bookinf report keyless for querying
            modelBuilder
           .Entity<BookingReport>()
           .HasNoKey();
        }
    }
}

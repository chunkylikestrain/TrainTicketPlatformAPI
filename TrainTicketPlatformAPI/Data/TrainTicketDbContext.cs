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
        public DbSet<Train> Trains { get; set; }
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

            // Train.Price → decimal(18,2)
            modelBuilder.Entity<Train>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            // Payment.Amount → decimal(18,2)
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // Makes bookinf report keyless for querying
            modelBuilder
           .Entity<BookingReport>()
           .HasNoKey();
        }
    }
}


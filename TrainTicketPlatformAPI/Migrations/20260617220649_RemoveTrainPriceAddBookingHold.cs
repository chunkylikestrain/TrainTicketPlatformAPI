using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrainPriceAddBookingHold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TrainId_SeatId_TravelDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Trains");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TrainId_SeatId_TravelDate",
                table: "Bookings",
                columns: new[] { "TrainId", "SeatId", "TravelDate" },
                unique: true,
                filter: "[IsCancelled] = 0 AND [TripId] IS NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings",
                columns: new[] { "TripId", "SeatId" },
                unique: true,
                filter: "[IsCancelled] = 0 AND [TripId] IS NOT NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TrainId_SeatId_TravelDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "Bookings");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Trains",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TrainId_SeatId_TravelDate",
                table: "Bookings",
                columns: new[] { "TrainId", "SeatId", "TravelDate" },
                unique: true,
                filter: "[IsCancelled] = 0 AND [TripId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings",
                columns: new[] { "TripId", "SeatId" },
                unique: true,
                filter: "[IsCancelled] = 0 AND [TripId] IS NOT NULL");
        }
    }
}

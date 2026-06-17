using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class HardenBookingSeatConflicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TrainId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TrainId_SeatId_TravelDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TrainId",
                table: "Bookings",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId",
                table: "Bookings",
                column: "TripId");
        }
    }
}

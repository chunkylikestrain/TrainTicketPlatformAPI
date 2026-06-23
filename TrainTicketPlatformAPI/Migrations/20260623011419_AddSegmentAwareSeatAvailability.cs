using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSegmentAwareSeatAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "SegmentArrivalOrder",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentArrivalStationId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SegmentArrivalTime",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentDepartureOrder",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentDepartureStationId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SegmentDepartureTime",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SegmentArrivalStationId",
                table: "Bookings",
                column: "SegmentArrivalStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SegmentDepartureStationId",
                table: "Bookings",
                column: "SegmentDepartureStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings",
                columns: new[] { "TripId", "SeatId" },
                filter: "[IsCancelled] = 0 AND [TripId] IS NOT NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_SeatId_SegmentDepartureOrder_SegmentArrivalOrder",
                table: "Bookings",
                columns: new[] { "TripId", "SeatId", "SegmentDepartureOrder", "SegmentArrivalOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Stations_SegmentArrivalStationId",
                table: "Bookings",
                column: "SegmentArrivalStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Stations_SegmentDepartureStationId",
                table: "Bookings",
                column: "SegmentDepartureStationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Stations_SegmentArrivalStationId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Stations_SegmentDepartureStationId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SegmentArrivalStationId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SegmentDepartureStationId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_SeatId_SegmentDepartureOrder_SegmentArrivalOrder",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentArrivalOrder",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentArrivalStationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentArrivalTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentDepartureOrder",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentDepartureStationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SegmentDepartureTime",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_SeatId",
                table: "Bookings",
                columns: new[] { "TripId", "SeatId" },
                unique: true,
                filter: "[IsCancelled] = 0 AND [TripId] IS NOT NULL AND [BookingStatus] IN ('PendingPayment', 'Confirmed')");
        }
    }
}

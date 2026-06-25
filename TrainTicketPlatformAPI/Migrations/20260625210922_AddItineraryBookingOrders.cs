using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddItineraryBookingOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsItinerary",
                table: "BookingOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItineraryId",
                table: "BookingOrders",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JourneyArrivalStationId",
                table: "BookingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JourneyArrivalTime",
                table: "BookingOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JourneyDepartureStationId",
                table: "BookingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JourneyDepartureTime",
                table: "BookingOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SegmentCount",
                table: "BookingOrders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_BookingOrders_ItineraryId",
                table: "BookingOrders",
                column: "ItineraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingOrders_ItineraryId",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "IsItinerary",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "ItineraryId",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "JourneyArrivalStationId",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "JourneyArrivalTime",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "JourneyDepartureStationId",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "JourneyDepartureTime",
                table: "BookingOrders");

            migrationBuilder.DropColumn(
                name: "SegmentCount",
                table: "BookingOrders");
        }
    }
}

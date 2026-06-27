using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundTripOrderJourneys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JourneyDirection",
                table: "Bookings",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Outbound");

            migrationBuilder.AddColumn<int>(
                name: "JourneySegmentIndex",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TripType",
                table: "BookingOrders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "OneWay");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingOrderId_JourneyDirection_JourneySegmentIndex",
                table: "Bookings",
                columns: new[] { "BookingOrderId", "JourneyDirection", "JourneySegmentIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingOrderId_JourneyDirection_JourneySegmentIndex",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "JourneyDirection",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "JourneySegmentIndex",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TripType",
                table: "BookingOrders");
        }
    }
}

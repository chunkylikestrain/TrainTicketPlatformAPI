using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDiscountPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Bookings",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "PLN");

            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "Bookings",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "normal");

            migrationBuilder.AddColumn<string>(
                name: "DiscountName",
                table: "Bookings",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "Normal Ticket");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Bookings",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PassengerType",
                table: "Bookings",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Adult");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PassengerType",
                table: "Bookings");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripDisruptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Trips",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DelayMinutes",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisruptionMessage",
                table: "Trips",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisruptionSeverity",
                table: "Trips",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalPlatform",
                table: "Trips",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalTrack",
                table: "Trips",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DelayMinutes",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DisruptionMessage",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "DisruptionSeverity",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "OriginalPlatform",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "OriginalTrack",
                table: "Trips");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteStopTimetableDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArrivalOffsetMinutes",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartureOffsetMinutes",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "TrainRouteStops",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StopType",
                table: "TrainRouteStops",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Track",
                table: "TrainRouteStops",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalOffsetMinutes",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "DepartureOffsetMinutes",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "StopType",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "Track",
                table: "TrainRouteStops");
        }
    }
}

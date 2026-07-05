using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trips_ArrivalTime",
                table: "Trips",
                column: "ArrivalTime");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DepartureTime",
                table: "Trips",
                column: "DepartureTime");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ExternalOperatingDate",
                table: "Trips",
                column: "ExternalOperatingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trips_ArrivalTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_DepartureTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ExternalOperatingDate",
                table: "Trips");
        }
    }
}

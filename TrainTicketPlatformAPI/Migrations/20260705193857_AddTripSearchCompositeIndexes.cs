using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripSearchCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trips_DepartureTime_ArrivalTime",
                table: "Trips",
                columns: new[] { "DepartureTime", "ArrivalTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ExternalOperatingDate_DepartureTime_ArrivalTime",
                table: "Trips",
                columns: new[] { "ExternalOperatingDate", "DepartureTime", "ArrivalTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trips_DepartureTime_ArrivalTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ExternalOperatingDate_DepartureTime_ArrivalTime",
                table: "Trips");
        }
    }
}

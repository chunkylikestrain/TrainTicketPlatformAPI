using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripServiceIdentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TripServiceIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CarrierCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: ""),
                    CountryCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: ""),
                    ServiceCategory = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false, defaultValue: ""),
                    Number = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: ""),
                    DisplayNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false, defaultValue: ""),
                    ServiceName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false, defaultValue: ""),
                    FromRouteStopId = table.Column<int>(type: "int", nullable: true),
                    ToRouteStopId = table.Column<int>(type: "int", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    ExternalSource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripServiceIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripServiceIdentities_TrainRouteStops_FromRouteStopId",
                        column: x => x.FromRouteStopId,
                        principalTable: "TrainRouteStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripServiceIdentities_TrainRouteStops_ToRouteStopId",
                        column: x => x.ToRouteStopId,
                        principalTable: "TrainRouteStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripServiceIdentities_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TripServiceIdentities_ExternalSource_CarrierCode_ServiceCategory_Number",
                table: "TripServiceIdentities",
                columns: new[] { "ExternalSource", "CarrierCode", "ServiceCategory", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_TripServiceIdentities_FromRouteStopId",
                table: "TripServiceIdentities",
                column: "FromRouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_TripServiceIdentities_ToRouteStopId",
                table: "TripServiceIdentities",
                column: "ToRouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_TripServiceIdentities_TripId_DisplayOrder",
                table: "TripServiceIdentities",
                columns: new[] { "TripId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TripServiceIdentities_TripId_IsPrimary",
                table: "TripServiceIdentities",
                columns: new[] { "TripId", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripServiceIdentities");
        }
    }
}

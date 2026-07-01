using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTripCarriageSegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TripCarriageSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    TrainCarriageId = table.Column<int>(type: "int", nullable: false),
                    FromRouteStopId = table.Column<int>(type: "int", nullable: true),
                    ToRouteStopId = table.Column<int>(type: "int", nullable: true),
                    PortionCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false, defaultValue: ""),
                    DestinationLabel = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false, defaultValue: ""),
                    IsBookable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripCarriageSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripCarriageSegments_TrainCarriages_TrainCarriageId",
                        column: x => x.TrainCarriageId,
                        principalTable: "TrainCarriages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripCarriageSegments_TrainRouteStops_FromRouteStopId",
                        column: x => x.FromRouteStopId,
                        principalTable: "TrainRouteStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripCarriageSegments_TrainRouteStops_ToRouteStopId",
                        column: x => x.ToRouteStopId,
                        principalTable: "TrainRouteStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripCarriageSegments_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_FromRouteStopId",
                table: "TripCarriageSegments",
                column: "FromRouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_ToRouteStopId",
                table: "TripCarriageSegments",
                column: "ToRouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_TrainCarriageId",
                table: "TripCarriageSegments",
                column: "TrainCarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_TripId_DisplayOrder",
                table: "TripCarriageSegments",
                columns: new[] { "TripId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_TripId_FromRouteStopId_ToRouteStopId",
                table: "TripCarriageSegments",
                columns: new[] { "TripId", "FromRouteStopId", "ToRouteStopId" });

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_TripId_PortionCode",
                table: "TripCarriageSegments",
                columns: new[] { "TripId", "PortionCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TripCarriageSegments_TripId_TrainCarriageId",
                table: "TripCarriageSegments",
                columns: new[] { "TripId", "TrainCarriageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripCarriageSegments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredRouteStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TrainRoutes",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TrainRouteStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainRouteId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainRouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainRouteStops_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainRouteStops_TrainRoutes_TrainRouteId",
                        column: x => x.TrainRouteId,
                        principalTable: "TrainRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainRouteStops_StationId",
                table: "TrainRouteStops",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainRouteStops_TrainRouteId_StationId",
                table: "TrainRouteStops",
                columns: new[] { "TrainRouteId", "StationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainRouteStops_TrainRouteId_StopOrder",
                table: "TrainRouteStops",
                columns: new[] { "TrainRouteId", "StopOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TrainRoutes");
        }
    }
}

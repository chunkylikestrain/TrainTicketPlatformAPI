using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminDisplayName",
                table: "TrainRoutes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RouteFingerprint",
                table: "TrainRoutes",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TrainRoutes_RouteFingerprint",
                table: "TrainRoutes",
                column: "RouteFingerprint",
                filter: "[RouteFingerprint] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainRoutes_RouteFingerprint",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "AdminDisplayName",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "RouteFingerprint",
                table: "TrainRoutes");
        }
    }
}

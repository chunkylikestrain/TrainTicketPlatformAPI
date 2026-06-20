using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminManagementFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Trips",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Track",
                table: "Trips",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CarriageCount",
                table: "Trains",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Trains",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeatsPerCarriage",
                table: "Trains",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Trains",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Trains",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "InterCity");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TrainRoutes",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "TrainRoutes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IntermediateStops",
                table: "TrainRoutes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingDays",
                table: "TrainRoutes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Daily");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscountRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EligibleClass = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DocumentHint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trains_Code",
                table: "Trains",
                column: "Code",
                unique: true,
                filter: "[Code] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_TrainRoutes_Code",
                table: "TrainRoutes",
                column: "Code",
                unique: true,
                filter: "[Code] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_Name",
                table: "DiscountRules",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountRules");

            migrationBuilder.DropIndex(
                name: "IX_Trains_Code",
                table: "Trains");

            migrationBuilder.DropIndex(
                name: "IX_TrainRoutes_Code",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Track",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CarriageCount",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "SeatsPerCarriage",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "IntermediateStops",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "OperatingDays",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Bookings");
        }
    }
}

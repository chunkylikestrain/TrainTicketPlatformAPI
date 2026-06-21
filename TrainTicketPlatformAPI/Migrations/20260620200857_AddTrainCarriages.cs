using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainCarriages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainCarriages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainId = table.Column<int>(type: "int", nullable: false),
                    Coach = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ClassType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LayoutType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    HasBikeSpace = table.Column<bool>(type: "bit", nullable: false),
                    HasAccessibleSpace = table.Column<bool>(type: "bit", nullable: false),
                    HasFamilyCompartment = table.Column<bool>(type: "bit", nullable: false),
                    HasDiningSection = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainCarriages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainCarriages_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainCarriages_TrainId_Coach",
                table: "TrainCarriages",
                columns: new[] { "TrainId", "Coach" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainCarriages");
        }
    }
}

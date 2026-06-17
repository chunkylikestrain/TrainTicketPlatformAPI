using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Stations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalityId",
                table: "Stations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateRegionId",
                table: "Stations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateRegions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Localities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateRegionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localities_StateRegions_StateRegionId",
                        column: x => x.StateRegionId,
                        principalTable: "StateRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_CountryId",
                table: "Stations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_LocalityId",
                table: "Stations",
                column: "LocalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StateRegionId",
                table: "Stations",
                column: "StateRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localities_StateRegionId_Name_Type",
                table: "Localities",
                columns: new[] { "StateRegionId", "Name", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_StateRegions_CountryId_Code",
                table: "StateRegions",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Countries_CountryId",
                table: "Stations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Localities_LocalityId",
                table: "Stations",
                column: "LocalityId",
                principalTable: "Localities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_StateRegions_StateRegionId",
                table: "Stations",
                column: "StateRegionId",
                principalTable: "StateRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Countries_CountryId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Localities_LocalityId",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_StateRegions_StateRegionId",
                table: "Stations");

            migrationBuilder.DropTable(
                name: "Localities");

            migrationBuilder.DropTable(
                name: "StateRegions");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Stations_CountryId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_LocalityId",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_StateRegionId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "LocalityId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "StateRegionId",
                table: "Stations");
        }
    }
}

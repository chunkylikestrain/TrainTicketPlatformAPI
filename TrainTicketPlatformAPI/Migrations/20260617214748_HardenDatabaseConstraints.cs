using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class HardenDatabaseConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Trips_TrainRouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Seats_TrainId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SeatId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Stations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Seats",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Coach",
                table: "Seats",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "BookingReference",
                table: "Bookings",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValueSql: "'BKG-' + CONVERT(varchar(36), NEWID())");

            migrationBuilder.AddColumn<string>(
                name: "BookingStatus",
                table: "Bookings",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "PendingPayment");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                computedColumnSql: "UPPER(LTRIM(RTRIM([Email])))",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedCode",
                table: "Stations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                computedColumnSql: "UPPER(LTRIM(RTRIM([Code])))",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Stations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                computedColumnSql: "UPPER(LTRIM(RTRIM([Name])))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TrainRouteId_DepartureTime",
                table: "Trips",
                columns: new[] { "TrainRouteId", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_NormalizedCode",
                table: "Stations",
                column: "NormalizedCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations",
                column: "NormalizedName");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stations_Code_NotBlank",
                table: "Stations",
                sql: "LEN(LTRIM(RTRIM([Code]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stations_Name_NotBlank",
                table: "Stations",
                sql: "LEN(LTRIM(RTRIM([Name]))) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_TrainId_Coach_Number",
                table: "Seats",
                columns: new[] { "TrainId", "Coach", "Number" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments",
                sql: "[Status] IN ('Successful', 'Failed', 'Refunded')");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SeatId_TravelDate",
                table: "Bookings",
                columns: new[] { "SeatId", "TravelDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripId_TravelDate",
                table: "Bookings",
                columns: new[] { "TripId", "TravelDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_BookingStatus",
                table: "Bookings",
                sql: "[BookingStatus] IN ('PendingPayment', 'Confirmed', 'Cancelled', 'Expired')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_PaymentStatus",
                table: "Bookings",
                sql: "[PaymentStatus] IN ('Pending', 'Successful', 'Failed', 'Refunded')");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Trips_TrainRouteId_DepartureTime",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Stations_NormalizedCode",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stations_Code_NotBlank",
                table: "Stations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stations_Name_NotBlank",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Seats_TrainId_Coach_Number",
                table: "Seats");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SeatId_TravelDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripId_TravelDate",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_BookingStatus",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_PaymentStatus",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedCode",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "BookingReference",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingStatus",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Stations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Seats",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Coach",
                table: "Seats",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldDefaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TrainRouteId",
                table: "Trips",
                column: "TrainRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_TrainId",
                table: "Seats",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SeatId",
                table: "Bookings",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Seats_SeatId",
                table: "Bookings",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

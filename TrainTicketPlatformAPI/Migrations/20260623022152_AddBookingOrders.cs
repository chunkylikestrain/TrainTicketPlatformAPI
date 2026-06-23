using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BookingOrderId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingOrderId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OrderReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "PendingPayment"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingOrders", x => x.Id);
                    table.CheckConstraint("CK_BookingOrders_BookingStatus", "[BookingStatus] IN ('PendingPayment', 'Confirmed', 'Cancelled', 'Expired', 'Refunded')");
                    table.CheckConstraint("CK_BookingOrders_PaymentStatus", "[PaymentStatus] IN ('Pending', 'Successful', 'Failed', 'Refunded')");
                    table.ForeignKey(
                        name: "FK_BookingOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingOrderId",
                table: "Payments",
                column: "BookingOrderId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Target",
                table: "Payments",
                sql: "([BookingId] IS NOT NULL AND [BookingOrderId] IS NULL) OR ([BookingId] IS NULL AND [BookingOrderId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingOrderId",
                table: "Bookings",
                column: "BookingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingOrders_GuestEmail",
                table: "BookingOrders",
                column: "GuestEmail");

            migrationBuilder.CreateIndex(
                name: "IX_BookingOrders_OrderReference",
                table: "BookingOrders",
                column: "OrderReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingOrders_UserId",
                table: "BookingOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BookingOrders_BookingOrderId",
                table: "Bookings",
                column: "BookingOrderId",
                principalTable: "BookingOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BookingOrders_BookingOrderId",
                table: "Payments",
                column: "BookingOrderId",
                principalTable: "BookingOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BookingOrders_BookingOrderId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BookingOrders_BookingOrderId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "BookingOrders");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingOrderId",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Target",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingOrderId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingOrderId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

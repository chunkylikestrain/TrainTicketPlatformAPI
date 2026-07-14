using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class HardenUserRolesAndStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [Users] SET [Role] = 'Admin' WHERE UPPER(LTRIM(RTRIM([Role]))) = 'ADMIN'");
            migrationBuilder.Sql(
                "UPDATE [Users] SET [Role] = 'Passenger' WHERE UPPER(LTRIM(RTRIM([Role]))) = 'PASSENGER'");
            migrationBuilder.Sql(
                "UPDATE [Users] SET [Status] = 'Active' WHERE UPPER(LTRIM(RTRIM([Status]))) = 'ACTIVE'");
            migrationBuilder.Sql(
                "UPDATE [Users] SET [Status] = 'Inactive' WHERE UPPER(LTRIM(RTRIM([Status]))) = 'INACTIVE'");
            migrationBuilder.Sql(
                "UPDATE [Users] SET [Status] = 'Suspended' WHERE UPPER(LTRIM(RTRIM([Status]))) = 'SUSPENDED'");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role",
                table: "Users",
                sql: "[Role] IN ('Admin', 'Passenger')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Status",
                table: "Users",
                sql: "[Status] IN ('Active', 'Inactive', 'Suspended')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Status",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);
        }
    }
}

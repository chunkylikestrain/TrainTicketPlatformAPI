using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenRailwayImportIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExternalImportedAtUtc",
                table: "Trips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExternalOperatingDate",
                table: "Trips",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalOrderId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalRawVersion",
                table: "Trips",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalScheduleId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSource",
                table: "Trips",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalTrainOrderId",
                table: "Trips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalCarrierCode",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalCommercialCategorySymbol",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalInternationalArrivalNumber",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalInternationalDepartureNumber",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalNationalNumber",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalSource",
                table: "Trains",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ArrivalDayOffset",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartureDayOffset",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalArrivalTrainNumber",
                table: "TrainRouteStops",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalDepartureTrainNumber",
                table: "TrainRouteStops",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalStationId",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalStopTypeId",
                table: "TrainRouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalStopTypeName",
                table: "TrainRouteStops",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExternalOperatingDate",
                table: "TrainRoutes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalOrderId",
                table: "TrainRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalScheduleId",
                table: "TrainRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSource",
                table: "TrainRoutes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalTrainOrderId",
                table: "TrainRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSource",
                table: "Stations",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalStationId",
                table: "Stations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ExternalSource_ExternalScheduleId_ExternalOrderId_ExternalOperatingDate",
                table: "Trips",
                columns: new[] { "ExternalSource", "ExternalScheduleId", "ExternalOrderId", "ExternalOperatingDate" },
                unique: true,
                filter: "[ExternalSource] <> '' AND [ExternalScheduleId] IS NOT NULL AND [ExternalOrderId] IS NOT NULL AND [ExternalOperatingDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ExternalSource_ExternalTrainOrderId",
                table: "Trips",
                columns: new[] { "ExternalSource", "ExternalTrainOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Trains_ExternalSource_ExternalCarrierCode_ExternalCommercialCategorySymbol_ExternalNationalNumber",
                table: "Trains",
                columns: new[] { "ExternalSource", "ExternalCarrierCode", "ExternalCommercialCategorySymbol", "ExternalNationalNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainRouteStops_ExternalStationId_TrainRouteId",
                table: "TrainRouteStops",
                columns: new[] { "ExternalStationId", "TrainRouteId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainRoutes_ExternalSource_ExternalScheduleId_ExternalOrderId_ExternalOperatingDate",
                table: "TrainRoutes",
                columns: new[] { "ExternalSource", "ExternalScheduleId", "ExternalOrderId", "ExternalOperatingDate" },
                unique: true,
                filter: "[ExternalSource] <> '' AND [ExternalScheduleId] IS NOT NULL AND [ExternalOrderId] IS NOT NULL AND [ExternalOperatingDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_ExternalSource_ExternalStationId",
                table: "Stations",
                columns: new[] { "ExternalSource", "ExternalStationId" },
                unique: true,
                filter: "[ExternalSource] <> '' AND [ExternalStationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trips_ExternalSource_ExternalScheduleId_ExternalOrderId_ExternalOperatingDate",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ExternalSource_ExternalTrainOrderId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trains_ExternalSource_ExternalCarrierCode_ExternalCommercialCategorySymbol_ExternalNationalNumber",
                table: "Trains");

            migrationBuilder.DropIndex(
                name: "IX_TrainRouteStops_ExternalStationId_TrainRouteId",
                table: "TrainRouteStops");

            migrationBuilder.DropIndex(
                name: "IX_TrainRoutes_ExternalSource_ExternalScheduleId_ExternalOrderId_ExternalOperatingDate",
                table: "TrainRoutes");

            migrationBuilder.DropIndex(
                name: "IX_Stations_ExternalSource_ExternalStationId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "ExternalImportedAtUtc",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalOperatingDate",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalOrderId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalRawVersion",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalScheduleId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalSource",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalTrainOrderId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ExternalCarrierCode",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ExternalCommercialCategorySymbol",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ExternalInternationalArrivalNumber",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ExternalInternationalDepartureNumber",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ExternalNationalNumber",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ExternalSource",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "ArrivalDayOffset",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "DepartureDayOffset",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalArrivalTrainNumber",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalDepartureTrainNumber",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalStationId",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalStopTypeId",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalStopTypeName",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "ExternalOperatingDate",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "ExternalOrderId",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "ExternalScheduleId",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "ExternalSource",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "ExternalTrainOrderId",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "ExternalSource",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "ExternalStationId",
                table: "Stations");
        }
    }
}

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

            migrationBuilder.Sql(
                """
                UPDATE route
                SET
                    RouteFingerprint = LEFT(CONCAT(
                        departureStation.Code,
                        CASE
                            WHEN stopCodes.Value IS NULL OR stopCodes.Value = '' THEN ''
                            ELSE CONCAT('>', stopCodes.Value)
                        END,
                        '>',
                        arrivalStation.Code), 1200),
                    AdminDisplayName = LEFT(CASE
                        WHEN keyStops.Value IS NULL OR keyStops.Value = ''
                            THEN CONCAT(departureStation.Name, ' to ', arrivalStation.Name)
                        ELSE CONCAT(departureStation.Name, ' to ', arrivalStation.Name, ' via ', keyStops.Value)
                    END, 300)
                FROM TrainRoutes route
                INNER JOIN Stations departureStation ON departureStation.Id = route.DepartureStationId
                INNER JOIN Stations arrivalStation ON arrivalStation.Id = route.ArrivalStationId
                OUTER APPLY
                (
                    SELECT STRING_AGG(CONVERT(nvarchar(max), orderedStops.Code), '>') WITHIN GROUP (ORDER BY orderedStops.StopOrder) AS Value
                    FROM
                    (
                        SELECT routeStop.StopOrder, stopStation.Code
                        FROM TrainRouteStops routeStop
                        INNER JOIN Stations stopStation ON stopStation.Id = routeStop.StationId
                        WHERE routeStop.TrainRouteId = route.Id
                    ) orderedStops
                ) stopCodes
                OUTER APPLY
                (
                    SELECT STRING_AGG(CONVERT(nvarchar(max), keyStopNames.Name), ', ') WITHIN GROUP (ORDER BY keyStopNames.StopOrder) AS Value
                    FROM
                    (
                        SELECT TOP (3) routeStop.StopOrder, stopStation.Name
                        FROM TrainRouteStops routeStop
                        INNER JOIN Stations stopStation ON stopStation.Id = routeStop.StationId
                        WHERE routeStop.TrainRouteId = route.Id
                        ORDER BY routeStop.StopOrder
                    ) keyStopNames
                ) keyStops
                WHERE route.RouteFingerprint = '' OR route.AdminDisplayName = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminDisplayName",
                table: "TrainRoutes");

            migrationBuilder.DropColumn(
                name: "RouteFingerprint",
                table: "TrainRoutes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class WidenStationNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Stations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Stations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                computedColumnSql: "UPPER(LTRIM(RTRIM([Name])))",
                stored: true);


            migrationBuilder.Sql(
                """
                ;WITH RankedStations AS
                (
                    SELECT
                        [Id],
                        MIN([Id]) OVER (PARTITION BY [NormalizedName]) AS [KeepId],
                        ROW_NUMBER() OVER (PARTITION BY [NormalizedName] ORDER BY [Id]) AS [StationRank]
                    FROM [Stations]
                    WHERE [NormalizedName] IS NOT NULL AND LTRIM(RTRIM([NormalizedName])) <> N''
                )
                SELECT [Id], [KeepId]
                INTO #DuplicateStations
                FROM RankedStations
                WHERE [StationRank] > 1;

                IF EXISTS (SELECT 1 FROM #DuplicateStations)
                BEGIN
                    DECLARE @StationIdColumnId int = COLUMNPROPERTY(OBJECT_ID(N'[dbo].[Stations]'), N'Id', 'ColumnId');
                    DECLARE @sql nvarchar(max) = N'';

                    SELECT @sql = @sql + N'
                UPDATE fk
                SET ' + QUOTENAME(c.[name]) + N' = d.[KeepId]
                FROM ' + QUOTENAME(OBJECT_SCHEMA_NAME(fkc.[parent_object_id])) + N'.' + QUOTENAME(OBJECT_NAME(fkc.[parent_object_id])) + N' AS fk
                INNER JOIN #DuplicateStations AS d ON fk.' + QUOTENAME(c.[name]) + N' = d.[Id];'
                    FROM sys.foreign_key_columns AS fkc
                    INNER JOIN sys.columns AS c
                        ON c.[object_id] = fkc.[parent_object_id]
                        AND c.[column_id] = fkc.[parent_column_id]
                    WHERE fkc.[referenced_object_id] = OBJECT_ID(N'[dbo].[Stations]')
                        AND fkc.[referenced_column_id] = @StationIdColumnId;

                    EXEC sp_executesql @sql;

                    DELETE s
                    FROM [Stations] AS s
                    INNER JOIN #DuplicateStations AS d ON s.[Id] = d.[Id];
                END

                DROP TABLE #DuplicateStations;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Stations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Stations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                computedColumnSql: "UPPER(LTRIM(RTRIM([Name])))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldComputedColumnSql: "UPPER(LTRIM(RTRIM([Name])))",
                oldStored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_NormalizedName",
                table: "Stations",
                column: "NormalizedName");
        }
    }
}

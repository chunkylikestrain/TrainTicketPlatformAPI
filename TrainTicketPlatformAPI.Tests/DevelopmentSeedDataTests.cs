using System.Collections;
using System.Reflection;
using System.Text.Json;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class DevelopmentSeedDataTests
    {
        [Test]
        public void ReferenceStations_HaveUniqueCodesAndExistingRegions()
        {
            var regions = GetSeedRecords("ReferenceRegions")
                .Select(seed => (CountryCode: GetString(seed, "CountryCode"), Code: GetString(seed, "Code")))
                .ToHashSet();
            var stations = GetSeedRecords("ReferenceStations").ToList();

            var duplicateCodes = stations
                .Select(seed => GetString(seed, "StationCode"))
                .GroupBy(code => code)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            var missingRegions = stations
                .Select(seed => (CountryCode: GetString(seed, "CountryCode"), RegionCode: GetString(seed, "RegionCode")))
                .Where(region => !regions.Contains((region.CountryCode, region.RegionCode)))
                .Distinct()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(duplicateCodes, Is.Empty);
                Assert.That(missingRegions, Is.Empty);
                Assert.That(stations, Has.Count.GreaterThanOrEqualTo(316));
            });
        }

        [Test]
        public void ReferenceStations_IncludeEveryEnrichedIcStationCandidate()
        {
            var seededCodes = GetSeedRecords("ReferenceStations")
                .Select(seed => GetString(seed, "StationCode"))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var snapshot = LoadEnrichedStationSnapshot();

            var missingCodes = snapshot.RootElement
                .GetProperty("stations")
                .EnumerateArray()
                .Select(station => station.GetProperty("code").GetString() ?? string.Empty)
                .Where(code => !seededCodes.Contains(code))
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(snapshot.RootElement.GetProperty("enrichedCount").GetInt32(), Is.EqualTo(207));
                Assert.That(snapshot.RootElement.GetProperty("stillNeedsReviewCount").GetInt32(), Is.EqualTo(0));
                Assert.That(missingCodes, Is.Empty);
            });
        }

        [Test]
        public void ReferenceStations_DoNotContainUnresolvedStationPlaceholders()
        {
            var unresolvedValues = GetSeedRecords("ReferenceStations")
                .SelectMany(seed => new[]
                {
                    GetString(seed, "CountryCode"),
                    GetString(seed, "RegionCode"),
                    GetString(seed, "LocalityName"),
                    GetString(seed, "LocalityType"),
                    GetString(seed, "StationCode"),
                    GetString(seed, "StationName"),
                    GetString(seed, "City")
                })
                .Where(value => value.Contains("TODO", StringComparison.OrdinalIgnoreCase) || value.Contains('?'))
                .ToList();

            Assert.That(unresolvedValues, Is.Empty);
        }

        [Test]
        public async Task SnapshotStationLookup_MatchesExistingReferenceStationByName()
        {
            await using var db = TestHelpers.GetInMemoryDb("SnapshotStationLookup_ByName");
            var existing = new Station
            {
                Code = "KRK",
                NormalizedCode = "KRK",
                Name = "Kraków Główny",
                NormalizedName = "KRAKÓW GŁÓWNY",
                City = "Kraków"
            };
            db.Stations.Add(existing);
            await db.SaveChangesAsync();

            var method = typeof(DevelopmentSeedData).GetMethod(
                "FindSnapshotStationAsync",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException(nameof(DevelopmentSeedData), "FindSnapshotStationAsync");

            var task = (Task<Station?>)method.Invoke(
                null,
                [
                    db,
                    "PLK",
                    80416,
                    "KRAKOW80416",
                    "Kraków Główny",
                    CancellationToken.None
                ])!;
            var station = await task;

            Assert.That(station, Is.Not.Null);
            Assert.That(station!.Id, Is.EqualTo(existing.Id));
        }

        [Test]
        public void CleanStationDisplays_KeepAccentedNamesForSqlServerCollation()
        {
            var displays = GetSeedRecords("CleanStationDisplays")
                .ToDictionary(seed => GetString(seed, "Code"), seed => GetString(seed, "Name"));

            Assert.Multiple(() =>
            {
                Assert.That(displays["POZ"], Is.EqualTo("Poznań Główny"));
                Assert.That(displays["KRK"], Is.EqualTo("Kraków Główny"));
                Assert.That(displays["WRO"], Is.EqualTo("Wrocław Główny"));
                Assert.That(displays["GDN"], Is.EqualTo("Gdańsk Główny"));
            });
        }

        private static IReadOnlyList<object> GetSeedRecords(string fieldName)
        {
            var field = typeof(DevelopmentSeedData).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException($"Could not find {fieldName}.");

            return ((IEnumerable)(field.GetValue(null)
                    ?? throw new InvalidOperationException($"{fieldName} is not initialized.")))
                .Cast<object>()
                .ToList();
        }

        private static string GetString(object seed, string propertyName)
        {
            return seed.GetType().GetProperty(propertyName)?.GetValue(seed)?.ToString() ?? string.Empty;
        }

        private static JsonDocument LoadEnrichedStationSnapshot()
        {
            var candidate = Path.Combine(
                FindSeedSnapshotDirectory(),
                "ic-station-candidates-locality-enriched-2026-07-03.json");

            if (File.Exists(candidate))
            {
                return JsonDocument.Parse(File.ReadAllText(candidate));
            }

            throw new FileNotFoundException("Could not find enriched station candidate snapshot.");
        }

        private static string FindSeedSnapshotDirectory()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "TrainTicketPlatformAPI",
                    "App_Data",
                    "SeedSnapshots");

                if (Directory.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find seed snapshot directory.");
        }
    }
}

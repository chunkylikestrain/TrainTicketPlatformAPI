using System.Collections;
using System.Reflection;
using System.Text.Json;
using TrainTicketPlatformAPI.Data;

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
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "TrainTicketPlatformAPI",
                    "App_Data",
                    "SeedSnapshots",
                    "ic-station-candidates-locality-enriched-2026-07-03.json");

                if (File.Exists(candidate))
                {
                    return JsonDocument.Parse(File.ReadAllText(candidate));
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find enriched station candidate snapshot.");
        }
    }
}

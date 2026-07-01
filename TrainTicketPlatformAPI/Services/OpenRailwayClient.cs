using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TrainTicketPlatformAPI.Contracts.OpenRailway;

namespace TrainTicketPlatformAPI.Services
{
    public class OpenRailwayClient : IOpenRailwayClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenRailwayOptions _options;

        public OpenRailwayClient(HttpClient httpClient, IOptions<OpenRailwayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<OpenRailwayDataVersionDto> GetDataVersionAsync(CancellationToken cancellationToken)
            => await GetRequiredJsonAsync<OpenRailwayDataVersionDto>("/api/v1/data-version", cancellationToken);

        public async Task<OpenRailwayStationsResponseDto> SearchStationsAsync(
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = new Dictionary<string, string?>
            {
                ["page"] = Math.Max(1, page).ToString(CultureInfo.InvariantCulture),
                ["pageSize"] = Math.Clamp(pageSize, 1, 10_000).ToString(CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrWhiteSpace(search))
                query["search"] = search.Trim();

            var path = QueryHelpers.AddQueryString("/api/v1/dictionaries/stations", query);
            return await GetRequiredJsonAsync<OpenRailwayStationsResponseDto>(path, cancellationToken);
        }

        public async Task<OpenRailwayRouteIdsResponseDto> GetRouteIdsAsync(
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var path = $"/api/v1/schedules/routes/{date:yyyy-MM-dd}";
            return await GetRequiredJsonAsync<OpenRailwayRouteIdsResponseDto>(path, cancellationToken);
        }

        public async Task<OpenRailwayRouteDto> GetRouteAsync(
            int scheduleId,
            int orderId,
            CancellationToken cancellationToken)
        {
            var path = $"/api/v1/schedules/route/{scheduleId.ToString(CultureInfo.InvariantCulture)}/{orderId.ToString(CultureInfo.InvariantCulture)}";
            return await GetRequiredJsonAsync<OpenRailwayRouteDto>(path, cancellationToken);
        }

        private async Task<T> GetRequiredJsonAsync<T>(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("OpenRailway:ApiKey is not configured.");

            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add("X-API-Key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = ExtractErrorMessage(body);
                throw new InvalidOperationException(
                    $"Open Railway API request failed with {(int)response.StatusCode} {response.ReasonPhrase}. {message}");
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new InvalidOperationException($"Open Railway API returned an empty {typeof(T).Name} payload.");
        }

        private static string ExtractErrorMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "The response body was empty.";

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("messageEn", out var messageEn) &&
                    messageEn.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(messageEn.GetString()))
                {
                    return messageEn.GetString()!;
                }

                if (root.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(message.GetString()))
                {
                    return message.GetString()!;
                }
            }
            catch (JsonException)
            {
            }

            return body.Length <= 500 ? body : body[..500];
        }
    }
}

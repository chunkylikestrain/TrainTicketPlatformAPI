using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Contracts.OpenRailway;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Security;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class AdminDangerousActionIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public async Task SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            await AuthorizeAsAdminAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task AdminDelete_RequiresDangerousActionHeader()
        {
            var response = await _client.DeleteAsync("/api/admin/users/999999");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task AdminCancelRefund_RequiresHeaderAndBodyConfirmation()
        {
            var missingHeader = await _client.PostAsJsonAsync(
                "/api/admin/bookings/999999/cancel-refund",
                new AdminCancelBookingRequest
                {
                    Reason = "Operational emergency",
                    ConfirmFullRefund = true,
                    ConfirmationText = "REFUND"
                });

            Assert.That(missingHeader.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/bookings/999999/cancel-refund")
            {
                Content = JsonContent.Create(new AdminCancelBookingRequest
                {
                    Reason = "Operational emergency",
                    ConfirmFullRefund = false,
                    ConfirmationText = ""
                })
            };
            request.Headers.Add(DangerousActionGuard.ConfirmationHeader, DangerousActionGuard.CancelRefund);

            var missingBodyConfirmation = await _client.SendAsync(request);

            Assert.That(missingBodyConfirmation.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task OpenRailwayApply_RequiresHeaderAndBodyConfirmation()
        {
            var missingHeader = await _client.PostAsJsonAsync(
                "/api/admin/open-railway/routes/2026-07-03/import",
                new OpenRailwayImportDateRequest
                {
                    DryRun = false,
                    ConfirmApply = true,
                    ConfirmationText = "IMPORT"
                });

            Assert.That(missingHeader.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/open-railway/routes/2026-07-03/import")
            {
                Content = JsonContent.Create(new OpenRailwayImportDateRequest
                {
                    DryRun = false,
                    ConfirmApply = false,
                    ConfirmationText = ""
                })
            };
            request.Headers.Add(DangerousActionGuard.ConfirmationHeader, DangerousActionGuard.Import);

            var missingBodyConfirmation = await _client.SendAsync(request);

            Assert.That(missingBodyConfirmation.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        private async Task AuthorizeAsAdminAsync()
        {
            var response = await _client.PostAsJsonAsync("/api/Auth/login", new LoginDto
            {
                Email = DevelopmentSeedData.AdminEmail,
                Password = DevelopmentSeedData.DefaultPassword
            });

            response.EnsureSuccessStatusCode();
            var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        }
    }
}

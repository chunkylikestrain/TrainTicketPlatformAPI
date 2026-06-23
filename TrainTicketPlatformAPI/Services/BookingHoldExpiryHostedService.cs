namespace TrainTicketPlatformAPI.Services
{
    public class BookingHoldExpiryHostedService : BackgroundService
    {
        private static readonly TimeSpan DefaultSweepInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan MinimumSweepInterval = TimeSpan.FromSeconds(15);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingHoldExpiryHostedService> _logger;
        private readonly TimeSpan _sweepInterval;

        public BookingHoldExpiryHostedService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<BookingHoldExpiryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var configuredSeconds = configuration.GetValue<int?>("BookingHolds:ExpirySweepIntervalSeconds");
            _sweepInterval = configuredSeconds is > 0
                ? TimeSpan.FromSeconds(configuredSeconds.Value)
                : DefaultSweepInterval;

            if (_sweepInterval < MinimumSweepInterval)
                _sweepInterval = MinimumSweepInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExpireStaleHoldsAsync(stoppingToken);

            using var timer = new PeriodicTimer(_sweepInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await ExpireStaleHoldsAsync(stoppingToken);
        }

        private async Task ExpireStaleHoldsAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var expiryService = scope.ServiceProvider.GetRequiredService<IBookingHoldExpiryService>();
                var expiredCount = await expiryService.ExpireStaleHoldsAsync(stoppingToken);

                if (expiredCount > 0)
                    _logger.LogInformation("Expired {BookingHoldCount} stale booking holds.", expiredCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire stale booking holds.");
            }
        }
    }
}

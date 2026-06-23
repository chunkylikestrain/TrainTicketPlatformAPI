namespace TrainTicketPlatformAPI.Services
{
    public interface IBookingHoldExpiryService
    {
        Task<int> ExpireStaleHoldsAsync(CancellationToken cancellationToken = default);
    }
}

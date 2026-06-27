using TrainTicketPlatformAPI.Contracts.Loyalty;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface ILoyaltyService
    {
        Task<LoyaltyAccountDto> GetAccountAsync(int userId);
        Task<IReadOnlyList<LoyaltyTransactionDto>> GetTransactionsAsync(int userId);
        Task<LoyaltyRedemptionQuote> CalculateRedemptionAsync(int? userId, decimal payableAmount, int requestedPoints);
        Task AwardTicketPurchaseAsync(Booking booking, DateTime paymentDateUtc);
        Task RedeemForBookingPaymentAsync(Booking booking, int points, decimal amount, DateTime paymentDateUtc);
        Task RedeemForOrderPaymentAsync(BookingOrder order, int points, decimal amount, DateTime paymentDateUtc);
    }
}

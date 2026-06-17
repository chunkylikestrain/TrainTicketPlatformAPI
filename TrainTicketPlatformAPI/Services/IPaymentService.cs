using TrainTicketPlatformAPI.Contracts.Payments;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreatePaymentIntentAsync(int bookingId);

        Task<Payment> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodToken);

        Task<Payment> ProcessPaymentAsync(int bookingId, decimal amount, string paymentMethodToken);

        Task<Payment> GetPaymentByIdAsync(int paymentId);

        Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(int bookingId);

        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    }
}

using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IPaymentService
    {

        /// <summary>
        /// Process a payment for a given booking.
        /// Returns the created Payment record.
        /// </summary>
        Task<Payment> ProcessPaymentAsync(int bookingId, decimal amount, string cardNumber);

        /// <summary>
        /// Get a single payment by its ID.
        /// Throws if not found.
        /// </summary>
        Task<Payment> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// Get all payments for a given booking.
        /// </summary>
        Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(int bookingId);

        /// <summary>
        /// Get all payments in the system (admin use).
        /// </summary>
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    }
}

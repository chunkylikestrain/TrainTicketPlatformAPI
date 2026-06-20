using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IBookingService
    {
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();

        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> UpdateGuestBookingDataAsync(
            int bookingId,
            string guestEmail,
            string passengerName,
            bool acceptedTerms);
        Task CancelBookingAsync(int bookingId);
        Task<Booking> AdminCancelAndRefundAsync(int bookingId, string reason);
        Task<Booking> RefundTicketAsync(string ticketNumber, string guestEmail);
        Task<Booking> UpdateBookingAsync(Booking booking);
        Task<Booking> ConfirmBookingAsync(int bookingId);
        Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId);
        Task<IEnumerable<Booking>> GetGuestTicketsByEmailAsync(string guestEmail);
        Task<bool> CheckSeatAvailabilityAsync(int trainId, int seatId, DateTime travelDate);
        Task<BookingReport> GenerateBookingReportAsync(DateTime from, DateTime to);

    }
}

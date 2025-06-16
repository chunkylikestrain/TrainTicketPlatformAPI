using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IBookingService
    {
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();

        Task<Booking> CreateBookingAsync(Booking booking);
        Task CancelBookingAsync(int bookingId);
        Task<Booking> UpdateBookingAsync(Booking booking);
        Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId);
        Task<bool> CheckSeatAvailabilityAsync(int trainId, int seatId, DateTime travelDate);
        Task<BookingReport> GenerateBookingReportAsync(DateTime from, DateTime to);

    }
}

using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface ISeatService
    {
        Task<IEnumerable<Seat>> GetAllSeatsAsync();
        Task<Seat> GetSeatByIdAsync(int seatId);
        Task<IEnumerable<Seat>> GetSeatsByTrainAsync(int trainId);
        Task<Seat> CreateSeatAsync(Seat seat);
        Task<Seat> UpdateSeatAsync(Seat seat);
        Task DeleteSeatAsync(int seatId);
    }
}

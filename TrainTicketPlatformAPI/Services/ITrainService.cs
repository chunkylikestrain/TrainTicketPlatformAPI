using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface ITrainService
    {
        Task<IEnumerable<Train>> SearchTrainsAsync(
            string departureStation,
            string arrivalStation,
            DateTime date);

        Task<IEnumerable<Train>> GetAllTrainsAsync();
        Task<Train> GetTrainByIdAsync(int trainId);
        Task<Train> CreateTrainAsync(Train train);
        Task<Train> UpdateTrainAsync(Train train);
        Task DeleteTrainAsync(int trainId);
    }
}


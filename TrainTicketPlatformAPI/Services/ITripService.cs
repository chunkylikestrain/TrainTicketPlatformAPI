using TrainTicketPlatformAPI.Contracts.Trips;

namespace TrainTicketPlatformAPI.Services
{
    public interface ITripService
    {
        Task<IEnumerable<TripSearchResultDto>> SearchTripsAsync(
            string from,
            string to,
            DateTime date);

        Task<TripDetailsDto> GetTripByIdAsync(int tripId);
        Task<IEnumerable<TripSeatAvailabilityDto>> GetSeatAvailabilityAsync(
            int tripId,
            int? segmentDepartureStationId = null,
            int? segmentArrivalStationId = null);
    }
}

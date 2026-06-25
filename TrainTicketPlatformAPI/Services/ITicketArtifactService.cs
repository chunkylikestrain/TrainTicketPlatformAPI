using TrainTicketPlatformAPI.Contracts.Tickets;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface ITicketArtifactService
    {
        Task<Booking> EnsureTicketIssuedAsync(int bookingId);
        Task<TicketArtifactDto> GetTicketAsync(int bookingId);
        Task<string> GetQrSvgAsync(int bookingId);
        Task<byte[]> GetTicketPdfAsync(int bookingId);
        Task<byte[]> GetOrderTicketPdfAsync(int bookingOrderId);
        Task<TicketEmailDeliveryDto> SendTicketEmailAsync(int bookingId, string? recipientEmail);
    }
}

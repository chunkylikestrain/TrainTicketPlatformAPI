using TrainTicketPlatformAPI.Contracts.Invoices;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> GenerateForBookingAsync(int bookingId, CreateInvoiceRequest request);
        Task<Invoice> GenerateForOrderAsync(int bookingOrderId, CreateInvoiceRequest request);
        Task<IReadOnlyList<Invoice>> GetInvoicesForUserAsync(int userId);
        Task<Invoice> GetInvoiceByIdAsync(int invoiceId);
        Task<byte[]> GetInvoicePdfAsync(int invoiceId);
    }
}

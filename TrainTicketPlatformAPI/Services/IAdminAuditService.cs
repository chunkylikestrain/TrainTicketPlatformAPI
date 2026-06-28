using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IAdminAuditService
    {
        Task RecordAsync(AdminAuditLog log, CancellationToken cancellationToken = default);
    }
}

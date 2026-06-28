using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class AdminAuditService : IAdminAuditService
    {
        private readonly TrainTicketDbContext _db;

        public AdminAuditService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task RecordAsync(AdminAuditLog log, CancellationToken cancellationToken = default)
        {
            log.CreatedAtUtc = log.CreatedAtUtc == default ? DateTime.UtcNow : log.CreatedAtUtc;
            _db.AdminAuditLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

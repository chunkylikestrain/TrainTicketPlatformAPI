using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;

namespace TrainTicketPlatformAPI.Tests
{
    public static class TestHelpers
    {
        public static TrainTicketDbContext GetInMemoryDb(string dbName)
        {
            var opts = new DbContextOptionsBuilder<TrainTicketDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new TrainTicketDbContext(opts);
        }
    }
}

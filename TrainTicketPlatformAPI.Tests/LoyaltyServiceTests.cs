using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class LoyaltyServiceTests
    {
        [Test]
        public async Task GetAccountAsync_CreatesEmptyAccount_ForExistingUser()
        {
            using var db = TestHelpers.GetInMemoryDb(nameof(GetAccountAsync_CreatesEmptyAccount_ForExistingUser));
            db.Users.Add(new User
            {
                Id = 1,
                Email = "passenger@example.com",
                PasswordHash = "hash",
                Phone = "555-0100",
                Role = "Passenger"
            });
            await db.SaveChangesAsync();

            var service = new LoyaltyService(db);

            var account = await service.GetAccountAsync(1);

            Assert.Multiple(() =>
            {
                Assert.That(account.UserId, Is.EqualTo(1));
                Assert.That(account.RedeemablePoints, Is.EqualTo(0));
                Assert.That(account.PendingPoints, Is.EqualTo(0));
                Assert.That(account.ExpiringPoints, Is.EqualTo(0));
                Assert.That(account.RedeemableValuePln, Is.EqualTo(0m));
                Assert.That(account.EarnRatePointsPerPln, Is.EqualTo(5m));
                Assert.That(account.RedeemRatePointsPerPln, Is.EqualTo(100));
            });

            Assert.That(db.LoyaltyAccounts.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetAccountAsync_RecalculatesTotals_FromLedgerTransactions()
        {
            using var db = TestHelpers.GetInMemoryDb(nameof(GetAccountAsync_RecalculatesTotals_FromLedgerTransactions));
            db.Users.Add(new User
            {
                Id = 2,
                Email = "loyal@example.com",
                PasswordHash = "hash",
                Phone = "555-0200",
                Role = "Passenger"
            });

            var account = new LoyaltyAccount
            {
                Id = 10,
                UserId = 2,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-3)
            };

            db.LoyaltyAccounts.Add(account);
            db.LoyaltyTransactions.AddRange(
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "TicketPurchase",
                    Status = "Available",
                    Points = 250,
                    SourceAmount = 50m,
                    Reference = "WH100",
                    TransactionDateUtc = DateTime.UtcNow.AddDays(-2),
                    ValidFromUtc = DateTime.UtcNow.AddDays(-2),
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(15)
                },
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "TicketPurchase",
                    Status = "Available",
                    Points = 175,
                    SourceAmount = 35m,
                    Reference = "WH101",
                    TransactionDateUtc = DateTime.UtcNow.AddDays(-1),
                    ValidFromUtc = DateTime.UtcNow.AddDays(-1),
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(60)
                },
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "TicketPurchase",
                    Status = "Pending",
                    Points = 100,
                    SourceAmount = 20m,
                    Reference = "WH102",
                    TransactionDateUtc = DateTime.UtcNow,
                    ValidFromUtc = DateTime.UtcNow.AddDays(1)
                },
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "Adjustment",
                    Status = "Cancelled",
                    Points = 500,
                    SourceAmount = 100m,
                    Reference = "ADJ",
                    TransactionDateUtc = DateTime.UtcNow,
                    ValidFromUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync();

            var service = new LoyaltyService(db);

            var summary = await service.GetAccountAsync(2);

            Assert.Multiple(() =>
            {
                Assert.That(summary.RedeemablePoints, Is.EqualTo(425));
                Assert.That(summary.PendingPoints, Is.EqualTo(100));
                Assert.That(summary.ExpiringPoints, Is.EqualTo(250));
                Assert.That(summary.RedeemableValuePln, Is.EqualTo(4.25m));
            });
        }

        [Test]
        public async Task GetTransactionsAsync_ReturnsNewestTransactionsFirst()
        {
            using var db = TestHelpers.GetInMemoryDb(nameof(GetTransactionsAsync_ReturnsNewestTransactionsFirst));
            db.Users.Add(new User
            {
                Id = 3,
                Email = "history@example.com",
                PasswordHash = "hash",
                Phone = "555-0300",
                Role = "Passenger"
            });

            var account = new LoyaltyAccount
            {
                Id = 20,
                UserId = 3,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-3)
            };

            db.LoyaltyAccounts.Add(account);
            db.LoyaltyTransactions.AddRange(
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "TicketPurchase",
                    Status = "Available",
                    Points = 50,
                    SourceAmount = 10m,
                    Reference = "WH-old",
                    TransactionDateUtc = DateTime.UtcNow.AddDays(-2),
                    ValidFromUtc = DateTime.UtcNow.AddDays(-2)
                },
                new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Type = "TicketPurchase",
                    Status = "Available",
                    Points = 150,
                    SourceAmount = 30m,
                    Reference = "WH-new",
                    TransactionDateUtc = DateTime.UtcNow.AddDays(-1),
                    ValidFromUtc = DateTime.UtcNow.AddDays(-1)
                });
            await db.SaveChangesAsync();

            var service = new LoyaltyService(db);

            var transactions = await service.GetTransactionsAsync(3);

            Assert.That(transactions, Has.Count.EqualTo(2));
            Assert.That(transactions[0].Reference, Is.EqualTo("WH-new"));
            Assert.That(transactions[1].Reference, Is.EqualTo("WH-old"));
        }
    }
}

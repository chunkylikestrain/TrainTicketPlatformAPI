using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Loyalty;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        public const decimal EarnRatePointsPerPln = 5m;
        public const int RedeemRatePointsPerPln = 100;

        private readonly TrainTicketDbContext _db;

        public LoyaltyService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<LoyaltyAccountDto> GetAccountAsync(int userId)
        {
            var account = await EnsureAccountAsync(userId);
            await RefreshAccountTotalsAsync(account);

            return ToAccountDto(account);
        }

        public async Task<IReadOnlyList<LoyaltyTransactionDto>> GetTransactionsAsync(int userId)
        {
            var account = await EnsureAccountAsync(userId);

            return await _db.LoyaltyTransactions
                .AsNoTracking()
                .Where(transaction => transaction.LoyaltyAccountId == account.Id)
                .OrderByDescending(transaction => transaction.TransactionDateUtc)
                .ThenByDescending(transaction => transaction.Id)
                .Select(transaction => new LoyaltyTransactionDto
                {
                    Id = transaction.Id,
                    Type = transaction.Type,
                    Status = transaction.Status,
                    Points = transaction.Points,
                    SourceAmount = transaction.SourceAmount,
                    Currency = transaction.Currency,
                    Reference = transaction.Reference,
                    Description = transaction.Description,
                    TransactionDateUtc = transaction.TransactionDateUtc,
                    ValidFromUtc = transaction.ValidFromUtc,
                    ExpiresAtUtc = transaction.ExpiresAtUtc,
                    BookingId = transaction.BookingId,
                    BookingOrderId = transaction.BookingOrderId
                })
                .ToListAsync();
        }

        public async Task<LoyaltyRedemptionQuote> CalculateRedemptionAsync(
            int? userId,
            decimal payableAmount,
            int requestedPoints)
        {
            if (requestedPoints < 0)
                throw new InvalidOperationException("Redeemed loyalty points cannot be negative");

            if (requestedPoints == 0 || payableAmount <= 0m)
                return new LoyaltyRedemptionQuote();

            if (!userId.HasValue)
                throw new InvalidOperationException("Log in to redeem loyalty points");

            var account = await EnsureAccountAsync(userId.Value, saveWhenCreated: false);
            await RefreshAccountTotalsAsync(account, saveChanges: false);

            if (requestedPoints > account.RedeemablePoints)
                throw new InvalidOperationException("Not enough loyalty points to redeem");

            var maxPointsForAmount = (int)Math.Floor(payableAmount * RedeemRatePointsPerPln);
            var points = Math.Min(requestedPoints, maxPointsForAmount);
            var amount = Math.Min(payableAmount, Math.Round(points / (decimal)RedeemRatePointsPerPln, 2));

            return new LoyaltyRedemptionQuote
            {
                Points = points,
                Amount = amount
            };
        }

        public async Task AwardTicketPurchaseAsync(Booking booking, DateTime paymentDateUtc)
        {
            if (!booking.UserId.HasValue)
                return;

            var sourceAmount = booking.Amount;
            if (sourceAmount <= 0m)
                return;

            var existingTransaction = await _db.LoyaltyTransactions.AnyAsync(transaction =>
                transaction.Type == "TicketPurchase" &&
                transaction.BookingId == booking.Id);

            if (existingTransaction)
                return;

            var account = await EnsureAccountAsync(booking.UserId.Value, saveWhenCreated: false);
            var points = CalculateEarnedPoints(sourceAmount);
            if (points <= 0)
                return;

            _db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                LoyaltyAccount = account,
                BookingId = booking.Id,
                BookingOrderId = booking.BookingOrderId,
                Type = "TicketPurchase",
                Status = "Available",
                Points = points,
                SourceAmount = sourceAmount,
                Currency = string.IsNullOrWhiteSpace(booking.Currency) ? "PLN" : booking.Currency,
                TransactionDateUtc = paymentDateUtc,
                ValidFromUtc = paymentDateUtc,
                ExpiresAtUtc = paymentDateUtc.AddYears(1),
                Reference = string.IsNullOrWhiteSpace(booking.TicketNumber)
                    ? booking.BookingReference
                    : booking.TicketNumber,
                Description = "Ticket purchase"
            });

            account.RedeemablePoints += points;
            account.ExpiringPoints += paymentDateUtc.AddYears(1) <= DateTime.UtcNow.AddDays(30)
                ? points
                : 0;
            account.RedeemableValuePln = Math.Round(account.RedeemablePoints / (decimal)RedeemRatePointsPerPln, 2);
            account.UpdatedAtUtc = paymentDateUtc;
        }

        public async Task RedeemForBookingPaymentAsync(
            Booking booking,
            int points,
            decimal amount,
            DateTime paymentDateUtc)
        {
            if (points <= 0 || amount <= 0m)
                return;

            if (!booking.UserId.HasValue)
                throw new InvalidOperationException("Log in to redeem loyalty points");

            var account = await EnsureAccountAsync(booking.UserId.Value, saveWhenCreated: false);
            await AddRedemptionTransactionAsync(
                account,
                points,
                amount,
                booking.Currency,
                paymentDateUtc,
                booking.Id,
                booking.BookingOrderId,
                string.IsNullOrWhiteSpace(booking.TicketNumber) ? booking.BookingReference : booking.TicketNumber);
        }

        public async Task RedeemForOrderPaymentAsync(
            BookingOrder order,
            int points,
            decimal amount,
            DateTime paymentDateUtc)
        {
            if (points <= 0 || amount <= 0m)
                return;

            if (!order.UserId.HasValue)
                throw new InvalidOperationException("Log in to redeem loyalty points");

            var account = await EnsureAccountAsync(order.UserId.Value, saveWhenCreated: false);
            await AddRedemptionTransactionAsync(
                account,
                points,
                amount,
                "PLN",
                paymentDateUtc,
                bookingId: null,
                order.Id,
                string.IsNullOrWhiteSpace(order.OrderReference) ? $"Order {order.Id}" : order.OrderReference);
        }

        private async Task<LoyaltyAccount> EnsureAccountAsync(int userId, bool saveWhenCreated = true)
        {
            var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var trackedAccount = _db.LoyaltyAccounts.Local
                .FirstOrDefault(item => item.UserId == userId);

            if (trackedAccount != null)
                return trackedAccount;

            var account = await _db.LoyaltyAccounts
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (account != null)
                return account;

            var now = DateTime.UtcNow;
            account = new LoyaltyAccount
            {
                UserId = userId,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.LoyaltyAccounts.Add(account);
            if (saveWhenCreated)
                await _db.SaveChangesAsync();

            return account;
        }

        private async Task AddRedemptionTransactionAsync(
            LoyaltyAccount account,
            int points,
            decimal amount,
            string currency,
            DateTime paymentDateUtc,
            int? bookingId,
            int? bookingOrderId,
            string reference)
        {
            await RefreshAccountTotalsAsync(account, saveChanges: false);

            if (points > account.RedeemablePoints)
                throw new InvalidOperationException("Not enough loyalty points to redeem");

            _db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                LoyaltyAccount = account,
                BookingId = bookingId,
                BookingOrderId = bookingOrderId,
                Type = "Redemption",
                Status = "Redeemed",
                Points = -points,
                SourceAmount = amount,
                Currency = string.IsNullOrWhiteSpace(currency) ? "PLN" : currency,
                TransactionDateUtc = paymentDateUtc,
                ValidFromUtc = paymentDateUtc,
                Reference = reference,
                Description = "Points redeemed for ticket payment"
            });

            account.RedeemablePoints -= points;
            account.RedeemableValuePln = Math.Round(account.RedeemablePoints / (decimal)RedeemRatePointsPerPln, 2);
            account.UpdatedAtUtc = paymentDateUtc;
        }

        private async Task RefreshAccountTotalsAsync(LoyaltyAccount account, bool saveChanges = true)
        {
            var now = DateTime.UtcNow;
            var transactions = await _db.LoyaltyTransactions
                .Where(transaction => transaction.LoyaltyAccountId == account.Id)
                .ToListAsync();

            account.PendingPoints = transactions
                .Where(transaction => transaction.Status == "Pending")
                .Sum(transaction => transaction.Points);

            account.RedeemablePoints = transactions
                .Where(transaction => transaction.Status is "Available" or "Redeemed")
                .Sum(transaction => transaction.Points);

            account.ExpiringPoints = transactions
                .Where(transaction =>
                    transaction.Status == "Available" &&
                    transaction.ExpiresAtUtc.HasValue &&
                    transaction.ExpiresAtUtc.Value <= now.AddDays(30))
                .Sum(transaction => transaction.Points);

            account.RedeemableValuePln = Math.Round(account.RedeemablePoints / (decimal)RedeemRatePointsPerPln, 2);
            account.UpdatedAtUtc = now;

            if (saveChanges)
                await _db.SaveChangesAsync();
        }

        private static int CalculateEarnedPoints(decimal sourceAmount)
            => (int)Math.Floor(sourceAmount * EarnRatePointsPerPln);

        private static LoyaltyAccountDto ToAccountDto(LoyaltyAccount account)
            => new()
            {
                UserId = account.UserId,
                RedeemablePoints = account.RedeemablePoints,
                PendingPoints = account.PendingPoints,
                ExpiringPoints = account.ExpiringPoints,
                RedeemableValuePln = account.RedeemableValuePln,
                EarnRatePointsPerPln = EarnRatePointsPerPln,
                RedeemRatePointsPerPln = RedeemRatePointsPerPln,
                UpdatedAtUtc = account.UpdatedAtUtc
            };
    }
}

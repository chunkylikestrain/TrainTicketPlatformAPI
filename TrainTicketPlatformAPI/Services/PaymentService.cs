using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly TrainTicketDbContext _db;
        public PaymentService(TrainTicketDbContext db) => _db = db;

        public async Task<Payment> ProcessPaymentAsync(int bookingId, decimal amount, string cardNumber)
        {

            // 1) Validate booking exists
            var booking = await _db.Bookings.FindAsync(bookingId)
                          ?? throw new KeyNotFoundException("Booking not found");

            // 2) Check card prefix: Visa (starts '4') or MasterCard (51-55)
           
            string prefix1 = cardNumber.Substring(0, 1);
            string prefix2 = cardNumber.Substring(0, 2);
            int prefix4 = int.TryParse(cardNumber.Substring(0, 4), out var p4) ? p4 : 0;

            // Visa: starts with “4”
            bool isVisa = prefix1 == "4";

            // MasterCard: old (51–55) or new (2221–2720)
            bool isMC = (int.TryParse(prefix2, out var p2) && p2 is >= 51 and <= 55)
                         || (prefix4 is >= 2221 and <= 2720);

            bool success = isVisa || isMC;
            string status = success ? "Successful" : "Failed";


            // 3) Record the payment
            var payment = new Payment
            {
                BookingId = bookingId,
                PaymentDate = DateTime.UtcNow,
                Amount = amount,
                Status = status
            };
            _db.Payments.Add(payment);

            // 4) Update the booking’s payment status
            booking.PaymentStatus = status;

            // 5) Persist both
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            var p = await _db.Payments.FindAsync(paymentId);
            if (p == null) throw new KeyNotFoundException("Payment not found");
            return p;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(int bookingId)
        {
            return await _db.Payments
                            .Where(p => p.BookingId == bookingId)
                            .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _db.Payments.ToListAsync();
        }
    }
}


using System;
using System.Linq;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrainTicketPlatformAPI.Data;

namespace TrainTicketApp
{
    public partial class SelectSeatForm : Form
    {
      
        private readonly IBookingService _bookingService;
        private readonly ISeatService _seatService;

        // these get set by the SearchTrainsForm:
        public int TrainId { get; set; }
        public DateTime TravelDate { get; set; }

        public SelectSeatForm(
            IBookingService bookingService,
            ISeatService seatService
        )
        {
            InitializeComponent();
            _bookingService = bookingService;
            _seatService = seatService;
            this.Load += SelectSeatForm_Load;
        }

        private async void SelectSeatForm_Load(object? sender, EventArgs e)
        {
            // load seats for that train
            var allSeats = await _seatService.GetSeatsByTrainAsync(TrainId);
            dgvSeats.DataSource = allSeats
              .Select(s => new { s.Id, s.Coach, s.Number, s.ClassType })
              .ToList();

            // show the date picker
            dtpTravelDate.Value = TravelDate;
            dtpTravelDate.Enabled = false;
        }

        private async void btnBook_Click(object sender, EventArgs e)
        {
            if (dgvSeats.CurrentRow == null) return;
            var seatId = (int)dgvSeats.CurrentRow.Cells["Id"].Value;

            try
            {
                var fare = await GetFareForSelectionAsync(seatId);

                // 1) create the booking
                var booking = await _bookingService.CreateBookingAsync(new Booking
                {
                    UserId = AppSession.CurrentUserId,
                    TrainId = TrainId,
                    TripId = fare.TripId,
                    SeatId = seatId,
                    TravelDate = TravelDate,
                    PaymentStatus = "Pending"
                });

                // 2) hand off to your PaymentForm
                var paymentForm = Program.AppHost!.Services
                                    .GetRequiredService<PaymentForm>();
                paymentForm.BookingId = booking.Id;
                paymentForm.Amount = fare.Price;

                paymentForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Booking Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async Task<Fare> GetFareForSelectionAsync(int seatId)
        {
            var seat = await _seatService.GetSeatByIdAsync(seatId);
            var db = Program.AppHost!.Services.GetRequiredService<TrainTicketDbContext>();
            var fare = await db.Fares
                .Include(f => f.Trip)
                .Where(f =>
                    f.Trip.TrainId == TrainId &&
                    f.Trip.DepartureTime.Date == TravelDate.Date)
                .OrderByDescending(f => f.ClassType == seat.ClassType)
                .ThenBy(f => f.Price)
                .FirstOrDefaultAsync();

            return fare ?? throw new InvalidOperationException(
                "No fare is configured for the selected train, date, and seat class");
        }
    }
}




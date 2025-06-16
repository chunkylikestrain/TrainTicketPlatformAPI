using System;
using System.Linq;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;
using Microsoft.Extensions.DependencyInjection;

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
            
            _bookingService = bookingService;
            _seatService = seatService;
            this.Load += SelectSeatForm_Load;
        }

        private async void SelectSeatForm_Load(object sender, EventArgs e)
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
                // 1) create the booking
                var booking = await _bookingService.CreateBookingAsync(new Booking
                {
                    UserId = AppSession.CurrentUserId,
                    TrainId = TrainId,
                    SeatId = seatId,
                    TravelDate = TravelDate,
                    PaymentStatus = "Pending"
                });

                // 2) hand off to your PaymentForm
                var paymentForm = Program.AppHost.Services
                                    .GetRequiredService<PaymentForm>();
                paymentForm.BookingId = booking.Id;
                paymentForm.Amount = /* you can pull the fare from your train service */
                    /* e.g. */ await Program.AppHost.Services
                        .GetRequiredService<ITrainService>()
                        .GetTrainByIdAsync(TrainId)
                        .ContinueWith(t => t.Result.Price);

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
    }
}




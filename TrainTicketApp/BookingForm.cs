using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class BookingForm : Form
    {
        private readonly IBookingService _bookingService;
        private readonly ITrainService _trainService;
        private readonly ISeatService _seatService;

        // these will be set for you before showing the form:
        public int TrainId { get; set; }
        public int SeatId { get; set; }
        public DateTime TravelDate { get; set; }

        // private holders for display & later use:
        private Train _train;
        private Seat _seat;

        public BookingForm(
            IBookingService bookingService,
            ITrainService trainService,
            ISeatService seatService
        )
        {
            _bookingService = bookingService;
            _trainService = trainService;
            _seatService = seatService;

            InitializeComponent();
            this.Load += BookingForm_Load;
            btnConfirm.Click += btnConfirm_Click;
        }

        private async void BookingForm_Load(object sender, EventArgs e)
        {
            // 1) fetch the train & seat so we can show details
            _train = await _trainService.GetTrainByIdAsync(TrainId);
            _seat = await _seatService.GetSeatByIdAsync(SeatId);

            // 2) populate your labels
            lblTrain.Text = $"{_train.Name} ({_train.DepartureStation} → {_train.ArrivalStation})";
            lblSeat.Text = $"{_seat.Coach}-{_seat.Number} ({_seat.ClassType})";
            lblTravelDate.Text = TravelDate.ToShortDateString();
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) build and send your Booking
                var booking = await _bookingService.CreateBookingAsync(new Booking
                {
                    UserId = AppSession.CurrentUserId,
                    TrainId = TrainId,
                    SeatId = SeatId,
                    TravelDate = TravelDate,
                    PaymentStatus = "Pending"
                });

                // 2) go to PaymentForm
                var payForm = Program
                    .AppHost
                    .Services
                    .GetRequiredService<PaymentForm>();

                payForm.BookingId = booking.Id;
                payForm.Amount = _train.Price;      // use the fetched train fare
                payForm.TrainName = _train.Name;       // if you want to display it there
                payForm.SeatInfo = $"{_seat.Coach}-{_seat.Number}";

                payForm.Show();
                this.Close();
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


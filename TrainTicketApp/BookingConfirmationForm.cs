using System;
using System.Windows.Forms;

namespace TrainTicketApp
{
    public partial class BookingConfirmationForm : Form
    {
        // — set these before calling Show()
        public int BookingId { get; set; }
        public string TrainName { get; set; } = "";
        public string SeatNumber { get; set; } = "";
        public DateTime TravelDate { get; set; }
        public decimal Amount { get; set; }

        public BookingConfirmationForm()
        {
            InitializeComponent();
        }

        private void BookingConfirmationForm_Load(object sender, EventArgs e)
        {
            // populate the labels
            lblBookingId.Text = BookingId.ToString();
            lblTrainName.Text = TrainName;
            lblSeatNumber.Text = SeatNumber;
            lblTravelDate.Text = TravelDate.ToString("yyyy-MM-dd");
            lblAmount.Text = Amount.ToString("C");
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}


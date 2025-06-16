using System;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Services;    // for IPaymentService
// no need for a “Dtos” namespace if your current interface takes 3 primitives

namespace TrainTicketApp
{
    public partial class PaymentForm : Form
    {
        private readonly IPaymentService _paymentService;

        // these are set by whoever opens this form
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string TrainName { get; set; } = "";
        public string SeatInfo { get; set; } = "";
        public DateTime TravelDate { get; set; }

        public PaymentForm(IPaymentService paymentService)
        {
            InitializeComponent();
            _paymentService = paymentService;
            this.Load += PaymentForm_Load;
        }

        private void PaymentForm_Load(object sender, EventArgs e)
        {
            // copy the incoming values into your controls
            txtBookingId.Text = BookingId.ToString();
            txtAmount.Text = Amount.ToString("C");        // currency
            lblTrainName.Text = $"Train: {TrainName}";
            lblSeatInfo.Text = $"Seat: {SeatInfo}";
            lblTravelDate.Text = $"Date: {TravelDate:d}";
        }

        private async void btnPay_Click(object sender, EventArgs e)
        {
            try
            {
                // note: match your IPaymentService signature exactly
                var payment = await _paymentService.ProcessPaymentAsync(
                    BookingId,
                    Amount,
                    txtCardNumber.Text.Trim()
                );

                MessageBox.Show(
                    $"Payment {payment.Status}\nTransaction ID: {payment.Id}",
                    "Payment Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Payment Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}







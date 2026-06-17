using System;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class PaymentForm : Form
    {
        private readonly IPaymentService _paymentService;

        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string TrainName { get; set; } = "";
        public string SeatInfo { get; set; } = "";
        public DateTime TravelDate { get; set; }

        public PaymentForm(IPaymentService paymentService)
        {
            InitializeComponent();
            _paymentService = paymentService;
            Load += PaymentForm_Load;
        }

        private void PaymentForm_Load(object? sender, EventArgs e)
        {
            txtBookingId.Text = BookingId.ToString();
            txtAmount.Text = Amount.ToString("C");
            txtPaymentToken.Text = PaymentService.SuccessToken;
            lblTrainName.Text = $"Train: {TrainName}";
            lblSeatInfo.Text = $"Seat: {SeatInfo}";
            lblTravelDate.Text = $"Date: {TravelDate:d}";
        }

        private async void btnPay_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = await _paymentService.CreatePaymentIntentAsync(BookingId);
                var payment = await _paymentService.ConfirmPaymentAsync(
                    intent.PaymentIntentId,
                    txtPaymentToken.Text.Trim());

                MessageBox.Show(
                    $"Payment {payment.Status}\nTransaction ID: {payment.Id}\nIntent: {payment.PaymentIntentId}",
                    "Payment Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Payment Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

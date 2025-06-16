using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class ViewBookingReportForm : Form
    {
        private readonly IBookingService _bookingService;

        public ViewBookingReportForm(IBookingService bookingService)
        {
            InitializeComponent();
            _bookingService = bookingService;
        }

        private void ViewBookingReportForm_Load(object sender, EventArgs e)
        {
            // default to last 7 days
            dtpFrom.Value = DateTime.Today.AddDays(-7);
            dtpTo.Value = DateTime.Today;
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            // disable while loading
            btnLoad.Enabled = false;
            try
            {
                var from = dtpFrom.Value.Date;
                var to = dtpTo.Value.Date.AddDays(1).AddTicks(-1); // include entire "To" day

                var report = await _bookingService.GenerateBookingReportAsync(from, to);

                lblTotalBookings.Text = report.TotalBookings.ToString();
                lblTotalRevenue.Text = report.TotalRevenue.ToString("C");
                lblTotalCancellations.Text = report.TotalCancellations.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading report",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoad.Enabled = true;
            }
        }
    }
}


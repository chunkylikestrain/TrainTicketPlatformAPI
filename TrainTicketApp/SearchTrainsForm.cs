using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;
using Microsoft.Extensions.DependencyInjection;


namespace TrainTicketApp
{
    public partial class SearchTrainsForm : Form

    {

        public DateTime TravelDateFilter { get; set; }
        private readonly ITrainService _trainService;
        private readonly IBookingService _bookingService;

        // Designer ctor
        public SearchTrainsForm()
        {
            InitializeComponent();
        }


        // DI ctor
        public SearchTrainsForm(ITrainService trainService,
                                IBookingService bookingService)
            : this()
        {
            _trainService = trainService
                              ?? throw new ArgumentNullException(nameof(trainService));
            _bookingService = bookingService
                              ?? throw new ArgumentNullException(nameof(bookingService));
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // 1) read filter inputs
            var departure = txtDeparture.Text.Trim();
            var arrival = txtArrival.Text.Trim();
            var date = dtpTravelDate.Value.Date;
            TravelDateFilter = date;

            if (string.IsNullOrEmpty(departure) || string.IsNullOrEmpty(arrival))
            {
                MessageBox.Show("Please enter both departure and arrival stations.",
                                "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 2) fetch and filter
                var all = await _trainService.GetAllTrainsAsync();
                var matches = all
                  .Where(t => t.DepartureStation.Equals(departure, StringComparison.OrdinalIgnoreCase)
                           && t.ArrivalStation.Equals(arrival, StringComparison.OrdinalIgnoreCase)
                           && t.DepartureTime.Date == date)
                  .ToList();

                // 3) bind to grid
                dgvTrains.DataSource = matches;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void dgvTrains_CellDoubleClick(object sender,
                                                DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;  // header click
            var selected = dgvTrains.Rows[e.RowIndex].DataBoundItem as Train;
            if (selected == null) return;

            // 4) create a booking & go to seat selection
            var select = Program.AppHost.Services
                         .GetRequiredService<SelectSeatForm>();
            select.TrainId = selected.Id;
            select.TravelDate = this.TravelDateFilter;   // ← pass it along
            select.Show();
            this.Hide();
        }

    }
}

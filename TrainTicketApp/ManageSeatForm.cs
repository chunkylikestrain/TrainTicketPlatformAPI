using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class ManageSeatMapForm : Form
    {
        private readonly ISeatService _seatService;

        public ManageSeatMapForm(ISeatService seatService)
        {
            InitializeComponent();
            _seatService = seatService;
        }

        private async void ManageSeatMapForm_Load(object sender, EventArgs e)
        {
            await RefreshGridAsync();
        }

        private async Task RefreshGridAsync()
        {
            var list = await _seatService.GetAllSeatsAsync();
            dgvSeatMaps.DataSource = list
                .Select(s => new {
                    s.Id,
                    s.TrainId,
                    s.Coach,
                    s.Number,
                    s.ClassType,
                    s.IsAvailable
                })
                .ToList();
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var upsert = Program
                .AppHost
                .Services
                .GetRequiredService<UpsertSeatMapForm>();

            // no existing seat → new
            upsert.SeatToEdit = null;
            upsert.FormClosed += async (s, _) => await RefreshGridAsync();
            upsert.Show();
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSeatMaps.CurrentRow == null) return;
            var row = dgvSeatMaps.CurrentRow.DataBoundItem;
            var id = (int)row.GetType().GetProperty("Id")!.GetValue(row)!;

            var seat = await _seatService.GetSeatByIdAsync(id);
            if (seat == null) return;

            var upsert = Program
                .AppHost
                .Services
                .GetRequiredService<UpsertSeatMapForm>();

            upsert.SeatToEdit = seat;
            upsert.FormClosed += async (s, _) => await RefreshGridAsync();
            upsert.Show();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSeatMaps.CurrentRow == null) return;
            var row = dgvSeatMaps.CurrentRow.DataBoundItem;
            var id = (int)row.GetType().GetProperty("Id")!.GetValue(row)!;

            if (MessageBox.Show("Really delete this seat?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            await _seatService.DeleteSeatAsync(id);
            await RefreshGridAsync();
        }
    }
}


using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class ManageTrainForm : Form
    {
        private readonly ITrainService _trainService;

        public ManageTrainForm(ITrainService trainService)
        {
            InitializeComponent();
            _trainService = trainService;
        }

        private async void ManageTrainForm_Load(object sender, EventArgs e)
        {
            await RefreshGridAsync();
        }

        private async Task RefreshGridAsync()
        {
            var trains = await _trainService.GetAllTrainsAsync();
            dgvTrains.DataSource = trains.ToList();
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var upsert = Program
                .AppHost
                .Services
                .GetRequiredService<UpsertTrainForm>();

            upsert.TrainToEdit = null;
            upsert.FormClosed += async (s, args) => await RefreshGridAsync();
            upsert.Show();
        }

        private async void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvTrains.CurrentRow == null) return;

            var selected = dgvTrains
                           .CurrentRow
                           .DataBoundItem as Train;
            if (selected == null) return;

            var upsert = Program
                .AppHost
                .Services
                .GetRequiredService<UpsertTrainForm>();

            upsert.TrainToEdit = selected;
            upsert.FormClosed += async (s, args) => await RefreshGridAsync();
            upsert.Show();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTrains.CurrentRow == null) return;
            var selected = dgvTrains.CurrentRow.DataBoundItem as Train;
            if (selected == null) return;

            await _trainService.DeleteTrainAsync(selected.Id);
            await RefreshGridAsync();
        }
    }
}



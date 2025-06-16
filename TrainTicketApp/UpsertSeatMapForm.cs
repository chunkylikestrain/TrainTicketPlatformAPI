using System;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketApp
{
    public partial class UpsertSeatMapForm : Form
    {
        private readonly ISeatService _seatService;
        public Seat? SeatToEdit { get; set; }

        public UpsertSeatMapForm(ISeatService seatService)
        {
            InitializeComponent();
            _seatService = seatService;
        }

        private void UpsertSeatMapForm_Load(object sender, EventArgs e)
        {
            // If editing an existing seat, prefill controls:
            if (SeatToEdit != null)
            {
                txtId.Text = SeatToEdit.Id.ToString();
                txtTrainId.Text = SeatToEdit.TrainId.ToString();
                txtCoach.Text = SeatToEdit.Coach;
                txtNumber.Text = SeatToEdit.Number;
                txtClassType.Text = SeatToEdit.ClassType;
                chkAvailable.Checked = SeatToEdit.IsAvailable;
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            var s = SeatToEdit ?? new Seat();
            s.TrainId = int.Parse(txtTrainId.Text);
            s.Coach = txtCoach.Text.Trim();
            s.Number = txtNumber.Text.Trim();
            s.ClassType = txtClassType.Text.Trim();
            s.IsAvailable = chkAvailable.Checked;

            if (SeatToEdit == null)
                await _seatService.CreateSeatAsync(s);
            else
                await _seatService.UpdateSeatAsync(s);

            this.Close();
        }
    }
}


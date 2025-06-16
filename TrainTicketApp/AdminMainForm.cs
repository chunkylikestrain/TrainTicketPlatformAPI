using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace TrainTicketApp
{
    public partial class AdminMainForm : Form
    {
        private readonly IServiceProvider _svc;

        public AdminMainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _svc = serviceProvider;
        }

        private void btnManageTrains_Click(object sender, EventArgs e)
        {
            // open your train‐management screen
            var frm = _svc.GetRequiredService<ManageTrainForm>();
            frm.Show();
        }

        private void btnManageSeatMaps_Click(object sender, EventArgs e)
        {
            // open your seat‐map editor
            var frm = _svc.GetRequiredService<ManageSeatMapForm>();
            frm.Show();
        }

        private void btnViewReports_Click(object sender, EventArgs e)
        {
            // open the booking report viewer
            var frm = _svc.GetRequiredService<ViewBookingReportForm>();
            frm.Show();
        }
    }
}


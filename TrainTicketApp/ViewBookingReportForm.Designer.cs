namespace TrainTicketApp
{
    partial class ViewBookingReportForm
    {
        private System.ComponentModel.IContainer components = null;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Label lblFrom;
        private Label lblTo;
        private Button btnLoad;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label lblTotalBookings;
        private Label lblTotalRevenue;
        private Label lblTotalCancellations;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dtpFrom = new DateTimePicker();
            this.dtpTo = new DateTimePicker();
            this.lblFrom = new Label();
            this.lblTo = new Label();
            this.btnLoad = new Button();
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.lblTotalBookings = new Label();
            this.lblTotalRevenue = new Label();
            this.lblTotalCancellations = new Label();

            this.SuspendLayout();

            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(12, 15);
            this.lblFrom.Text = "From:";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Format = DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(60, 12);
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(200, 15);
            this.lblTo.Text = "To:";
            // 
            // dtpTo
            // 
            this.dtpTo.Format = DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(235, 12);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(380, 10);
            this.btnLoad.Size = new System.Drawing.Size(75, 25);
            this.btnLoad.Text = "Load";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // label1 (Total Bookings)
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Text = "Total Bookings:";
            // 
            // lblTotalBookings
            // 
            this.lblTotalBookings.AutoSize = true;
            this.lblTotalBookings.Location = new System.Drawing.Point(130, 60);
            this.lblTotalBookings.Text = "—";
            // 
            // label2 (Revenue)
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 90);
            this.label2.Text = "Total Revenue:";
            // 
            // lblTotalRevenue
            // 
            this.lblTotalRevenue.AutoSize = true;
            this.lblTotalRevenue.Location = new System.Drawing.Point(130, 90);
            this.lblTotalRevenue.Text = "—";
            // 
            // label3 (Cancellations)
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 120);
            this.label3.Text = "Total Cancellations:";
            // 
            // lblTotalCancellations
            // 
            this.lblTotalCancellations.AutoSize = true;
            this.lblTotalCancellations.Location = new System.Drawing.Point(150, 120);
            this.lblTotalCancellations.Text = "—";
            // 
            // ViewBookingReportForm
            // 
            this.ClientSize = new System.Drawing.Size(480, 160);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.dtpFrom);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.dtpTo);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTotalBookings);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblTotalRevenue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTotalCancellations);
            this.Text = "Booking Report";
            this.Load += new System.EventHandler(this.ViewBookingReportForm_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

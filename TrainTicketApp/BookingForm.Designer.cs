namespace TrainTicketApp
{
    partial class BookingForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTrain;
        private Label lblSeat;
        private Label lblTravelDate;
        private Button btnConfirm;

        private void InitializeComponent()
        {
            this.lblTrain = new System.Windows.Forms.Label();
            this.lblSeat = new System.Windows.Forms.Label();
            this.lblTravelDate = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTrain
            // 
            this.lblTrain.AutoSize = true;
            this.lblTrain.Location = new System.Drawing.Point(20, 20);
            this.lblTrain.Name = "lblTrain";
            this.lblTrain.Size = new System.Drawing.Size(46, 17);
            this.lblTrain.Text = "Train:";
            // 
            // lblSeat
            // 
            this.lblSeat.AutoSize = true;
            this.lblSeat.Location = new System.Drawing.Point(20, 60);
            this.lblSeat.Name = "lblSeat";
            this.lblSeat.Size = new System.Drawing.Size(38, 17);
            this.lblSeat.Text = "Seat:";
            // 
            // lblTravelDate
            // 
            this.lblTravelDate.AutoSize = true;
            this.lblTravelDate.Location = new System.Drawing.Point(20, 100);
            this.lblTravelDate.Name = "lblTravelDate";
            this.lblTravelDate.Size = new System.Drawing.Size(80, 17);
            this.lblTravelDate.Text = "Travel Date:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(140, 140);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(120, 30);
            this.btnConfirm.Text = "Confirm Booking";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // BookingForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.lblTrain);
            this.Controls.Add(this.lblSeat);
            this.Controls.Add(this.lblTravelDate);
            this.Controls.Add(this.btnConfirm);
            this.Name = "BookingForm";
            this.Text = "Confirm Booking";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}

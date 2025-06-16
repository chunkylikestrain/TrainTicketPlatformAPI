namespace TrainTicketApp
{
    partial class BookingConfirmationForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblBookingId, lblTrainName, lblSeatNumber, lblTravelDate, lblAmount;
        private Button btnOk;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblBookingId = new Label();
            this.lblTrainName = new Label();
            this.lblSeatNumber = new Label();
            this.lblTravelDate = new Label();
            this.lblAmount = new Label();
            this.btnOk = new Button();
            this.SuspendLayout();
            // 
            // lblBookingId
            // 
            this.lblBookingId.AutoSize = true;
            this.lblBookingId.Location = new System.Drawing.Point(30, 30);
            this.lblBookingId.Name = "lblBookingId";
            this.lblBookingId.Size = new System.Drawing.Size(85, 20);
            this.lblBookingId.Text = "Booking ID: ";
            // 
            // lblTrainName
            // 
            this.lblTrainName.AutoSize = true;
            this.lblTrainName.Location = new System.Drawing.Point(30, 70);
            this.lblTrainName.Name = "lblTrainName";
            this.lblTrainName.Size = new System.Drawing.Size(44, 20);
            this.lblTrainName.Text = "Train: ";
            // 
            // lblSeatNumber
            // 
            this.lblSeatNumber.AutoSize = true;
            this.lblSeatNumber.Location = new System.Drawing.Point(30, 110);
            this.lblSeatNumber.Name = "lblSeatNumber";
            this.lblSeatNumber.Size = new System.Drawing.Size(48, 20);
            this.lblSeatNumber.Text = "Seat: ";
            // 
            // lblTravelDate
            // 
            this.lblTravelDate.AutoSize = true;
            this.lblTravelDate.Location = new System.Drawing.Point(30, 150);
            this.lblTravelDate.Name = "lblTravelDate";
            this.lblTravelDate.Size = new System.Drawing.Size(87, 20);
            this.lblTravelDate.Text = "Travel Date:";
            // 
            // lblAmount
            // 
            this.lblAmount.AutoSize = true;
            this.lblAmount.Location = new System.Drawing.Point(30, 190);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Size = new System.Drawing.Size(68, 20);
            this.lblAmount.Text = "Amount: ";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(120, 230);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(94, 29);
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // BookingConfirmationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 280);
            this.Controls.Add(this.lblBookingId);
            this.Controls.Add(this.lblTrainName);
            this.Controls.Add(this.lblSeatNumber);
            this.Controls.Add(this.lblTravelDate);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.btnOk);
            this.Name = "BookingConfirmationForm";
            this.Text = "Booking Confirmed";
            this.Load += new System.EventHandler(this.BookingConfirmationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

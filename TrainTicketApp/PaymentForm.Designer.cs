namespace TrainTicketApp
{
    partial class PaymentForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label labelBookingId;
        private System.Windows.Forms.TextBox txtBookingId;
        private System.Windows.Forms.Label labelAmount;
        private System.Windows.Forms.TextBox txtAmount;
        private System.Windows.Forms.Label labelCard;
        private System.Windows.Forms.TextBox txtCardNumber;
        private System.Windows.Forms.Button btnPay;

        // These three are your “extra” labels:
        private System.Windows.Forms.Label lblTrainName;
        private System.Windows.Forms.Label lblSeatInfo;
        private System.Windows.Forms.Label lblTravelDate;

        /// <summary> 
        /// Clean up any resources being used. 
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.labelBookingId = new Label();
            this.txtBookingId = new TextBox();
            this.labelAmount = new Label();
            this.txtAmount = new TextBox();
            this.labelCard = new Label();
            this.txtCardNumber = new TextBox();
            this.btnPay = new Button();

            this.lblTrainName = new Label();
            this.lblSeatInfo = new Label();
            this.lblTravelDate = new Label();

            this.SuspendLayout();

            // labelBookingId
            this.labelBookingId.AutoSize = true;
            this.labelBookingId.Location = new System.Drawing.Point(12, 15);
            this.labelBookingId.Name = "labelBookingId";
            this.labelBookingId.Size = new System.Drawing.Size(77, 15);
            this.labelBookingId.Text = "Booking ID";

            // txtBookingId
            this.txtBookingId.Location = new System.Drawing.Point(100, 12);
            this.txtBookingId.Name = "txtBookingId";
            this.txtBookingId.ReadOnly = true;
            this.txtBookingId.Size = new System.Drawing.Size(150, 23);

            // labelAmount
            this.labelAmount.AutoSize = true;
            this.labelAmount.Location = new System.Drawing.Point(12, 50);
            this.labelAmount.Name = "labelAmount";
            this.labelAmount.Size = new System.Drawing.Size(54, 15);
            this.labelAmount.Text = "Amount";

            // txtAmount
            this.txtAmount.Location = new System.Drawing.Point(100, 42);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.ReadOnly = true;
            this.txtAmount.Size = new System.Drawing.Size(150, 23);

            // labelCard
            this.labelCard.AutoSize = true;
            this.labelCard.Location = new System.Drawing.Point(12, 75);
            this.labelCard.Name = "labelCard";
            this.labelCard.Size = new System.Drawing.Size(79, 15);
            this.labelCard.Text = "Card Number";

            // txtCardNumber
            this.txtCardNumber.Location = new System.Drawing.Point(100, 72);
            this.txtCardNumber.Name = "txtCardNumber";
            this.txtCardNumber.Size = new System.Drawing.Size(150, 23);

            // lblTrainName
            this.lblTrainName.AutoSize = true;
            this.lblTrainName.Location = new System.Drawing.Point(12, 110);
            this.lblTrainName.Name = "lblTrainName";
            this.lblTrainName.Size = new System.Drawing.Size(200, 23);
            this.lblTrainName.Text = "Train: —";

            // lblSeatInfo
            this.lblSeatInfo.AutoSize = true;
            this.lblSeatInfo.Location = new System.Drawing.Point(12, 135);
            this.lblSeatInfo.Name = "lblSeatInfo";
            this.lblSeatInfo.Text = "Seat: —";

            // lblTravelDate
            this.lblTravelDate.AutoSize = true;
            this.lblTravelDate.Location = new System.Drawing.Point(12, 160);
            this.lblTravelDate.Name = "lblTravelDate";
            this.lblTravelDate.Text = "Date: —";

            // btnPay
            this.btnPay.Location = new System.Drawing.Point(100, 190);
            this.btnPay.Name = "btnPay";
            this.btnPay.Size = new System.Drawing.Size(75, 25);
            this.btnPay.Text = "Pay";
            this.btnPay.UseVisualStyleBackColor = true;
            this.btnPay.Click += new System.EventHandler(this.btnPay_Click);

            // PaymentForm
            this.ClientSize = new System.Drawing.Size(280, 230);
            this.Controls.Add(this.labelBookingId);
            this.Controls.Add(this.txtBookingId);
            this.Controls.Add(this.labelAmount);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.labelCard);
            this.Controls.Add(this.txtCardNumber);
            this.Controls.Add(this.lblTrainName);
            this.Controls.Add(this.lblSeatInfo);
            this.Controls.Add(this.lblTravelDate);
            this.Controls.Add(this.btnPay);
            this.Name = "PaymentForm";
            this.Text = "Payment";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

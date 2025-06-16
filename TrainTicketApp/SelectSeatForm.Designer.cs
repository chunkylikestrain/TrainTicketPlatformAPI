namespace TrainTicketApp
{
    partial class SelectSeatForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvSeats;
        private System.Windows.Forms.DateTimePicker dtpTravelDate;
        private System.Windows.Forms.Button btnBook;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvSeats = new System.Windows.Forms.DataGridView();
            this.dtpTravelDate = new System.Windows.Forms.DateTimePicker();
            this.btnBook = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeats)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvSeats
            // 
            this.dgvSeats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSeats.Location = new System.Drawing.Point(12, 50);
            this.dgvSeats.Name = "dgvSeats";
            this.dgvSeats.ReadOnly = true;
            this.dgvSeats.RowTemplate.Height = 25;
            this.dgvSeats.Size = new System.Drawing.Size(360, 200);
            // 
            // dtpTravelDate
            // 
            this.dtpTravelDate.Location = new System.Drawing.Point(12, 12);
            this.dtpTravelDate.Name = "dtpTravelDate";
            this.dtpTravelDate.Size = new System.Drawing.Size(200, 23);
            // 
            // btnBook
            // 
            this.btnBook.Location = new System.Drawing.Point(297, 12);
            this.btnBook.Name = "btnBook";
            this.btnBook.Size = new System.Drawing.Size(75, 23);
            this.btnBook.Text = "Book";
            this.btnBook.UseVisualStyleBackColor = true;
            this.btnBook.Click += new System.EventHandler(this.btnBook_Click);
            // 
            // SelectSeatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.btnBook);
            this.Controls.Add(this.dtpTravelDate);
            this.Controls.Add(this.dgvSeats);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SelectSeatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Your Seat";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSeats)).EndInit();
            this.ResumeLayout(false);
        }
    }
}



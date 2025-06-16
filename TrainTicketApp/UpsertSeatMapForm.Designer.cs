namespace TrainTicketApp
{
    partial class UpsertSeatMapForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtId;
        private TextBox txtTrainId;
        private TextBox txtCoach;
        private TextBox txtNumber;
        private TextBox txtClassType;
        private CheckBox chkAvailable;
        private Button btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtId = new TextBox();
            this.txtTrainId = new TextBox();
            this.txtCoach = new TextBox();
            this.txtNumber = new TextBox();
            this.txtClassType = new TextBox();
            this.chkAvailable = new CheckBox();
            this.btnSave = new Button();

            this.SuspendLayout();
            // 
            // txtId (read-only)
            // 
            this.txtId.Location = new System.Drawing.Point(12, 12);
            this.txtId.ReadOnly = true;
            this.txtId.Size = new System.Drawing.Size(100, 23);
            this.txtId.PlaceholderText = "Id";
            // 
            // txtTrainId
            // 
            this.txtTrainId.Location = new System.Drawing.Point(12, 50);
            this.txtTrainId.Size = new System.Drawing.Size(100, 23);
            this.txtTrainId.PlaceholderText = "Train Id";
            // 
            // txtCoach
            // 
            this.txtCoach.Location = new System.Drawing.Point(12, 88);
            this.txtCoach.Size = new System.Drawing.Size(100, 23);
            this.txtCoach.PlaceholderText = "Coach";
            // 
            // txtNumber
            // 
            this.txtNumber.Location = new System.Drawing.Point(12, 126);
            this.txtNumber.Size = new System.Drawing.Size(100, 23);
            this.txtNumber.PlaceholderText = "Number";
            // 
            // txtClassType
            // 
            this.txtClassType.Location = new System.Drawing.Point(12, 164);
            this.txtClassType.Size = new System.Drawing.Size(100, 23);
            this.txtClassType.PlaceholderText = "ClassType";
            // 
            // chkAvailable
            // 
            this.chkAvailable.Location = new System.Drawing.Point(12, 202);
            this.chkAvailable.Text = "Available";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 240);
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // 
            // UpsertSeatMapForm
            // 
            this.ClientSize = new System.Drawing.Size(200, 280);
            this.Controls.Add(this.txtId);
            this.Controls.Add(this.txtTrainId);
            this.Controls.Add(this.txtCoach);
            this.Controls.Add(this.txtNumber);
            this.Controls.Add(this.txtClassType);
            this.Controls.Add(this.chkAvailable);
            this.Controls.Add(this.btnSave);
            this.Text = "Add / Edit Seat";
            this.Load += new System.EventHandler(this.UpsertSeatMapForm_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

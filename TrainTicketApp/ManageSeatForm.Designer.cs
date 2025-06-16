namespace TrainTicketApp
{
    partial class ManageSeatMapForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvSeatMaps;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvSeatMaps = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvSeatMaps)).BeginInit();
            this.SuspendLayout();

            // 
            // dgvSeatMaps
            // 
            this.dgvSeatMaps.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dgvSeatMaps.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSeatMaps.Location = new System.Drawing.Point(12, 12);
            this.dgvSeatMaps.MultiSelect = false;
            this.dgvSeatMaps.ReadOnly = true;
            this.dgvSeatMaps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSeatMaps.Size = new System.Drawing.Size(560, 350);
            this.dgvSeatMaps.TabIndex = 0;
            this.dgvSeatMaps.AllowUserToAddRows = false;

            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnAdd.Location = new System.Drawing.Point(12, 375);
            this.btnAdd.Size = new System.Drawing.Size(75, 30);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnEdit.Location = new System.Drawing.Point(100, 375);
            this.btnEdit.Size = new System.Drawing.Size(75, 30);
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnDelete.Location = new System.Drawing.Point(188, 375);
            this.btnDelete.Size = new System.Drawing.Size(75, 30);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // 
            // ManageSeatMapForm
            // 
            this.ClientSize = new System.Drawing.Size(584, 417);
            this.Controls.Add(this.dgvSeatMaps);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Text = "Manage Seat Maps";
            this.Load += new System.EventHandler(this.ManageSeatMapForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvSeatMaps)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

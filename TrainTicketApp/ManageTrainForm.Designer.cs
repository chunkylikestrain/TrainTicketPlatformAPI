namespace TrainTicketApp
{
    partial class ManageTrainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvTrains = new DataGridView();
            btnEdit = new Button();
            btnAdd = new Button();
            btnDelete = new Button();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvTrains).BeginInit();
            SuspendLayout();
            // 
            // dgvTrains
            // 
            dgvTrains.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTrains.Location = new Point(69, 55);
            dgvTrains.Name = "dgvTrains";
            dgvTrains.RowHeadersWidth = 51;
            dgvTrains.Size = new Size(653, 337);
            dgvTrains.TabIndex = 0;
            // 
            // Edit
            // 
            btnEdit.Location = new Point(338, 405);
            btnEdit.Name = "Edit";
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(69, 405);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(94, 29);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(628, 405);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(295, 9);
            label1.Name = "label1";
            label1.Size = new Size(184, 20);
            label1.TabIndex = 4;
            label1.Text = "Train Managment Window";
            // 
            // ManageTrainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(btnEdit);
            Controls.Add(dgvTrains);
            Name = "ManageTrainForm";
            Text = "ManageTrainForm";
            ((System.ComponentModel.ISupportInitialize)dgvTrains).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvTrains;
        private Button btnEdit;
        private Button btnAdd;
        private Button btnDelete;
        private Label label1;
    }
}
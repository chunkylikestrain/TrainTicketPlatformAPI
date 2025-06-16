namespace TrainTicketApp
{
    partial class AdminMainForm
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
            btnManageTrains = new Button();
            btnManageSeatMaps = new Button();
            btnViewReports = new Button();
            SuspendLayout();
            // 
            // btnManageTrains
            // 
            btnManageTrains.Location = new Point(133, 178);
            btnManageTrains.Name = "btnManageTrains";
            btnManageTrains.Size = new Size(131, 29);
            btnManageTrains.TabIndex = 0;
            btnManageTrains.Text = "Manage Train";
            btnManageTrains.UseVisualStyleBackColor = true;
            // 
            // btnManageSeatMaps
            // 
            btnManageSeatMaps.Location = new Point(330, 178);
            btnManageSeatMaps.Name = "btnManageSeatMaps";
            btnManageSeatMaps.Size = new Size(147, 29);
            btnManageSeatMaps.TabIndex = 1;
            btnManageSeatMaps.Text = "Manage Seat Maps";
            btnManageSeatMaps.UseVisualStyleBackColor = true;
            // 
            // btnViewReports
            // 
            btnViewReports.Location = new Point(544, 178);
            btnViewReports.Name = "btnViewReports";
            btnViewReports.Size = new Size(108, 29);
            btnViewReports.TabIndex = 2;
            btnViewReports.Text = "View Reports";
            btnViewReports.UseVisualStyleBackColor = true;
            // 
            // AdminMainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnViewReports);
            Controls.Add(btnManageSeatMaps);
            Controls.Add(btnManageTrains);
            Name = "AdminMainForm";
            Text = "AdminMainForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnManageTrains;
        private Button btnManageSeatMaps;
        private Button btnViewReports;
    }
}
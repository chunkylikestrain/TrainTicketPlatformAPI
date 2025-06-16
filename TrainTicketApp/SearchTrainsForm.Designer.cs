namespace TrainTicketApp
{
    partial class SearchTrainsForm
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
            label1 = new Label();
            txtDeparture = new TextBox();
            label2 = new Label();
            txtArrival = new TextBox();
            label3 = new Label();
            dtpTravelDate = new DateTimePicker();
            btnSearch = new Button();
            dgvTrains = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvTrains).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(237, 15);
            label1.Name = "label1";
            label1.Size = new Size(76, 20);
            label1.TabIndex = 0;
            label1.Text = "Departure";
            // 
            // txtDeparture
            // 
            txtDeparture.Location = new Point(334, 13);
            txtDeparture.Name = "txtDeparture";
            txtDeparture.Size = new Size(125, 27);
            txtDeparture.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(250, 48);
            label2.Name = "label2";
            label2.Size = new Size(52, 20);
            label2.TabIndex = 2;
            label2.Text = "Arrival";
            // 
            // txtArrival
            // 
            txtArrival.Location = new Point(334, 48);
            txtArrival.Name = "txtArrival";
            txtArrival.Size = new Size(125, 27);
            txtArrival.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(237, 83);
            label3.Name = "label3";
            label3.Size = new Size(84, 20);
            label3.TabIndex = 4;
            label3.Text = "Travel Date";
            // 
            // dtpTravelDate
            // 
            dtpTravelDate.Location = new Point(334, 83);
            dtpTravelDate.Name = "dtpTravelDate";
            dtpTravelDate.Size = new Size(250, 27);
            dtpTravelDate.TabIndex = 6;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(351, 116);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // dgvTrains
            // 
            dgvTrains.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTrains.Location = new Point(149, 151);
            dgvTrains.Name = "dgvTrains";
            dgvTrains.RowHeadersWidth = 51;
            dgvTrains.Size = new Size(495, 287);
            dgvTrains.TabIndex = 8;
            dgvTrains.CellDoubleClick += dgvTrains_CellDoubleClick;
            // 
            // SearchTrainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvTrains);
            Controls.Add(btnSearch);
            Controls.Add(dtpTravelDate);
            Controls.Add(label3);
            Controls.Add(txtArrival);
            Controls.Add(label2);
            Controls.Add(txtDeparture);
            Controls.Add(label1);
            Name = "SearchTrainForm";
            Text = "SearchTrainForm";
            ((System.ComponentModel.ISupportInitialize)dgvTrains).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtDeparture;
        private Label label2;
        private TextBox txtArrival;
        private Label label3;
        private DateTimePicker dtpTravelDate;
        private Button btnSearch;
        private DataGridView dgvTrains;
    }
}
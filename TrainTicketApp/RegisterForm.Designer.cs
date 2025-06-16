namespace TrainTicketApp
{
    partial class RegisterForm
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
            label2 = new Label();
            label3 = new Label();
            txtRegEmail = new TextBox();
            txtRegPhone = new TextBox();
            txtRegPwd = new TextBox();
            btnRegister = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(277, 14);
            label1.Name = "label1";
            label1.Size = new Size(53, 20);
            label1.TabIndex = 0;
            label1.Text = "Email :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(215, 55);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 1;
            label2.Text = "Phone Number :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(253, 93);
            label3.Name = "label3";
            label3.Size = new Size(77, 20);
            label3.TabIndex = 2;
            label3.Text = "Password :";
            // 
            // txtRegEmail
            // 
            txtRegEmail.Location = new Point(336, 14);
            txtRegEmail.Name = "txtRegEmail";
            txtRegEmail.Size = new Size(125, 27);
            txtRegEmail.TabIndex = 3;
            // 
            // txtRegPhone
            // 
            txtRegPhone.Location = new Point(336, 55);
            txtRegPhone.Name = "txtRegPhone";
            txtRegPhone.Size = new Size(125, 27);
            txtRegPhone.TabIndex = 4;
            // 
            // txtRegPwd
            // 
            txtRegPwd.Location = new Point(336, 93);
            txtRegPwd.Name = "txtRegPwd";
            txtRegPwd.Size = new Size(125, 27);
            txtRegPwd.TabIndex = 5;
            txtRegPwd.UseSystemPasswordChar = true;
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(349, 137);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(94, 29);
            btnRegister.TabIndex = 6;
            btnRegister.Text = "Register";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // RegisterForm1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnRegister);
            Controls.Add(txtRegPwd);
            Controls.Add(txtRegPhone);
            Controls.Add(txtRegEmail);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "RegisterForm1";
            Text = "RegisterForm1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtRegEmail;
        private TextBox txtRegPhone;
        private TextBox txtRegPwd;
        private Button btnRegister;
    }
}
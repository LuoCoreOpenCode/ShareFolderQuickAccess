namespace ShareFolderQuickAccess
{
    partial class CredentialDialog
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblServer = new Label();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPass = new Label();
            txtPass = new TextBox();
            chkSave = new CheckBox();
            okButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // lblServer
            // 
            lblServer.AutoSize = true;
            lblServer.Location = new Point(16, 15);
            lblServer.Name = "lblServer";
            lblServer.TabIndex = 0;
            lblServer.Text = "目标:";
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Location = new Point(16, 50);
            lblUser.Name = "lblUser";
            lblUser.TabIndex = 1;
            lblUser.Text = "用户名(&U):";
            // 
            // txtUser
            // 
            txtUser.Location = new Point(104, 46);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(280, 31);
            txtUser.TabIndex = 2;
            // 
            // lblPass
            // 
            lblPass.AutoSize = true;
            lblPass.Location = new Point(16, 88);
            lblPass.Name = "lblPass";
            lblPass.TabIndex = 3;
            lblPass.Text = "密码(&P):";
            // 
            // txtPass
            // 
            txtPass.Location = new Point(104, 84);
            txtPass.Name = "txtPass";
            txtPass.Size = new Size(280, 31);
            txtPass.TabIndex = 4;
            txtPass.UseSystemPasswordChar = true;
            // 
            // chkSave
            // 
            chkSave.AutoSize = true;
            chkSave.Checked = true;
            chkSave.CheckState = CheckState.Checked;
            chkSave.Location = new Point(104, 124);
            chkSave.Name = "chkSave";
            chkSave.TabIndex = 5;
            chkSave.Text = "保存到 Windows 凭据管理器（推荐）";
            // 
            // okButton
            // 
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(128, 162);
            okButton.Name = "okButton";
            okButton.Size = new Size(118, 40);
            okButton.TabIndex = 6;
            okButton.Text = "连接(&C)";
            okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(266, 162);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(118, 40);
            cancelButton.TabIndex = 7;
            cancelButton.Text = "取消";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // CredentialDialog
            // 
            AcceptButton = okButton;
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(400, 218);
            Controls.Add(lblServer);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(chkSave);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CredentialDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "输入共享凭据";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblServer;
        private Label lblUser;
        private TextBox txtUser;
        private Label lblPass;
        private TextBox txtPass;
        private CheckBox chkSave;
        private Button okButton;
        private Button cancelButton;
    }
}

namespace ShareFolderQuickAccess
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            heartbeatTimer = new System.Windows.Forms.Timer(components);
            trayIcon = new NotifyIcon(components);
            lblPath = new Label();
            pathBox = new TextBox();
            openButton = new Button();
            diagButton = new Button();
            keepAliveCheckBox = new CheckBox();
            lblInterval = new Label();
            intervalUpDown = new NumericUpDown();
            lblSec = new Label();
            trayCheckBox = new CheckBox();
            lblFallback = new Label();
            fallbackIpBox = new TextBox();
            lblFallbackHint = new Label();
            fixHostsButton = new Button();
            logGroup = new GroupBox();
            logBox = new RichTextBox();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)intervalUpDown).BeginInit();
            logGroup.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // heartbeatTimer
            // 
            heartbeatTimer.Interval = 60000;
            // 
            // trayIcon
            // 
            trayIcon.Text = "共享文件夹一键直达";
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.Location = new Point(16, 21);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(77, 21);
            lblPath.TabIndex = 0;
            lblPath.Text = "共享路径(&P):";
            // 
            // pathBox
            // 
            pathBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pathBox.Location = new Point(96, 17);
            pathBox.Name = "pathBox";
            pathBox.Size = new Size(568, 31);
            pathBox.TabIndex = 1;
            pathBox.Text = "\\\\server\\share";
            // 
            // openButton
            // 
            openButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            openButton.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            openButton.Location = new Point(16, 54);
            openButton.Name = "openButton";
            openButton.Size = new Size(530, 46);
            openButton.TabIndex = 2;
            openButton.Text = "📂  一键打开共享文件夹";
            openButton.UseVisualStyleBackColor = true;
            // 
            // diagButton
            // 
            diagButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            diagButton.Location = new Point(556, 54);
            diagButton.Name = "diagButton";
            diagButton.Size = new Size(108, 46);
            diagButton.TabIndex = 3;
            diagButton.Text = "🩺 诊断";
            diagButton.UseVisualStyleBackColor = true;
            // 
            // keepAliveCheckBox
            // 
            keepAliveCheckBox.AutoSize = true;
            keepAliveCheckBox.Checked = true;
            keepAliveCheckBox.CheckState = CheckState.Checked;
            keepAliveCheckBox.Location = new Point(16, 114);
            keepAliveCheckBox.Name = "keepAliveCheckBox";
            keepAliveCheckBox.Size = new Size(265, 25);
            keepAliveCheckBox.TabIndex = 4;
            keepAliveCheckBox.Text = "启用保活心跳（防空闲断连）";
            // 
            // lblInterval
            // 
            lblInterval.AutoSize = true;
            lblInterval.Location = new Point(300, 117);
            lblInterval.Name = "lblInterval";
            lblInterval.Size = new Size(44, 21);
            lblInterval.TabIndex = 5;
            lblInterval.Text = "间隔:";
            // 
            // intervalUpDown
            // 
            intervalUpDown.Increment = 15;
            intervalUpDown.Location = new Point(348, 113);
            intervalUpDown.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            intervalUpDown.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            intervalUpDown.Name = "intervalUpDown";
            intervalUpDown.Size = new Size(64, 31);
            intervalUpDown.TabIndex = 6;
            intervalUpDown.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // lblSec
            // 
            lblSec.AutoSize = true;
            lblSec.Location = new Point(418, 117);
            lblSec.Name = "lblSec";
            lblSec.Size = new Size(26, 21);
            lblSec.TabIndex = 7;
            lblSec.Text = "秒";
            // 
            // trayCheckBox
            // 
            trayCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            trayCheckBox.AutoSize = true;
            trayCheckBox.Checked = true;
            trayCheckBox.CheckState = CheckState.Checked;
            trayCheckBox.Location = new Point(452, 114);
            trayCheckBox.Name = "trayCheckBox";
            trayCheckBox.Size = new Size(232, 25);
            trayCheckBox.TabIndex = 8;
            trayCheckBox.Text = "关闭窗口时最小化到托盘";
            // 
            // lblFallback
            // 
            lblFallback.AutoSize = true;
            lblFallback.Location = new Point(16, 150);
            lblFallback.Name = "lblFallback";
            lblFallback.TabIndex = 11;
            lblFallback.Text = "IP 兜底:";
            // 
            // fallbackIpBox
            // 
            fallbackIpBox.Location = new Point(96, 146);
            fallbackIpBox.Name = "fallbackIpBox";
            fallbackIpBox.Size = new Size(150, 31);
            fallbackIpBox.TabIndex = 12;
            fallbackIpBox.Text = "192.168.1.10";
            // 
            // lblFallbackHint
            // 
            lblFallbackHint.AutoSize = true;
            lblFallbackHint.ForeColor = Color.Gray;
            lblFallbackHint.Location = new Point(254, 150);
            lblFallbackHint.Name = "lblFallbackHint";
            lblFallbackHint.TabIndex = 13;
            lblFallbackHint.Text = "留空则禁用";
            // 
            // fixHostsButton
            // 
            fixHostsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            fixHostsButton.Location = new Point(452, 144);
            fixHostsButton.Name = "fixHostsButton";
            fixHostsButton.Size = new Size(212, 34);
            fixHostsButton.TabIndex = 14;
            fixHostsButton.Text = "🔧 一键写入 hosts 根治";
            fixHostsButton.UseVisualStyleBackColor = true;
            // 
            // logGroup
            // 
            logGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logGroup.Controls.Add(logBox);
            logGroup.Location = new Point(16, 176);
            logGroup.Name = "logGroup";
            logGroup.Size = new Size(648, 298);
            logGroup.TabIndex = 9;
            logGroup.TabStop = false;
            logGroup.Text = "运行日志";
            // 
            // logBox
            // 
            logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logBox.BackColor = Color.White;
            logBox.BorderStyle = BorderStyle.None;
            logBox.Font = new Font("Consolas", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            logBox.Location = new Point(12, 24);
            logBox.Name = "logBox";
            logBox.ReadOnly = true;
            logBox.Size = new Size(624, 262);
            logBox.TabIndex = 0;
            logBox.Text = "";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 490);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(680, 30);
            statusStrip.TabIndex = 10;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(665, 25);
            statusLabel.Spring = true;
            statusLabel.Text = "● 检测中…";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AcceptButton = openButton;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 520);
            Controls.Add(lblPath);
            Controls.Add(pathBox);
            Controls.Add(openButton);
            Controls.Add(diagButton);
            Controls.Add(keepAliveCheckBox);
            Controls.Add(lblInterval);
            Controls.Add(intervalUpDown);
            Controls.Add(lblSec);
            Controls.Add(trayCheckBox);
            Controls.Add(lblFallback);
            Controls.Add(fallbackIpBox);
            Controls.Add(lblFallbackHint);
            Controls.Add(fixHostsButton);
            Controls.Add(logGroup);
            Controls.Add(statusStrip);
            MinimumSize = new Size(560, 420);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "共享文件夹一键直达";
            // 
            // 事件绑定（集中放在末尾，避免设计器初始化期间触发副作用）
            // 
            openButton.Click += new EventHandler(OpenButton_Click);
            diagButton.Click += new EventHandler(DiagButton_Click);
            keepAliveCheckBox.CheckedChanged += new EventHandler(KeepAliveCheckBox_CheckedChanged);
            intervalUpDown.ValueChanged += new EventHandler(IntervalUpDown_ValueChanged);
            heartbeatTimer.Tick += new EventHandler(Heartbeat_Tick);
            trayIcon.DoubleClick += new EventHandler(TrayIcon_DoubleClick);
            fixHostsButton.Click += new EventHandler(FixHostsButton_Click);
            ((System.ComponentModel.ISupportInitialize)intervalUpDown).EndInit();
            logGroup.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPath;
        private TextBox pathBox;
        private Button openButton;
        private Button diagButton;
        private CheckBox keepAliveCheckBox;
        private Label lblInterval;
        private NumericUpDown intervalUpDown;
        private Label lblSec;
        private CheckBox trayCheckBox;
        private Label lblFallback;
        private TextBox fallbackIpBox;
        private Label lblFallbackHint;
        private Button fixHostsButton;
        private GroupBox logGroup;
        private RichTextBox logBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private NotifyIcon trayIcon;
        private System.Windows.Forms.Timer heartbeatTimer;
    }
}

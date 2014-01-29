namespace TestOnvif
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
        protected override void Dispose(bool disposing)
        {
          if (disposing)
          {
            if (components != null)
            {
              components.Dispose();
            }
            //if (gchCallbackDelegate.IsAllocated)
            //    gchCallbackDelegate.Free();
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
            this.VideoStopButton = new System.Windows.Forms.Button();
            this.VideoStartButton = new System.Windows.Forms.Button();
            this.WsDicoveryButton = new System.Windows.Forms.Button();
            this.RebootButton = new System.Windows.Forms.Button();
            this.SetDateTimefromNtpButton = new System.Windows.Forms.Button();
            this.setDateTimeButton = new System.Windows.Forms.Button();
            this.GetConfigurationButton = new System.Windows.Forms.Button();
            this.getDeviceInformationButton = new System.Windows.Forms.Button();
            this.getHostnameButton = new System.Windows.Forms.Button();
            this.GetSystemDateAndTimeButton = new System.Windows.Forms.Button();
            this.TimerLabel = new System.Windows.Forms.Label();
            this.MediaClientGetProfilesButton = new System.Windows.Forms.Button();
            this.OpenVideoButton = new System.Windows.Forms.Button();
            this.MediaProfileComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LoggerTextBox = new System.Windows.Forms.RichTextBox();
            this.WebButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.MediaDeviceComboBox = new System.Windows.Forms.ComboBox();
            this.FindDeviceButton = new System.Windows.Forms.Button();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // VideoStopButton
            // 
            this.VideoStopButton.Enabled = false;
            this.VideoStopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.VideoStopButton.Location = new System.Drawing.Point(444, 154);
            this.VideoStopButton.Name = "VideoStopButton";
            this.VideoStopButton.Size = new System.Drawing.Size(108, 50);
            this.VideoStopButton.TabIndex = 33;
            this.VideoStopButton.Text = "Stop";
            this.VideoStopButton.UseVisualStyleBackColor = true;
            this.VideoStopButton.Click += new System.EventHandler(this.VideoStopButton_Click);
            // 
            // VideoStartButton
            // 
            this.VideoStartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.VideoStartButton.Location = new System.Drawing.Point(302, 154);
            this.VideoStartButton.Name = "VideoStartButton";
            this.VideoStartButton.Size = new System.Drawing.Size(136, 50);
            this.VideoStartButton.TabIndex = 32;
            this.VideoStartButton.Text = "Video Start";
            this.VideoStartButton.UseVisualStyleBackColor = true;
            this.VideoStartButton.Click += new System.EventHandler(this.VideoStartButton_Click);
            // 
            // WsDicoveryButton
            // 
            this.WsDicoveryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.WsDicoveryButton.Location = new System.Drawing.Point(12, 108);
            this.WsDicoveryButton.Name = "WsDicoveryButton";
            this.WsDicoveryButton.Size = new System.Drawing.Size(170, 45);
            this.WsDicoveryButton.TabIndex = 30;
            this.WsDicoveryButton.Text = "WSDiscovery";
            this.WsDicoveryButton.UseVisualStyleBackColor = true;
            this.WsDicoveryButton.Click += new System.EventHandler(this.WsDicoveryButton_Click);
            // 
            // RebootButton
            // 
            this.RebootButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RebootButton.Location = new System.Drawing.Point(12, 474);
            this.RebootButton.Name = "RebootButton";
            this.RebootButton.Size = new System.Drawing.Size(170, 45);
            this.RebootButton.TabIndex = 29;
            this.RebootButton.Text = "Reboot";
            this.RebootButton.UseVisualStyleBackColor = true;
            this.RebootButton.Click += new System.EventHandler(this.RebootButton_Click);
            // 
            // SetDateTimefromNtpButton
            // 
            this.SetDateTimefromNtpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SetDateTimefromNtpButton.Location = new System.Drawing.Point(284, 312);
            this.SetDateTimefromNtpButton.Name = "SetDateTimefromNtpButton";
            this.SetDateTimefromNtpButton.Size = new System.Drawing.Size(275, 45);
            this.SetDateTimefromNtpButton.TabIndex = 27;
            this.SetDateTimefromNtpButton.Text = "SetSystemDateAndTimeNTP";
            this.SetDateTimefromNtpButton.UseVisualStyleBackColor = true;
            this.SetDateTimefromNtpButton.Click += new System.EventHandler(this.SetDateTimefromNtpButton_Click);
            // 
            // setDateTimeButton
            // 
            this.setDateTimeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.setDateTimeButton.Location = new System.Drawing.Point(284, 363);
            this.setDateTimeButton.Name = "setDateTimeButton";
            this.setDateTimeButton.Size = new System.Drawing.Size(275, 45);
            this.setDateTimeButton.TabIndex = 26;
            this.setDateTimeButton.Text = "SetSystemDateAndTime";
            this.setDateTimeButton.UseVisualStyleBackColor = true;
            this.setDateTimeButton.Click += new System.EventHandler(this.setDateTimeButton_Click);
            // 
            // GetConfigurationButton
            // 
            this.GetConfigurationButton.Enabled = false;
            this.GetConfigurationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GetConfigurationButton.Location = new System.Drawing.Point(12, 159);
            this.GetConfigurationButton.Name = "GetConfigurationButton";
            this.GetConfigurationButton.Size = new System.Drawing.Size(170, 45);
            this.GetConfigurationButton.TabIndex = 24;
            this.GetConfigurationButton.Text = "GetConfiguration";
            this.GetConfigurationButton.UseVisualStyleBackColor = true;
            this.GetConfigurationButton.Click += new System.EventHandler(this.GetConfigurationButton_Click);
            // 
            // getDeviceInformationButton
            // 
            this.getDeviceInformationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.getDeviceInformationButton.Location = new System.Drawing.Point(12, 261);
            this.getDeviceInformationButton.Name = "getDeviceInformationButton";
            this.getDeviceInformationButton.Size = new System.Drawing.Size(226, 45);
            this.getDeviceInformationButton.TabIndex = 21;
            this.getDeviceInformationButton.Text = "GetDeviceInformation";
            this.getDeviceInformationButton.UseVisualStyleBackColor = true;
            this.getDeviceInformationButton.Click += new System.EventHandler(this.getDeviceInformationButton_Click);
            // 
            // getHostnameButton
            // 
            this.getHostnameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.getHostnameButton.Location = new System.Drawing.Point(12, 210);
            this.getHostnameButton.Name = "getHostnameButton";
            this.getHostnameButton.Size = new System.Drawing.Size(170, 45);
            this.getHostnameButton.TabIndex = 23;
            this.getHostnameButton.Text = "GetHostname";
            this.getHostnameButton.UseVisualStyleBackColor = true;
            this.getHostnameButton.Click += new System.EventHandler(this.getHostnameButton_Click);
            // 
            // GetSystemDateAndTimeButton
            // 
            this.GetSystemDateAndTimeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GetSystemDateAndTimeButton.Location = new System.Drawing.Point(12, 312);
            this.GetSystemDateAndTimeButton.Name = "GetSystemDateAndTimeButton";
            this.GetSystemDateAndTimeButton.Size = new System.Drawing.Size(226, 45);
            this.GetSystemDateAndTimeButton.TabIndex = 22;
            this.GetSystemDateAndTimeButton.Text = "GetSystemDateAndTime";
            this.GetSystemDateAndTimeButton.UseVisualStyleBackColor = true;
            this.GetSystemDateAndTimeButton.Click += new System.EventHandler(this.GetSystemDateAndTimeButton_Click);
            // 
            // TimerLabel
            // 
            this.TimerLabel.AutoSize = true;
            this.TimerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.TimerLabel.Location = new System.Drawing.Point(8, 64);
            this.TimerLabel.Name = "TimerLabel";
            this.TimerLabel.Size = new System.Drawing.Size(58, 24);
            this.TimerLabel.TabIndex = 35;
            this.TimerLabel.Text = "Time:";
            // 
            // MediaClientGetProfilesButton
            // 
            this.MediaClientGetProfilesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MediaClientGetProfilesButton.Location = new System.Drawing.Point(12, 363);
            this.MediaClientGetProfilesButton.Name = "MediaClientGetProfilesButton";
            this.MediaClientGetProfilesButton.Size = new System.Drawing.Size(226, 45);
            this.MediaClientGetProfilesButton.TabIndex = 36;
            this.MediaClientGetProfilesButton.Text = "MediaClient.GetProfiles";
            this.MediaClientGetProfilesButton.UseVisualStyleBackColor = true;
            this.MediaClientGetProfilesButton.Click += new System.EventHandler(this.MediaClientGetProfilesButton_Click);
            // 
            // OpenVideoButton
            // 
            this.OpenVideoButton.Enabled = false;
            this.OpenVideoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.OpenVideoButton.Location = new System.Drawing.Point(414, 474);
            this.OpenVideoButton.Name = "OpenVideoButton";
            this.OpenVideoButton.Size = new System.Drawing.Size(145, 50);
            this.OpenVideoButton.TabIndex = 38;
            this.OpenVideoButton.Text = "Open Video";
            this.OpenVideoButton.UseVisualStyleBackColor = true;
            this.OpenVideoButton.Click += new System.EventHandler(this.OpenVideoButton_Click);
            // 
            // MediaProfileComboBox
            // 
            this.MediaProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MediaProfileComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MediaProfileComboBox.FormattingEnabled = true;
            this.MediaProfileComboBox.Location = new System.Drawing.Point(389, 105);
            this.MediaProfileComboBox.Name = "MediaProfileComboBox";
            this.MediaProfileComboBox.Size = new System.Drawing.Size(166, 32);
            this.MediaProfileComboBox.TabIndex = 39;
            this.MediaProfileComboBox.SelectedIndexChanged += new System.EventHandler(this.MediaProfileComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(264, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 24);
            this.label1.TabIndex = 40;
            this.label1.Text = "MediaProfile:";
            // 
            // LoggerTextBox
            // 
            this.LoggerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LoggerTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoggerTextBox.Location = new System.Drawing.Point(12, 535);
            this.LoggerTextBox.Name = "LoggerTextBox";
            this.LoggerTextBox.Size = new System.Drawing.Size(551, 67);
            this.LoggerTextBox.TabIndex = 41;
            this.LoggerTextBox.Text = "";
            // 
            // WebButton
            // 
            this.WebButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.WebButton.Location = new System.Drawing.Point(407, 239);
            this.WebButton.Name = "WebButton";
            this.WebButton.Size = new System.Drawing.Size(145, 50);
            this.WebButton.TabIndex = 42;
            this.WebButton.Text = "Web";
            this.WebButton.UseVisualStyleBackColor = true;
            this.WebButton.Click += new System.EventHandler(this.WebButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(12, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 24);
            this.label2.TabIndex = 44;
            this.label2.Text = "Device:";
            // 
            // MediaDeviceComboBox
            // 
            this.MediaDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MediaDeviceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MediaDeviceComboBox.FormattingEnabled = true;
            this.MediaDeviceComboBox.Location = new System.Drawing.Point(91, 9);
            this.MediaDeviceComboBox.Name = "MediaDeviceComboBox";
            this.MediaDeviceComboBox.Size = new System.Drawing.Size(166, 32);
            this.MediaDeviceComboBox.TabIndex = 43;
            //this.MediaDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceComboBox_SelectedIndexChanged);
            // 
            // FindDeviceButton
            // 
            this.FindDeviceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FindDeviceButton.Location = new System.Drawing.Point(268, 4);
            this.FindDeviceButton.Name = "FindDeviceButton";
            this.FindDeviceButton.Size = new System.Drawing.Size(145, 41);
            this.FindDeviceButton.TabIndex = 45;
            this.FindDeviceButton.Text = "FindDevice";
            this.FindDeviceButton.UseVisualStyleBackColor = true;
            this.FindDeviceButton.Click += new System.EventHandler(this.FindDeviceButton_Click);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ConnectButton.Location = new System.Drawing.Point(419, 4);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(145, 41);
            this.ConnectButton.TabIndex = 46;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 614);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.FindDeviceButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MediaDeviceComboBox);
            this.Controls.Add(this.WebButton);
            this.Controls.Add(this.LoggerTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MediaProfileComboBox);
            this.Controls.Add(this.OpenVideoButton);
            this.Controls.Add(this.MediaClientGetProfilesButton);
            this.Controls.Add(this.TimerLabel);
            this.Controls.Add(this.VideoStopButton);
            this.Controls.Add(this.VideoStartButton);
            this.Controls.Add(this.WsDicoveryButton);
            this.Controls.Add(this.RebootButton);
            this.Controls.Add(this.SetDateTimefromNtpButton);
            this.Controls.Add(this.setDateTimeButton);
            this.Controls.Add(this.GetConfigurationButton);
            this.Controls.Add(this.getDeviceInformationButton);
            this.Controls.Add(this.getHostnameButton);
            this.Controls.Add(this.GetSystemDateAndTimeButton);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button VideoStopButton;
        private System.Windows.Forms.Button VideoStartButton;
        private System.Windows.Forms.Button WsDicoveryButton;
        private System.Windows.Forms.Button RebootButton;
        private System.Windows.Forms.Button SetDateTimefromNtpButton;
        private System.Windows.Forms.Button setDateTimeButton;
        private System.Windows.Forms.Button GetConfigurationButton;
        private System.Windows.Forms.Button getDeviceInformationButton;
        private System.Windows.Forms.Button getHostnameButton;
        private System.Windows.Forms.Button GetSystemDateAndTimeButton;
        private System.Windows.Forms.Label TimerLabel;
        private System.Windows.Forms.Button MediaClientGetProfilesButton;
        private System.Windows.Forms.Button OpenVideoButton;
        private System.Windows.Forms.ComboBox MediaProfileComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox LoggerTextBox;
        private System.Windows.Forms.Button WebButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox MediaDeviceComboBox;
        private System.Windows.Forms.Button FindDeviceButton;
        private System.Windows.Forms.Button ConnectButton;


    }
}


namespace Onvif.Controls
{
    partial class ClockForm
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
            this.ClockLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ClockLabel
            // 
            this.ClockLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClockLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ClockLabel.Location = new System.Drawing.Point(0, 0);
            this.ClockLabel.Name = "ClockLabel";
            this.ClockLabel.Size = new System.Drawing.Size(392, 92);
            this.ClockLabel.TabIndex = 38;
            this.ClockLabel.Text = "Time:";
            this.ClockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ClockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 92);
            this.ControlBox = false;
            this.Controls.Add(this.ClockLabel);
            this.Name = "ClockForm";
            this.Text = "RtpTime";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ClockLabel;
    }
}
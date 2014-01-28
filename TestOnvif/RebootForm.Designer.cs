namespace TestOnvif
{
    partial class RebootForm
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
            this.RebootProgressBar = new System.Windows.Forms.ProgressBar();
            this.RebootMessgeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RebootProgressBar
            // 
            this.RebootProgressBar.Location = new System.Drawing.Point(12, 100);
            this.RebootProgressBar.Name = "RebootProgressBar";
            this.RebootProgressBar.Size = new System.Drawing.Size(431, 23);
            this.RebootProgressBar.TabIndex = 0;
            // 
            // RebootMessgeLabel
            // 
            this.RebootMessgeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RebootMessgeLabel.Location = new System.Drawing.Point(12, 9);
            this.RebootMessgeLabel.Name = "RebootMessgeLabel";
            this.RebootMessgeLabel.Size = new System.Drawing.Size(431, 77);
            this.RebootMessgeLabel.TabIndex = 1;
            this.RebootMessgeLabel.Text = "REBOOT";
            this.RebootMessgeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RebootForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 141);
            this.ControlBox = false;
            this.Controls.Add(this.RebootMessgeLabel);
            this.Controls.Add(this.RebootProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RebootForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "REBOOT!!!";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar RebootProgressBar;
        private System.Windows.Forms.Label RebootMessgeLabel;
    }
}
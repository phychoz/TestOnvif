namespace Onvif.Controls
{
    partial class MediaClientProfilesForm
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
            this.MediaProfilesTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // MediaProfilesTreeView
            // 
            this.MediaProfilesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MediaProfilesTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MediaProfilesTreeView.Location = new System.Drawing.Point(0, 0);
            this.MediaProfilesTreeView.Name = "MediaProfilesTreeView";
            this.MediaProfilesTreeView.Size = new System.Drawing.Size(492, 573);
            this.MediaProfilesTreeView.TabIndex = 0;
            // 
            // MediaClientProfilesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 573);
            this.Controls.Add(this.MediaProfilesTreeView);
            this.Name = "MediaClientProfilesForm";
            this.Text = "MediaClientProfilesForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView MediaProfilesTreeView;
    }
}
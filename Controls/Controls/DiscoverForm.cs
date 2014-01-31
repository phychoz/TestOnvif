using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Onvif.Controls
{
    public partial class DiscoverForm : Form
    {
        public DiscoverForm()
        {
            InitializeComponent();
            FillDiscoverBox();
        }

        private void FillDiscoverBox()
        {
            var devices = ONVIFClient.GetAvailableMediaDevices();

            BindMediaDeviceCollection(devices);
        }

        public void BindMediaDeviceCollection(MediaDevice[] devices)
        {
            BindingSource binding = new BindingSource()
            {
                DataSource = devices
            };

            this.DeviceBox.DataSource = binding.DataSource;
            this.DeviceBox.DisplayMember = "DisplayName";

        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var device = this.DeviceBox.SelectedValue as MediaDevice;
            if (device != null)
            {
                MessageBox.Show(device.DisplayName);
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}

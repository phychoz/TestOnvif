using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestOnvif
{
    public partial class DiscoverForm : Form
    {
        MediaDevice[] mediaDeviceCollection;

        public DiscoverForm(MediaDevice[] devices)
        {
            mediaDeviceCollection = devices;

            InitializeComponent();
            FillDiscoverBox();

        }

        private void FillDiscoverBox()
        {
            for (int index = 0; index < mediaDeviceCollection.Length; index++)
            {
                //mediaDeviceCollection[index].OnvifClient.Init();
                mediaDeviceCollection[index].ONVIFClient.GetDeviceInformation();
                DeviceBox.Items.Add(mediaDeviceCollection[index].ONVIFClient.DeviceInformation.Model);
            }
            DeviceBox.SelectedIndex = 0;
        }


        private void ApplyButton_Click(object sender, EventArgs e)
        {
            int index = DeviceBox.SelectedIndex;
            string result = String.Format("device#{0}-{1}", index + 1, mediaDeviceCollection[index].ONVIFClient.DeviceInformation.Model);
            MessageBox.Show(result);
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Net;
namespace TestOnvif
{
    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer timer;
        
        public MainForm()
        {
            InitializeComponent();
 
            timer = new System.Windows.Forms.Timer() { /*Interval = 1000*/ };
            timer.Tick += (obj, arg) => 
            { 
                TimerLabel.Text = "Time: " + DateTime.Now.ToString("HH:mm:ss");
                //CameraLabel.Text = "Time: " + CameraDateTime.Get(address);//mediaDevice.GetSystemDateAndTime();
            };
            timer.Start();

        }

        public void Echo(string message)
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke((Action)(() => { this.LoggerTextBox.AppendText(message); }));
            }
            else
                this.LoggerTextBox.AppendText(message);
        }

        public void BindMediaDeviceCollection(MediaDevice[] devices)
        {
            BindingSource binding = new BindingSource() 
            {
                DataSource = devices 
            };

            this.MediaDeviceComboBox.DataSource = binding.DataSource;
            this.MediaDeviceComboBox.DisplayMember = "DisplayName";

        }

        public void BindMediaProfileCollection(MediaDevice device)
        {
            BindingSource binding = new BindingSource()
            {
                DataSource = device.ONVIFClient.MediaProfiles
            };

            this.MediaProfileComboBox.DataSource = binding.DataSource;
            this.MediaProfileComboBox.DisplayMember = "Name";
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            var device = this.MediaDeviceComboBox.SelectedValue as MediaDevice;

            if (device != null)
            {
                MediaService.Instance.Connect(device, "admin", "123456");
            }

        }


        private void VideoStartButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.Start();
        }

        public void UIStartMode()
        {
            VideoStartButton.Enabled = false;
            MediaProfileComboBox.Enabled = false;

            VideoStopButton.Enabled = true;
        }

        private void VideoStopButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.Stop();
        }



        public void UIStopMode()
        {

            VideoStartButton.Enabled = true;
            VideoStopButton.Enabled = false;
            MediaProfileComboBox.Enabled = true;
        }

        #region ONVIF

        private void WsDicoveryButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("WsDicovery");
        }

        private void FindDeviceButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.FindDevices();
        }

        private void setDateTimeButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("SetDateTime");
        }

        private void SetDateTimefromNtpButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("SetDateTimefromNtp");
        }
        private void RebootButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("Reboot");
        }

        private void getDeviceInformationButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("GetDeviceInformation");
        }

        private void GetSystemDateAndTimeButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("GetSystemDateAndTime");

        }

        private void getHostnameButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("GetHostname");
        }
        #endregion


        private void WriteToFileCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //if(WriteToFileCheckBox.Checked)
            //    mediaDevice.FFmpegProcessor.WriteVideoFrameToFile = true;
            //else
            //    mediaDevice.FFmpegProcessor.WriteVideoFrameToFile = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if (mediaDevice != null)
            //    mediaDevice.StopMedia();
        }

        private void MediaClientGetProfilesButton_Click(object sender, EventArgs e)
        {
            //MediaClientProfilesForm mcpf = new MediaClientProfilesForm(mediaDevice.ONVIFClient.MediaProfiles);
            //mcpf.ShowDialog();
        }

        private void GetConfigurationButton_Click(object sender, EventArgs e)
        {

        }

        private void OpenVideoButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\";
            openFileDialog.Filter = @"video files (*.mkv)|*.mkv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(openFileDialog.FileName) == false)
                {
                    string filename = openFileDialog.FileName;
                    //this.Text = inputMediaFile;

                    //Start();
                    //mediaDevice.OpenVideo(filename);
                }
            }
        }
        private void WebButton_Click(object sender, EventArgs e)
        {
            MediaService.Instance.ExecuteCommand("ShowWebForm");
        }

        private void MediaProfileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(MediaService.Instance.MediaDevice!=null)
                MediaService.Instance.MediaDevice.ONVIFClient.MediaProfileIndex = MediaProfileComboBox.SelectedIndex;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MediaService.Instance.FindDevices();
        }
    }

}

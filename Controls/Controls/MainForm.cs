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

using TestOnvif;

namespace Onvif.Controls
{
    public partial class MainForm : Form, IMediaForm
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

        public void Verbose(string message)
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

        public void BindMediaProfileCollection(TestOnvif.deviceio.Profile[] profiles)
        {
            BindingSource binding = new BindingSource()
            {
                DataSource = profiles,
            };

            this.MediaProfileComboBox.DataSource = binding.DataSource;
            this.MediaProfileComboBox.DisplayMember = "Name";
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            var device = this.MediaDeviceComboBox.SelectedValue as MediaDevice;

            if (device != null)
            {
                MediaServiceController.Controller.Connect(device, "admin", "123456");
            }

        }
        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            MediaServiceController.Controller.Disconnect();
        }

        private void VideoStartButton_Click(object sender, EventArgs e)
        {
            TestOnvif.deviceio.Profile profile = this.MediaProfileComboBox.SelectedValue as TestOnvif.deviceio.Profile;

            if (profile != null)
            {
                MediaServiceController.Controller.Start(profile);
            }
        }

        public void UpdateControls()
        {
            if (MediaServiceController.Controller.IsConnected == true)
            {
                UpdateControls(true);

                if (MediaServiceController.Controller.IsStreaming == true)
                {
                    this.VideoStartButton.Enabled = false;
                    this.MediaProfileComboBox.Enabled = false;

                    this.VideoStopButton.Enabled = true;
                }
                else
                {
                    this.VideoStartButton.Enabled = true;
                    this.VideoStopButton.Enabled = false;
                    this.MediaProfileComboBox.Enabled = true;
                }
            }
            else
            {
                UpdateControls(false);

                this.VideoStartButton.Enabled = false;
                this.VideoStopButton.Enabled = false;
                this.MediaProfileComboBox.Enabled = false;
            }

        }

        private void UpdateControls(bool Enabled)
        {
            this.ConnectButton.Enabled = !Enabled;
            this.DisconnectButton.Enabled = Enabled;
            this.GetConfigurationButton.Enabled = Enabled;
            this.getDeviceInformationButton.Enabled = Enabled;
            this.getHostnameButton.Enabled = Enabled;
            this.GetSystemDateAndTimeButton.Enabled = Enabled;
            this.MediaClientGetProfilesButton.Enabled = Enabled;
            this.RebootButton.Enabled = Enabled;
            this.SetDateTimefromNtpButton.Enabled = Enabled;
            this.setDateTimeButton.Enabled = Enabled;
        }

        private void VideoStopButton_Click(object sender, EventArgs e)
        {
            MediaServiceController.Controller.Stop();
        }


        #region ONVIF

        private void WsDicoveryButton_Click(object sender, EventArgs e)
        {
            MediaServiceController.Controller.ExecuteCommand("WsDicovery");
        }

        private void FindDeviceButton_Click(object sender, EventArgs e)
        {
           // MediaService.Instance.FindDevices();
            MediaServiceController.Controller.FindDevices();
        }

        private void setDateTimeButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("SetDateTime");
        }

        private void SetDateTimefromNtpButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("SetDateTimefromNtp");
        }
        private void RebootButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("Reboot");
        }

        private void getDeviceInformationButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("GetDeviceInformation");
        }

        private void GetSystemDateAndTimeButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("GetSystemDateAndTime");

        }

        private void getHostnameButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("GetHostname");
        }
        #endregion

        private void MediaClientGetProfilesButton_Click(object sender, EventArgs e)
        {
            //MediaService.Instance.ExecuteCommand("MediaClientGetProfiles");
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
            //MediaService.Instance.ExecuteCommand("ShowWebForm");
        }

        private void MediaProfileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(MediaService.Instance.MediaDevice!=null)
            //    MediaService.Instance.MediaDevice.ONVIFClient.MediaProfileIndex = MediaProfileComboBox.SelectedIndex;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //MediaService.Instance.FindDevices();

        }


    }

}

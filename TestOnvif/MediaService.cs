using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace TestOnvif
{
    class MediaService
    {
        private static MediaService service = null;
        public static MediaService Instance
        {
            get
            {
                if (service == null)
                {
                    service = new MediaService();
                }
                return service;
            }
        }

        /// <summary>
        /// Колекция доступных ONVIF устройств
        /// </summary>
        MediaDevice[] mediaDeviceCollection;

        /// <summary>
        /// камера с которой работаем
        /// </summary>
        MediaDevice mediaDevice;

        VideoForm videoForm;

        MainForm mainForm;

        public MediaDevice[] MediaDeviceCollection
        {
            get { return mediaDeviceCollection; }
            set { mediaDeviceCollection = value; }
        }

        public MediaDevice MediaDevice
        {
            get { return mediaDevice; }
            set { mediaDevice = value; }
        }

        public MainForm CreateMainForm()
        {
            mainForm =new MainForm();
            return mainForm;
        }

        public MainForm MainForm
        {
            get
            {
                return mainForm;
            }
        }

        public void FindDevices()
        {
            mediaDeviceCollection = ONVIFClient.GetAvailableMediaDevices();

            mainForm.BindMediaDeviceCollection(mediaDeviceCollection);
        }

        public void Connect(MediaDevice device, string login, string password)
        {
            device.ONVIFClient.Connect(login, password);

            mainForm.BindMediaProfileCollection(device);

            mediaDevice = device;
        }

        public void Disconnect() { }


        public void Start()
        {
            if (mediaDevice != null)
            {
                bool result = mediaDevice.StartMedia();
                if (result == true)
                {
                    string uri = mediaDevice.ONVIFClient.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri;
                    string filename = mediaDevice.AVClient.FFmpegMedia.OutputFilename;

                    int width = mediaDevice.AVClient.InVideoParams.Width;
                    int height = mediaDevice.AVClient.InVideoParams.Height;

                    videoForm = new VideoForm(uri, filename, width, height);

                    mediaDevice.AVClient.ShowVideo += videoForm.ShowVideo;
                    mediaDevice.AVClient.PlayAudio += videoForm.PlayAudio;

                    videoForm.Show();

                    mainForm.UIStartMode();
                }
            }
        }

        public void Stop()
        {
            if (mediaDevice != null)
            {
                mediaDevice.StopMedia();

                if (videoForm != null)
                {
                    mediaDevice.AVClient.ShowVideo -= videoForm.ShowVideo;
                    mediaDevice.AVClient.PlayAudio -= videoForm.PlayAudio;

                    videoForm.Close();
                    videoForm = null;
                };

                mainForm.UIStopMode();
            }
        }

        public bool ExecuteCommand(string command)
        {
            bool result = true;

            switch (command)
            {
                case "GetDeviceInformation":
                    {
                        MessageBox.Show(mediaDevice.ONVIFClient.GetDeviceInformation());
                        break;
                    }
                case "GetSystemDateAndTime":
                    {
                        string nowTimeString = DateTime.Now.ToString("HH:mm:ss.fff");

                        string mediaTimeString = string.Empty;

                        DateTime? media = mediaDevice.ONVIFClient.GetSystemDateAndTime();

                        if (media != null)
                            mediaTimeString = ((DateTime)media).ToString("HH:mm:ss.fff");

                        string message = string.Format("now={0}; media={1}", nowTimeString, mediaTimeString);
                        Logger.Write(message, EnumLoggerType.Output);

                        break;
                    }
                case "SetDateTime":
                    {
                        if (mediaDevice != null)
                        {
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.DoWork += (obj, arg) =>
                            {
                                mediaDevice.ONVIFClient.SetSystemDateAndTime(DateTime.Now);
                            };
                            worker.RunWorkerCompleted += (obj, arg) =>
                            {
                                mainForm.Enabled = true;
                                mainForm.Cursor = Cursors.Default;
                            };
                            worker.RunWorkerAsync();
                            mainForm.Cursor = Cursors.WaitCursor;
                            mainForm.Enabled = false;
                        }
                        break;
                    }
                case "SetDateTimefromNtp":
                    {
                        if (mediaDevice != null)
                        {
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.DoWork += (obj, arg) =>
                            {
                                mediaDevice.ONVIFClient.SetSystemDateAndTimeNTP("192.168.10.251", "UTC-4");
                            };
                            worker.RunWorkerCompleted += (obj, arg) =>
                            {
                                mainForm.Enabled = true;
                                mainForm.Cursor = Cursors.Default;
                            };

                            worker.RunWorkerAsync();
                            Logger.Write(String.Format("SetSystemDateAndTimeNTP"), EnumLoggerType.DebugLog);
                            mainForm.Cursor = Cursors.WaitCursor;
                            mainForm.Enabled = false;

                        }
                        break;
                    }
                case "WsDicovery":
                    {
                        BackgroundWorker worker = new BackgroundWorker();

                        worker.DoWork += (s, arg) =>
                            {
                                mediaDeviceCollection = ONVIFClient.GetAvailableMediaDevices();
                            };

                        worker.RunWorkerCompleted += (s, arg) =>
                            {
                                if (mediaDeviceCollection != null)
                                {
                                    DiscoverForm df = new DiscoverForm(mediaDeviceCollection);
                                    df.ShowDialog();
                                }

                                mainForm.Cursor = Cursors.Default;
                               // WsDicoveryButton.Enabled = true;

                            };

                        worker.RunWorkerAsync();
                        //WsDicoveryButton.Enabled = false;
                        mainForm.Cursor = Cursors.WaitCursor;
                        break;
                    }
                case "GetHostname":
                    {
                        MessageBox.Show(mediaDevice.ONVIFClient.GetHostname());
                        break;
                    }
                case "Reboot":
                        {
                            if (MessageBox.Show("Reboot will take 2 minutes\nYou are sure that want to reboot device now?", "",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                using (RebootForm rf = new RebootForm(mediaDevice.ONVIFClient.Reboot()))
                                {
                                    rf.ShowDialog();
                                }
                            }
                            break;
                        }
                case "ShowWebForm":
                        {
                            WebForm form = new WebForm(new Uri(@"http://192.168.10.203/admin/index.html"));
                            form.Show();
                            break;
                        }
                default :
                    //...
                    break;
            }

            return result;
        }

    }

}

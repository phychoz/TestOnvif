using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Channels;
using FFmpegWrapper;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;

namespace TestOnvif
{
    public abstract class MediaDeviceClient
    {
        private MediaDevice mediaDevice;

        public MediaDevice MediaDevice
        {
            get { return mediaDevice; }
        }

        public MediaDeviceClient(MediaDevice device)
        {
            this.mediaDevice = device;
        }
    }

    public class MediaDevice 
    {
        public MediaDevice(string name, Uri uri)
        {
            MediaDeviceUri = uri;
            DisplayName = name;

            StreamClient = new MediaStreamClient(this);
            onvifClient = new ONVIFClient(this);
            avProcessor = new AVDeviceClient(this);
        }

        private string displayName = string.Empty;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        private Uri mediaDeviceUri;

        private MediaStreamClient StreamClient;
        private ONVIFClient onvifClient;

        private AVDeviceClient avProcessor;

        public AVDeviceClient AVProcessor
        {
            get { return avProcessor; }
            set { avProcessor = value; }
        }

        public MediaStreamClient streamClient
        {
            get { return StreamClient; }
            set { StreamClient = value; }
        }

        public Uri MediaDeviceUri
        {
            get { return mediaDeviceUri; }
            set { mediaDeviceUri = value; }
        }

        public ONVIFClient ONVIFClient
        {
            get { return onvifClient; }
            set { onvifClient = value; }
        }

        public bool StartMedia()
        {
            bool result = false;
            try
            {
                avProcessor.Start();

                StreamClient.AudioDataRecieved += avProcessor.AudioDataRecieved;
                StreamClient.VideoDataRecieved += avProcessor.VideoDataRecieved;

                StreamClient.StartRecieving();

                result = true;
            }
            catch (Exception exception)
            {
                avProcessor.FFmpegMedia = null;

                System.Windows.Forms.MessageBox.Show(exception.Message);
                Logger.Write(exception);
            }

            return result;
        }


        public void StopMedia()
        {
            try
            {
                StreamClient.StopRecieving();

                StreamClient.AudioDataRecieved -= avProcessor.AudioDataRecieved;
                StreamClient.VideoDataRecieved -= avProcessor.VideoDataRecieved;

                avProcessor.FFmpegStop();

                //VideoFormStop();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                Logger.Write(ex);
            }

        }

    }

    public enum MediaType { Video, Audio };

    public class MediaData
    {
        public IntPtr Data { get; set; }
        public int Size { get; set; }
        public uint Time { get; set; }
        public MediaType Type { get; set; }
    }

    public class DeviceInformation
    {
        string hostName = String.Empty;

        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }
        string firmware = String.Empty;

        public string Firmware
        {
            get { return firmware; }
            set { firmware = value; }
        }

        string serial = String.Empty;

        public string Serial
        {
            get { return serial; }
            set { serial = value; }
        }
        string hardware = String.Empty;

        public string Hardware
        {
            get { return hardware; }
            set { hardware = value; }
        }

        string model = String.Empty;

        public string Model
        {
            get { return model; }
            set { model = value; }
        }

    }

}

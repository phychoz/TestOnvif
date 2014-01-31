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

            mediaStreamClient = new MediaStreamClient(this);
            onvifClient = new ONVIFClient(this);
            avProcessorClient = new AVProcessorClient(this);
        }

        private string displayName = string.Empty;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        private Uri mediaDeviceUri;

        private MediaStreamClient mediaStreamClient;
        private ONVIFClient onvifClient;

        private AVProcessorClient avProcessorClient;

        public AVProcessorClient AVProcessor
        {
            get { return avProcessorClient; }
            set { avProcessorClient = value; }
        }

        public MediaStreamClient streamClient
        {
            get { return mediaStreamClient; }
            set { mediaStreamClient = value; }
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

        public bool Start()
        {
            avProcessorClient.Start();
            mediaStreamClient.Start();

            return true;
        }


        public void Stop()
        {
            mediaStreamClient.Stop();
            avProcessorClient.Stop();
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

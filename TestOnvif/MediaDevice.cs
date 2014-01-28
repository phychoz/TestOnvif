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
    public class MediaDevice 
    {
        public MediaDevice(Uri uri)
        {
            MediaDeviceUri = uri;

            rtspClient = new RTSPClient(this);
            onvifClient = new ONVIFClient(this);
            avClient = new AVClient(this);
        }

        private Uri mediaDeviceUri;

        private RTSPClient rtspClient;
        private ONVIFClient onvifClient;

        private AVClient avClient;

        public AVClient AVClient
        {
            get { return avClient; }
            set { avClient = value; }
        }

        public RTSPClient RtspClient
        {
            get { return rtspClient; }
            set { rtspClient = value; }
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


        DateTime startVideoSessionTime;
        uint startVideoSessionTimestamp = 0;

        

        public bool StartMedia()
        {
            bool result = false;
            try
            {
                avClient.Start();

                rtspClient.AudioDataRecieved += avClient.AudioDataRecieved;
                rtspClient.VideoDataRecieved += avClient.VideoDataRecieved;

                rtspClient.StartRecieving();

                result = true;
            }
            catch (Exception exception)
            {
                avClient.FFmpegMedia = null;

                System.Windows.Forms.MessageBox.Show(exception.Message);
                Logger.Write(exception);
            }

            return result;
        }


        public void StopMedia()
        {
            try
            {
                rtspClient.StopRecieving();

                avClient.FFmpegStop();

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


// FFmpegPlayer ffmpegPlayer;

//BlockingCollection<MediaData> audioCollection = new BlockingCollection<MediaData>(30);
//BlockingCollection<MediaData> videoCollection = new BlockingCollection<MediaData>(30);

// AutoResetEvent aEvent;
// public void OpenVideo(string filename)
// {
//     videoForm = new VideoForm(mediaStreamUri.AbsoluteUri, filename, 640, 480);
//     //videoForm.Show();

//     audioCollection = new BlockingCollection<MediaData>(30);
//     videoCollection = new BlockingCollection<MediaData>(30);

//     ffmpegPlayer = new FFmpegWrapper.FFmpegPlayer();


//     //ffmpegPlayer.VideoFrameReceived += ProcessVideoCollection;
//     //ffmpegPlayer.AudioFrameReceived += ProcessAadioCollection;

//     //videoEvent = new AutoResetEvent(true);
//     aEvent = new AutoResetEvent(true);

//     videoWorker = new Thread(PlayVideoFrame);
//     audioWorker = new Thread(PlayAudioFrame);

//     videoWorker.Start();
//     audioWorker.Start();

//     ffmpegPlayer.Open(filename);

//     new Thread(ffmpegPlayer.ProcessFile).Start();



// }

// private void ProcessVideoCollection(IntPtr data, int linesize, int width, int height, int number, uint time, int flag)
// {
//     videoCollection.Add(new MediaData() { Data = data, Size = linesize, Type = MediaType.Video });//, Timeout.Infinite);

// }

// private void ProcessAadioCollection(IntPtr data, int linesize, uint time)
// {
//     audioCollection.Add(new MediaData() { Data = data, Size = linesize, Type = MediaType.Audio });//, Timeout.Infinite);
// }

// private void PlayVideoFrame()
// {

//     while (true)
//     {
//         MediaData item;
//         if (videoCollection.TryTake(out item) == true)
//         {
//             using (Bitmap bitmap = new Bitmap(640, 480, item.Size, PixelFormat.Format24bppRgb, item.Data))
//             {
//                 videoForm.ShowVideo(bitmap);
//                 // bitmap.Save(filename, ImageFormat.Jpeg);
//             }
//         }
//         aEvent.WaitOne(30);
//         //Thread.Sleep(33);

//     }


// }
// private void PlayAudioFrame()
// {
//     while (true)
//     {
//         MediaData item;
//         if (audioCollection.TryTake(out item) == true)
//         {
//             videoForm.PlayAudio(item.Data, item.Size);
//             //audioPlayer.PlayFromMemory(item.Data, item.Size);
//         }

//         //Thread.Sleep(33);
//     }

// }
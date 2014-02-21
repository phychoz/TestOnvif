using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TestOnvif
{
    
    public interface IMediaService 
    {
        bool IsConnected { get;}
        bool IsStreaming { get; }
        object[] FindDevices();
        bool Connect(MediaDevice device, string login, string password);
        void Disconnect();
        bool Start(deviceio.Profile profile);
        void Stop();
        object ExecuteCommand(string command, object[] parameters = null);

        event EventHandler<VideoDataEventArgs> VideoDataReady;
        event EventHandler<AudioDataEventArgs> AudioDataReady;
    }

    public class MediaDataEventArgs : EventArgs
    {
        public uint Timestamp { get; set; }
        public DateTime Time { get; set; }
    }

    public class VideoDataEventArgs : MediaDataEventArgs
    {
        public Bitmap Bitmap { get; set; }
    }
    public class AudioDataEventArgs : MediaDataEventArgs
    {
        public IntPtr Ptr { get; set; }
        public int Size { get; set; }
    }


    public class PresenterThread
    {
        public PresenterThread(MediaDevice device)
        {
            this.device = device;
        }

        private MediaDevice device = null;
        private Thread videoWorker;
        private Thread audioWorker;

        private bool Break = false;

        public void Start()
        {
            videoWorker = new Thread(ProcessVideo);
            audioWorker = new Thread(ProcessAudio);

            videoWorker.Start();
            audioWorker.Start();
        }

        public void Stop()
        {
            Break = true;

            //videoWorker.Abort();
            //audioWorker.Abort();

            
        }
        Stopwatch videoStopWatch = new Stopwatch();
        public void ProcessAudio()
        {
            while (true)
            {

                while (device.AudioBufferIsComplete() == false)
                {
                    AudioFrame frame = device.GetAudioFrame();

                    if (frame != null)
                    {
                        OnAudioFrameReadyEventHandler(frame);
                    }

                    if (Break == true) break;

                    Thread.Sleep(1);
                }

                Thread.Sleep(15);
                //Thread.Yield();

                if (Break == true) break;

            }
        }

        public void ProcessVideo()
        {
            while (true)
            {
                videoStopWatch.Restart();

                while (device.VideoBufferIsComplete() == false)
                {
                    VideoFrame frame = device.GetVideoFrame();

                    if (frame != null)
                    {
                        //Bitmap bitmap = ProcessFrame(frame.Bitmap);

                        //frame.Bitmap = bitmap;
                        OnVideoFrameReadyEventHandler(frame);
                    }

                    if (Break == true) break;

                    Thread.Sleep(1);
                }
                Thread.Sleep(15);
                //Console.WriteLine("Wait");
                //Thread.Yield();
                if (Break == true) break;

                long msec = videoStopWatch.ElapsedMilliseconds;

                //if(msec>0)
                //Console.WriteLine("video = {0},", msec);
            }
        }

        public event EventHandler<VideoFrameRecievedEventArgs> VideoFrameReadyEventHandler;
        public event EventHandler<AudioFrameRecievedEventArgs> AudioFrameReadyEventHandler;

        private void OnVideoFrameReadyEventHandler(VideoFrame frame)
        {
            if (VideoFrameReadyEventHandler != null)
            {
                VideoFrameReadyEventHandler(this, new VideoFrameRecievedEventArgs { VideoFrame = frame });
            }
        }

        private void OnAudioFrameReadyEventHandler(AudioFrame frame)
        {
            if (AudioFrameReadyEventHandler != null)
            {
                AudioFrameReadyEventHandler(this, new AudioFrameRecievedEventArgs { AudioFrame = frame });
            }
        }

        private bool prevLEDStatus = false;
        private bool currLEDStatus = false;

        private Bitmap ProcessFrame(Bitmap bitmap)
        {
            
            Image<Bgr, Byte> frame = new Image<Bgr,byte>(bitmap); //capture.QueryFrame();//RetrieveBgrFrame();
            
            //if (frame == null) return;

            //Image<Gray, Byte> smallGrayFrame = grayFrame.PyrDown();
            //Image<Gray, Byte> smoothedGrayFrame = smallGrayFrame.PyrUp();
            //Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(new Gray(100), new Gray(60));

            Image<Gray, Byte> smooth = frame.SmoothBlur(1, 1).Convert<Gray, Byte>();

            //Image<Gray, Byte> smoothImage = frame.Convert<Gray, Byte>();

            //smoothImage._EqualizeHist();
            //smoothImage._GammaCorrect(10.0d);

            Image<Gray, Byte> threshold = smooth.ThresholdBinary(new Gray(240), new Gray(255));

            Image<Gray, Byte> canny = threshold.Canny(new Gray(255), new Gray(255));

            Contour<Point> largestContour = null;
            double largestArea = 0;
            currLEDStatus = false;

            for (Contour<System.Drawing.Point> contours = canny.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contours != null; contours = contours.HNext)
            {
                Point pt = new Point(contours.BoundingRectangle.X, contours.BoundingRectangle.Y);
                //draw.Draw(new CircleF(pt, 5), new Gray(255), 0);

                if (contours.Area > largestArea)
                {
                    largestArea = contours.Area;
                    largestContour = contours;
                }

                canny.Draw(contours.BoundingRectangle, new Gray(255), 1);
                //frame.Draw(contours.BoundingRectangle, new Bgr(Color.Red), 3);

                if (largestContour != null)
                {
                    frame.Draw(largestContour.BoundingRectangle, new Bgr(Color.Red), 2);

                    currLEDStatus = true;

                    string message = string.Format("{0}, Area={1}", "LargestContour", largestContour.Area);

                    //OnVerbose(message);

                }
            }

            if (currLEDStatus != prevLEDStatus)
            {
                //string message = String.Format("LED Status = {0}", currLEDStatus ? "ON" : "OFF");

                if (currLEDStatus == true)
                {
                    string message = string.Format("{0:HH:mm:ss.fff}", DateTime.Now);
                    Logger.Write(message, EnumLoggerType.Output);
                }
                //OnVerbose(message);

                //OnLEDStatusChanged(currLEDStatus);
            }

            prevLEDStatus = currLEDStatus;

            //OnImageProcessed(frame, smooth, threshold, canny);

            //form.DrawFrame(frame);

            return frame.Bitmap;
        }

    }

    public class MediaService : IMediaService, IDisposable
    {
        /// <summary>
        /// Колекция доступных ONVIF устройств
        /// </summary>
        MediaDevice[] mediaDeviceCollection;

        /// <summary>
        /// камера с которой работаем
        /// </summary>
         MediaDevice mediaDevice;


        public MediaDevice[] MediaDeviceCollection
        {
            get { return mediaDeviceCollection; }
            private set { mediaDeviceCollection = value; }
        }

        public MediaDevice MediaDevice
        {
            get { return mediaDevice; }
            private set { mediaDevice = value; }
        }

        private PresenterThread presenter;

        private bool isConnected = false;
        private bool isStreaming = false;

        public bool IsConnected
        {
            get { return isConnected; }
            private set { isConnected = value; }
        }

        public bool IsStreaming
        {
            get { return isStreaming; }
            private set { isStreaming = value; }
        }

        public object[] FindDevices()
        {
            mediaDeviceCollection = ONVIFAgent.GetAvailableMediaDevices();

            return mediaDeviceCollection;
        }

        public event EventHandler<VideoDataEventArgs> VideoDataReady;
        public event EventHandler<AudioDataEventArgs> AudioDataReady;

        private void OnVideoDataReady(Bitmap bitmap)
        {
            if (VideoDataReady != null)
                VideoDataReady(this, new VideoDataEventArgs {Bitmap=bitmap });
        }

        private void OnAudioDataReady(IntPtr ptr, int size)
        {
            if (AudioDataReady != null)
                AudioDataReady(this, new AudioDataEventArgs {Ptr=ptr, Size=size });
        }

        public bool Connect(MediaDevice device, string login, string password)
        {
            if (isConnected == false)
            {
                device.ONVIF.Connect(login, password);

                mediaDevice = device;

                isConnected = true;

                OnConnected();

            }

            return isConnected;

        }

        public void Disconnect() 
        {
            if (isConnected == true)
            {
                if (isStreaming == true)
                {
                    //...
                    Stop();
                    isConnected = false;
                    OnDisonnected();
                }
                else
                {
                    //...
                    isConnected = false;
                    OnDisonnected();
                }
                //...
            }

        }


        public bool Start(deviceio.Profile profile)
        {
            if (isConnected == true && isStreaming == false)
            {
                mediaDevice.ONVIF.CurrentMediaProfile = profile;
                presenter = new PresenterThread(mediaDevice);
                
                presenter.VideoFrameReadyEventHandler+=new EventHandler<VideoFrameRecievedEventArgs>(presenter_VideoFrameReadyEventHandler);
                presenter.AudioFrameReadyEventHandler+=new EventHandler<AudioFrameRecievedEventArgs>(presenter_AudioFrameReadyEventHandler);

                
                isStreaming = mediaDevice.Start();

                presenter.Start();
               
                
                //mediaDevice.Decoder.AudioFrameRecievedEventHandler+=new EventHandler<AudioFrameRecievedEventArgs>(Decoder_AudioFrameRecievedEventHandler);
                //mediaDevice.Decoder.VideoFrameRecievedEventHandler+=new EventHandler<VideoFrameRecievedEventArgs>(Decoder_VideoFrameRecievedEventHandler);

                //mediaDevice.AVProcessor.ShowVideo += AVProcessor_ShowVideo;
                //mediaDevice.AVProcessor.PlayAudio += AVProcessor_PlayAudio;

                OnCaptureStarted();
            }

            return isStreaming;
        }
        private void presenter_VideoFrameReadyEventHandler(object sender, VideoFrameRecievedEventArgs e)
        {
            OnVideoDataReady(e.VideoFrame.Bitmap);
            
        }

        private void presenter_AudioFrameReadyEventHandler(object sender, AudioFrameRecievedEventArgs e)
        {
            OnAudioDataReady(e.AudioFrame.AudioSample.Ptr, e.AudioFrame.AudioSample.Size);
        }

        private void AVProcessor_ShowVideo (Bitmap bitmap)
        {
            OnVideoDataReady(bitmap);
        }

        private void AVProcessor_PlayAudio(IntPtr ptr, int size)
        {
            OnAudioDataReady(ptr, size);
        }

        public void Stop()
        {
            if (isConnected == true && isStreaming == true)
            {
                if (presenter != null)
                {
                    presenter.VideoFrameReadyEventHandler -= new EventHandler<VideoFrameRecievedEventArgs>(presenter_VideoFrameReadyEventHandler);
                    presenter.AudioFrameReadyEventHandler -= new EventHandler<AudioFrameRecievedEventArgs>(presenter_AudioFrameReadyEventHandler);

                    presenter.Stop();
                }
                mediaDevice.Stop();
                isStreaming = false;


                //mediaDevice.Decoder.AudioFrameRecievedEventHandler -= new EventHandler<AudioFrameRecievedEventArgs>(Decoder_AudioFrameRecievedEventHandler);
                //mediaDevice.Decoder.VideoFrameRecievedEventHandler -= new EventHandler<VideoFrameRecievedEventArgs>(Decoder_VideoFrameRecievedEventHandler);

                //mediaDevice.AVProcessor.ShowVideo -= AVProcessor_ShowVideo;
                //mediaDevice.AVProcessor.PlayAudio -= AVProcessor_PlayAudio;

                OnCaptureStoped();               
            }
        }

        public object ExecuteCommand(string command, object [] parameters=null)
        {
            return true;
        }

        public void Dispose()
        {
            //...
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler CaptureStarted;
        public event EventHandler CaptureStoped;

        private void OnConnected()
        {
            if (Connected != null)
                Connected(this, new EventArgs());
        }

        private void OnDisonnected()
        {
            if (Disconnected != null)
                Disconnected(this, new EventArgs());
        }

        private void OnCaptureStarted()
        {
            if (CaptureStarted != null)
                CaptureStarted(this, new EventArgs());
        }

        private void OnCaptureStoped()
        {
            if (CaptureStoped != null)
                CaptureStoped(this, new EventArgs());
        }

    }

}

//if (IsConnected == true)
//{
//    switch (command)
//    {
//        case "GetDeviceInformation":
//            {
//                MessageBox.Show(mediaDevice.ONVIFClient.GetDeviceInformation());
//                break;
//            }
//        case "GetSystemDateAndTime":
//            {
//                string nowTimeString = DateTime.Now.ToString("HH:mm:ss.fff");

//                string mediaTimeString = string.Empty;

//                DateTime? media = mediaDevice.ONVIFClient.GetSystemDateAndTime();

//                if (media != null)
//                    mediaTimeString = ((DateTime)media).ToString("HH:mm:ss.fff");

//                string message = string.Format("now={0}; media={1}", nowTimeString, mediaTimeString);
//                Logger.Write(message, EnumLoggerType.Output);

//                break;
//            }
//        case "SetDateTime":
//            {
//                if (mediaDevice != null)
//                {
//                    BackgroundWorker worker = new BackgroundWorker();
//                    worker.DoWork += (obj, arg) =>
//                    {
//                        mediaDevice.ONVIFClient.SetSystemDateAndTime(DateTime.Now);
//                    };
//                    worker.RunWorkerCompleted += (obj, arg) =>
//                    {
//                        mainForm.Enabled = true;
//                        mainForm.Cursor = Cursors.Default;
//                    };
//                    worker.RunWorkerAsync();
//                    mainForm.Cursor = Cursors.WaitCursor;
//                    mainForm.Enabled = false;
//                }
//                break;
//            }
//        case "SetDateTimefromNtp":
//            {
//                if (mediaDevice != null)
//                {
//                    BackgroundWorker worker = new BackgroundWorker();
//                    worker.DoWork += (obj, arg) =>
//                    {
//                        mediaDevice.ONVIFClient.SetSystemDateAndTimeNTP("192.168.10.251", "UTC-4");
//                    };
//                    worker.RunWorkerCompleted += (obj, arg) =>
//                    {
//                        mainForm.Enabled = true;
//                        mainForm.Cursor = Cursors.Default;
//                    };

//                    worker.RunWorkerAsync();
//                    Logger.Write(String.Format("SetSystemDateAndTimeNTP"), EnumLoggerType.DebugLog);
//                    mainForm.Cursor = Cursors.WaitCursor;
//                    mainForm.Enabled = false;

//                }
//                break;
//            }
//        case "MediaClientGetProfiles":
//            {
//                MediaClientProfilesForm mcpf = new MediaClientProfilesForm(mediaDevice.ONVIFClient.MediaProfiles);
//                mcpf.ShowDialog();
//                break;
//            }
//        case "GetHostname":
//            {
//                MessageBox.Show(mediaDevice.ONVIFClient.GetHostname());
//                break;
//            }
//        case "Reboot":
//            {
//                if (MessageBox.Show("Reboot will take 2 minutes\nYou are sure that want to reboot device now?", "",
//                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
//                {
//                    string message = mediaDevice.ONVIFClient.Reboot();
//                    using (RebootForm rf = new RebootForm(message))
//                    {
//                        //
//                        rf.ShowDialog();
//                    }
//                }
//                break;
//            }
//        default:
//            //...
//            break;
//    }
//}

//switch (command)
//{
//    case "WsDicovery":
//        {
//            BackgroundWorker worker = new BackgroundWorker();

//            worker.DoWork += (s, arg) =>
//            {
//                mediaDeviceCollection = ONVIFClient.GetAvailableMediaDevices();
//            };

//            worker.RunWorkerCompleted += (s, arg) =>
//            {
//                if (mediaDeviceCollection != null)
//                {
//                    DiscoverForm df = new DiscoverForm();
//                    df.ShowDialog();
//                }

//                mainForm.Cursor = Cursors.Default;
//                // WsDicoveryButton.Enabled = true;

//            };

//            worker.RunWorkerAsync();
//            //WsDicoveryButton.Enabled = false;
//            mainForm.Cursor = Cursors.WaitCursor;
//            break;
//        }
//    case "ShowWebForm":
//        {
//            WebForm form = new WebForm(new Uri(@"http://192.168.10.203/admin/index.html"));
//            form.Show();
//            break;
//        }
//    default:
//        //...
//        break;
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    class RTSPChannel
    {
        private UnicastUdpClient unicastUdpClient;
        private RtpPacketHandler rtpPacketHandler;
        private RtcpReporter rtcpReporter;
        private RFCHandler rfcHandler;

        private int rtpPort;
        private int rtcpPort;

        public void Init(int rtpPort, int rtcpPort, string encoder, MediaType type)
        {
            this.rtpPort = rtpPort;
            this.rtcpPort = rtcpPort;

            unicastUdpClient = new UnicastUdpClient(rtpPort);
            rtpPacketHandler = new RtpPacketHandler();

            unicastUdpClient.UdpPacketRecived += rtpPacketHandler.HandleRtpPacket;
            rfcHandler = RFCHandlerFactory.Create(encoder);

            rtpPacketHandler.RtpPacketRecieved += rfcHandler.HandleRtpPacket;
            rfcHandler.FrameReceived += rfcHandler_FrameRecived;

            rtcpReporter = new RtcpReporter(rtcpPort, type);
            rtpPacketHandler.RtpPacketRecieved += rtcpReporter.HandleRtpPacket;
            rtcpReporter.RtpTimeReporting += videoRtcpClient_OnRtpTimeReporting;

            rtcpReporter.SessionTimeCorrecting += videoRtcpClient_OnRtpTimeCorrecting;
        }

        public void StartRecieving()
        {
            rtcpReporter.StartReporting();
            unicastUdpClient.StartReceiving();
        }

        public void StopRecieving()
        {
            rtcpReporter.StopReporting();
            unicastUdpClient.StopReceiving();
        }

        private void rfcHandler_FrameRecived(IntPtr ptr, int size, bool key, uint pts)
        {
            if (DataRecieved != null)
            {
                DataRecieved(ptr, size, key, pts);
            }
        }

        private void videoRtcpClient_OnRtpTimeReporting(DateTime time)
        {
            //...
        }
        void videoRtcpClient_OnRtpTimeCorrecting(uint timestamp, DateTime time)
        {
            //...
        }

        private void OnVideoDataRecieved(IntPtr ptr, int size, bool key, uint pts)
        {
            if (DataRecieved != null)
            {
                DataRecieved(ptr, size, key, pts);
            }
        }

        public event DataRecievedHandler DataRecieved;

        public delegate void DataRecievedHandler(IntPtr ptr, int size, bool key, uint pts);

    }

    public class MediaStreamClient : MediaDeviceClient, IDisposable
    {
        public MediaStreamClient(MediaDevice device) : base(device) { }

        private Uri mediaStreamUri;

        public Uri MediaStreamUri
        {
            get { return mediaStreamUri; }
            set { mediaStreamUri = value; }
        }

        private RTSPSession rtspSession;

        RTSPChannel videoChannel;

        internal RTSPChannel VideoChannel
        {
            get { return videoChannel; }
            set { videoChannel = value; }
        }
        RTSPChannel audioChannel;

        internal RTSPChannel AudioChannel
        {
            get { return audioChannel; }
            set { audioChannel = value; }
        }


        //private UnicastUdpClient videoUdpClient;
        //private UnicastUdpClient audioUdpClient;

        //private RtpPacketHandler audioRtpHandler;
        //private RtpPacketHandler videoRtpHandler;

        //private RtcpReporter videoRtcpReporter;
        //private RtcpReporter audioRtcpReporter;

        //private RFCHandler videoRfcHandler;
        //private RFCHandler audioRfcHandler;

        public void InitMedia(string VideoEncoding, string AudioEncoding)
        {
            //InitVideo(VideoEncoding);

            //InitAudio(AudioEncoding);

            videoChannel = new RTSPChannel();
            videoChannel.Init(videoRtpPort, videoRtcpPort, VideoEncoding, MediaType.Video);

            audioChannel = new RTSPChannel();
            videoChannel.Init(audioRtpPort, audioRtcpPort, AudioEncoding, MediaType.Audio);
        }

        //private void InitVideo(string VideoEncoding)
        //{
        //    videoUdpClient = new UnicastUdpClient(videoRtpPort);
        //    videoRtpHandler = new RtpPacketHandler();

        //    videoUdpClient.UdpPacketRecived += videoRtpHandler.HandleRtpPacket;
        //    videoRfcHandler = RFCHandlerFactory.Create(VideoEncoding);

        //    videoRtpHandler.RtpPacketRecieved += videoRfcHandler.HandleRtpPacket;
        //    videoRfcHandler.FrameReceived += videoHandler_FrameRecived;

        //    videoRtcpReporter = new RtcpReporter(videoRtcpPort, MediaType.Video);
        //    videoRtpHandler.RtpPacketRecieved += videoRtcpReporter.HandleRtpPacket;
        //    videoRtcpReporter.RtpTimeReporting += videoRtcpClient_OnRtpTimeReporting;

        //    videoRtcpReporter.SessionTimeCorrecting += videoRtcpClient_OnRtpTimeCorrecting;
        //}

        //private void InitAudio(string AudioEncoding)
        //{
        //    audioUdpClient = new UnicastUdpClient(audioRtpPort);
        //    audioRtpHandler = new RtpPacketHandler();

        //    audioUdpClient.UdpPacketRecived += audioRtpHandler.HandleRtpPacket;

        //    audioRfcHandler = RFCHandlerFactory.Create(AudioEncoding);

        //    audioRtpHandler.RtpPacketRecieved += audioRfcHandler.HandleRtpPacket;
        //    audioRfcHandler.FrameReceived += audioHandler_FrameRecived;

        //    audioRtcpReporter = new RtcpReporter(audioRtcpPort, MediaType.Audio);

        //    // audioRtcpReporter.RtpTimeReporting += audioRtcpClient_OnRtpTimeReporting;
        //    audioRtpHandler.RtpPacketRecieved += audioRtcpReporter.HandleRtpPacket;
        //}

        public void StartRecieving()
        {
            RtspStart();


            audioChannel.DataRecieved += MediaDevice.AVProcessor.AudioDataRecieved;
            videoChannel.DataRecieved += MediaDevice.AVProcessor.VideoDataRecieved;

            videoChannel.StartRecieving();
            audioChannel.StartRecieving();
            //videoRtcpReporter.StartReporting();
            //audioRtcpReporter.StartReporting();

            //videoUdpClient.StartReceiving();
            //audioUdpClient.StartReceiving();

        }

        public void StopRecieving()
        {
            RtspStop();
            audioChannel.DataRecieved -= MediaDevice.AVProcessor.AudioDataRecieved;
            videoChannel.DataRecieved -= MediaDevice.AVProcessor.VideoDataRecieved;

            videoChannel.StopRecieving();
            audioChannel.StopRecieving();

            //videoUdpClient.StopReceiving();
            //audioUdpClient.StopReceiving();

            //videoRtcpReporter.StopReporting();
            //audioRtcpReporter.StopReporting();

        }


        public void RtspStart()
        { 
            rtspSession = RTSPSession.Open(this.MediaDevice.ONVIFClient.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri);
            rtspSession.RTSPServerResponse += new RTSPSession.RTSPResponseHandler(rtsp_RTSPServerResponse);

            int[] ports = GetPortRange(4);

            videoRtpPort = ports[0];
            videoRtcpPort = ports[1];
            audioRtpPort = ports[2];
            audioRtcpPort = ports[3];


            rtspSession.StartTranslation(videoRtpPort, videoRtcpPort, audioRtpPort, audioRtcpPort);
        }

        private void RtspStop()
        {
            if (rtspSession != null)
            {
                rtspSession.Teardown();
                rtspSession.Close();
            }
        }
        void rtsp_RTSPServerResponse(string command)
        {
            if (command == "play")
            {
                InitMedia(RTSPSession.Codec, "G711");
            }
        }

        int videoRtpPort;
        int audioRtpPort;

        int videoRtcpPort;
        int audioRtcpPort;

        const int MIN_PORT = 49152;
        const int MAX_PORT = 65535;

        /// <summary>
        /// Получаем свободные порты
        /// </summary>
        private int[] GetPortRange(int count)
        {
            int[] ports = new int[count];
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            var free = Enumerable.Range(MIN_PORT, MAX_PORT - MIN_PORT + 1)
                                                .Select(port => port)
                                                .Except(properties.GetActiveUdpListeners()
                                                    .Where(listener => listener.Port >= MIN_PORT && listener.Port <= MAX_PORT)
                                                    .Select(listener => listener.Port));

            Stack<int> freePorts = new Stack<int>(free);


            if (freePorts.Count < count) return null;

            for (int index = 0; index < count; index++)
                ports[index] = freePorts.Pop();

            return ports;
        }

        //void videoHandler_FrameRecived(IntPtr ptr, int size, bool key, uint pts)
        //{
        //    if (VideoDataRecieved != null)
        //    {
        //        VideoDataRecieved(ptr, size, key, pts);
        //    }
        //}

        //void audioHandler_FrameRecived(IntPtr ptr, int size, bool key, uint pts)
        //{
        //    if (AudioDataRecieved != null)
        //    {
        //        AudioDataRecieved(ptr, size, false, pts);
        //    }
        //}

        //public event VideoDataRecievedHandler VideoDataRecieved;
        //public event AudioDataRecievedHandler AudioDataRecieved;

        //public delegate void VideoDataRecievedHandler(IntPtr ptr, int size, bool key, uint pts);
        //public delegate void AudioDataRecievedHandler(IntPtr ptr, int size, bool key, uint pts);


        //void videoRtcpClient_OnRtpTimeCorrecting(uint timestamp, DateTime time)
        //{
        //    //startVideoSessionTime = time;
        //    //startVideoSessionTimestamp = timestamp;
        //}

        //void videoRtcpClient_OnRtpTimeReporting(DateTime time)
        //{
        //    ////Logger.Write(time.ToString("HH:mm:ss.fff"), EnumLoggerType.DebugLog);
        //    //if (videoClockForm != null)
        //    //{
        //    //    //videoForm.UpdateCapture(time.ToString("HH:mm:ss.fff"));
        //    //    videoClockForm.UpdateLabel(time.ToString("HH:mm:ss.fff"));
        //    //}
        //}

        //void audioRtcpClient_OnRtpTimeReporting(DateTime time)
        //{
        //    //if (audioClockForm != null)
        //    //{
        //    //    audioClockForm.UpdateLabel(time.ToString("HH:mm:ss.fff"));
        //    //}

        //}

        public void Dispose()
        {
            if (rtspSession != null)
            {
                rtspSession.Dispose();
                rtspSession = null;
            }
        }

    }
}

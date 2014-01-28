using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    public class RTSPClient:MediaClient
    {
        public RTSPClient(MediaDevice device) : base(device) { }

        Uri mediaStreamUri;

        public Uri MediaStreamUri
        {
            get { return mediaStreamUri; }
            set { mediaStreamUri = value; }
        }

        RTSPSession rtsp;

        UnicastUdpClient videoUdpClient;
        UnicastUdpClient audioUdpClient;

        RtpPacketHandler audioRtpHandler;
        RtpPacketHandler videoRtpHandler;

        RtcpReporter videoRtcpReporter;
        RtcpReporter audioRtcpReporter;

        RFC2435Handler jpegHandler;
        RFC3984Handler h264Handler;
        RFC3016Handler mpeg4Handler;

        public void MediaInit(string VideoEncoding)
        {

            //int[] ports = GetPortRange(4);

            //videoRtpPort = ports[0];
            videoUdpClient = new UnicastUdpClient(videoRtpPort);
            videoUdpClient.UdpPacketRecived += videoClient_UdpPacketRecieved;
            videoRtpHandler = new RtpPacketHandler();

            switch (VideoEncoding)
            {
                case "JPEG":
                    jpegHandler = new RFC2435Handler();
                    videoRtpHandler.RtpPacketRecieved += jpegHandler.HandleRtpPacket; //rtpVideoHandler_RtpPacketRecieved;
                    jpegHandler.JpegFrameReceived += videoHandler_FrameRecived;

                    break;

                case "MP4V-ES":
                    mpeg4Handler = new RFC3016Handler();
                    videoRtpHandler.RtpPacketRecieved += mpeg4Handler.HandleRtpPacket;
                    mpeg4Handler.Mpeg4FrameReceived += videoHandler_FrameRecived;

                    break;

                case "H264":
                    h264Handler = new RFC3984Handler();
                    videoRtpHandler.RtpPacketRecieved += h264Handler.HandleRtpPacket;
                    h264Handler.H264FrameReceived += videoHandler_FrameRecived;

                    break;
            }

            //videoRtcpPort = ports[1];
            videoRtcpReporter = new RtcpReporter(videoRtcpPort, MediaType.Video);
            videoRtpHandler.RtpPacketRecieved += videoRtcpReporter.HandleRtpPacket;
            videoRtcpReporter.RtpTimeReporting += videoRtcpClient_OnRtpTimeReporting;

            videoRtcpReporter.SessionTimeCorrecting += videoRtcpClient_OnRtpTimeCorrecting;

            //audioRtpPort = ports[2];
            audioUdpClient = new UnicastUdpClient(audioRtpPort);
            audioUdpClient.UdpPacketRecived += audioClient_UdpPacketReceived;
            audioRtpHandler = new RtpPacketHandler();
            audioRtpHandler.RtpPacketRecieved += rtpAudioHandler_RtpPacketRecieved;

            //audioRtcpPort = ports[3];
            audioRtcpReporter = new RtcpReporter(audioRtcpPort, MediaType.Audio);

            // audioRtcpReporter.RtpTimeReporting += audioRtcpClient_OnRtpTimeReporting;
            audioRtpHandler.RtpPacketRecieved += audioRtcpReporter.HandleRtpPacket;

            //mediaStreamUri = GetStreamUri();

        }

        public void StartRecieving()
        {
            RtspStart();

            videoRtcpReporter.StartReporting();
            audioRtcpReporter.StartReporting();

            videoUdpClient.StartReceiving();
            audioUdpClient.StartReceiving();

        }

        public void StopRecieving()
        {
            RtspStop();

            videoUdpClient.StopReceiving();
            audioUdpClient.StopReceiving();

            videoRtcpReporter.StopReporting();
            audioRtcpReporter.StopReporting();

        }


        public void RtspStart()
        { 
            rtsp = RTSPSession.Open(this.MediaDevice.ONVIFClient.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri);
            rtsp.RTSPServerResponse += new RTSPSession.RTSPResponseHandler(rtsp_RTSPServerResponse);

            int[] ports = GetPortRange(4);

            videoRtpPort = ports[0];
            videoRtcpPort = ports[1];
            audioRtpPort = ports[2];
            audioRtcpPort = ports[3];

            rtsp.StartTranslation(videoRtpPort, videoRtcpPort, audioRtpPort, audioRtcpPort);
        }

        private void RtspStop()
        {
            if (rtsp != null)
            {
                rtsp.Teardown();
                rtsp.Close();
            }
        }
        void rtsp_RTSPServerResponse(string command)
        {
            if (command == "play")
            {
                MediaInit(RTSPSession.Codec);
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


        void audioClient_UdpPacketReceived(IPEndPoint remoteEndPoint, IntPtr data, int count)
        {

            audioRtpHandler.HandleRtpPacket(data, count);
        }



        void videoClient_UdpPacketRecieved(IPEndPoint remoteEndPoint, IntPtr data, int count)
        {

            videoRtpHandler.HandleRtpPacket(data, count);
        }


        void videoHandler_FrameRecived(IntPtr ptr, int size, bool key, uint pts)
        {
            if (VideoDataRecieved != null)
            {
                VideoDataRecieved(ptr, size, key, pts);
            }
        }

        void rtpAudioHandler_RtpPacketRecieved(RtpPacket packet)
        {
            if (AudioDataRecieved != null)
            {
                AudioDataRecieved(packet.Payload, packet.PayloadLength, false, packet.Timestamp);
            }
        }

        public event VideoDataRecievedHandler VideoDataRecieved;
        public event AudioDataRecievedHandler AudioDataRecieved;

        public delegate void VideoDataRecievedHandler(IntPtr ptr, int size, bool key, uint pts);
        public delegate void AudioDataRecievedHandler(IntPtr ptr, int size, bool key, uint pts);


        void videoRtcpClient_OnRtpTimeCorrecting(uint timestamp, DateTime time)
        {
            //startVideoSessionTime = time;
            //startVideoSessionTimestamp = timestamp;
        }

        void videoRtcpClient_OnRtpTimeReporting(DateTime time)
        {
            ////Logger.Write(time.ToString("HH:mm:ss.fff"), EnumLoggerType.DebugLog);
            //if (videoClockForm != null)
            //{
            //    //videoForm.UpdateCapture(time.ToString("HH:mm:ss.fff"));
            //    videoClockForm.UpdateLabel(time.ToString("HH:mm:ss.fff"));
            //}
        }

        void audioRtcpClient_OnRtpTimeReporting(DateTime time)
        {
            //if (audioClockForm != null)
            //{
            //    audioClockForm.UpdateLabel(time.ToString("HH:mm:ss.fff"));
            //}

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    class RTSPChannelParameters
    {
        public int RTPPort { get; set; }

        public int RTCPPort { get; set; }

        public uint SSRT { get; set; }

        public uint RtpTime { get; set; }

        public string Codec { get; set; }

        public int SampleRate { get; set; }
    }

    class RTSPChannel
    {
        private UnicastUdpClient unicastUdpClient;
        private RtpPacketHandler rtpPacketHandler;
        private PayloadHandler payloadHandler;

        private RtcpReporter rtcpReporter;

        public RTSPChannelParameters Parameters { get; set; }

        public RTSPChannel(RTSPChannelParameters parameters)
        {
            Parameters = parameters;

            unicastUdpClient = new UnicastUdpClient(Parameters.RTPPort);
            rtpPacketHandler = new RtpPacketHandler(parameters.SSRT);

            unicastUdpClient.UdpPacketRecived += rtpPacketHandler.HandleRtpPacket;
            payloadHandler = PayloadHandlerFactory.Create(Parameters.Codec);

            rtpPacketHandler.RtpPacketRecieved += payloadHandler.HandleRtpPacket;
            payloadHandler.FrameReceived += PayloadHandler_FrameRecived;

            rtcpReporter = new RtcpReporter(Parameters.RTCPPort, Parameters.SampleRate);

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

        private void PayloadHandler_FrameRecived(IntPtr ptr, int size, bool key, uint pts)
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
}

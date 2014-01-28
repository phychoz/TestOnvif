using System;
using System.Collections.Generic;
using System.Linq;

namespace TestOnvif
{
    class RTSPRequest
    {
        private RTSPRequest() { }

        public static RTSPRequest CreateOptions(string url, int CSeq)
        {
            return new RTSPRequest()
            {
                Method = "OPTIONS",
                URL = url,
                CSeq = CSeq,
                Accept = null,
                Transport = null,
                Session = null,
                Range = null
            };
        }

        public static RTSPRequest CreateDescribe(string url, int CSeq)
        {
            return new RTSPRequest()
            {
                Method = "DESCRIBE",
                URL = url,
                CSeq = CSeq,
                Accept = "application/sdp",
                Transport = null,
                Session = null,
                Range = null
            };
        }

        public static RTSPRequest CreateSetup(string url, int CSeq, int RTPclientPort, int RTCPclientPort, string session)
        {
            return new RTSPRequest()
            {
                Method = "SETUP",
                URL = url,
                CSeq = CSeq,
                Accept = null,
                Transport = String.Format("RTP/AVP/UDP;unicast;client_port={0}-{1}", RTPclientPort, RTCPclientPort),
                Session = session,
                Range = null
            };
        }

        public static RTSPRequest CreateSetup(string url, int CSeq, int RTPclientPort, int RTCPclientPort)
        {
            return new RTSPRequest()
            {
                Method = "SETUP",
                URL = url,
                CSeq = CSeq,
                Accept = null,
                Transport = String.Format("RTP/AVP/UDP;unicast;client_port={0}-{1}", RTPclientPort, RTCPclientPort),
                Session = null,
                Range = null
            };
        }

        public static RTSPRequest CreatePlay(string url, int CSeq, string session)
        {
            return new RTSPRequest()
            {
                Method = "PLAY",
                URL = url,
                CSeq = CSeq,
                Accept = null,
                Transport = null,
                Session = session,
                Range = "npt=0.000-"
            };
        }

        public static RTSPRequest CreateTeardown(string url, int CSeq, string session)
        {
            return new RTSPRequest()
            {
                Method = "TEARDOWN",
                URL = url,
                CSeq = CSeq,
                Accept = null,
                Transport = null,
                Session = session,
                Range = null
            };
        }

        public override string ToString()
        {
            string data = String.Format("{0} {1} RTSP/1.0\r\n", Method, URL);
            if (Accept != null)
                data += String.Format("Accept: {0}\r\n", Accept);
            if (Transport != null)
                data += String.Format("Transport: {0}\r\n", Transport);
            if (Session != null)
                data += String.Format("Session: {0}\r\n", Session);
            if (Range != null)
                data += String.Format("Range: {0}\r\n", Range);
            data += String.Format("CSeq: {0}\r\n", CSeq);
            data += Environment.NewLine;
            return data;
        }

        public string URL { get; private set; }

        public string Method { get; private set; }

        public int CSeq { get; private set; }

        public string Accept { get; private set; }

        public string Session { get; private set; }

        public string Transport { get; private set; }

        public string Range { get; private set; }
    }
}

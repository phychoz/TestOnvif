using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace TestOnvif
{
    class RTSPSession : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private const int RTSP_PORT = 554;

        private Uri uri;
        private int CSeq = 1;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private string session;

        static private uint videoSSRT;
        static private uint audioSSRT;

        static private uint videoRtpTime;
        static private uint audioRtpTime;

        public static string Codec = string.Empty;

        static public bool IsCurrentSessionSSRT(uint ssrt)
        {
            bool result = false;
            if (ssrt == videoSSRT || ssrt == audioSSRT)
            {
                result = true;
            }
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (networkStream != null)
                {
                    networkStream.Dispose();
                    networkStream = null;
                }
        }
        ~RTSPSession()
        {
            Dispose(false);
        }
        private RTSPSession() { }

        public static RTSPSession Open(string url)
        {
            Uri uri = new Uri(url);
            TcpClient client = new TcpClient(uri.DnsSafeHost, RTSP_PORT);
            return new RTSPSession()
            {
                uri = new Uri(url),
                tcpClient = client,
                networkStream = client.GetStream()
            };
        }

        public void Close()
        {
            if (tcpClient != null)
            {
                networkStream.Close();
                tcpClient.Close();
                tcpClient = null;
            }
        }

        public void StartTranslation(int RtpVideoPort, int RtcpVideoPort, int RtpAudioPort, int RtcpAudioPort)
        {
            RTSPResponse respons;

            // OPTIONS возвращает команды сервера
            // OPTIONS, DESCRIBE, SETUP, PLAY, PAUSE, GET_PARAMETER, TEARDOWN, SET_PARAMETER
            respons = Send(RTSPRequest.CreateOptions(uri.AbsoluteUri, CSeq));

            OnRTSPServerResponse("OPTIONS");

            // DESCRIBE возвращает SDP файл 
            respons = Send(RTSPRequest.CreateDescribe(uri.AbsoluteUri, CSeq));

            string ContentBase = respons.ContentBase;

            // Парсим SDP пакет
            SDP sdp = SDP.Parse(respons.Body);

            MediaDescription VideoMediaDescription = sdp.Session.GetMediaDescriptionByName("video");

            string VideoControl = VideoMediaDescription.GetAttributeValueByName("control");

            MediaDescription AudioMediaDescription = sdp.Session.GetMediaDescriptionByName("audio");

            string AudioControl = AudioMediaDescription.GetAttributeValueByName("control");

            string VideoSetupUri = String.Format("{0}{1}", ContentBase, VideoControl);
            string AudioSetupUri = String.Format("{0}{1}", ContentBase, AudioControl);


            respons = Send(RTSPRequest.CreateSetup(VideoSetupUri, CSeq, RtpVideoPort, RtcpVideoPort));

            session = respons.Session;

            videoSSRT = respons.SSRT;

            respons = Send(RTSPRequest.CreateSetup(AudioSetupUri, CSeq, RtpAudioPort, RtcpAudioPort, session));
            audioSSRT = respons.SSRT;

            respons = Send(RTSPRequest.CreatePlay(uri.AbsoluteUri, CSeq, session));

            videoRtpTime = respons.RtpTracks[0].RTPTime;
            audioRtpTime = respons.RtpTracks[1].RTPTime;

            string RtpmapAttribute = VideoMediaDescription.GetAttributeValueByName("rtpmap");
            if (string.IsNullOrEmpty(RtpmapAttribute) == false)
            {
                string[] split = RtpmapAttribute.Split(' ');
                if (split.Length == 2)
                {
                    string[] values = split[1].Split('/');
                    Codec = values[0];
                    string bitrare=values[1];

                    

                }
            }

            OnRTSPServerResponse("play");
            

        }

        public void Teardown()
        {
            if (tcpClient == null) return;

            string localPath = uri.LocalPath;
            if (localPath[localPath.Length - 1] != '/')
                localPath += '/';

            Send(RTSPRequest.CreateTeardown(String.Format("rtsp://{0}{1}", uri.DnsSafeHost, localPath), CSeq, session));
        }

        private RTSPResponse Send(RTSPRequest request)
        {            
            RTSPResponse resp = RTSPResponse.Get(request, tcpClient);
            CSeq++;
            if (resp.StatusCode != 200)
            {
                throw new Exception("RTSPResponse " + resp.StatusCode);
            }
            else
            {
                return resp;
            }
        }

        private void OnRTSPServerResponse(string command)
        {
            if(RTSPServerResponse!=null)
                RTSPServerResponse(command);
        }

        public event RTSPResponseHandler  RTSPServerResponse;

        public delegate void RTSPResponseHandler(string command); //ReceiveUdpPacketHandler(IPEndPoint remoteEndPoint, IntPtr data, int count);

    }
}

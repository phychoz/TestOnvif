using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace TestOnvif
{
    public class RTSPSessionParameters
    {
        //...
        public string Session { get; set; }
    }

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

        public RTSPSessionParameters Parameters { get; set; } 

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
                networkStream = client.GetStream(),
                Parameters =new RTSPSessionParameters()
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

        public RTSPResponse Options()
        {
            return Send(RTSPRequest.CreateOptions(uri.AbsoluteUri, CSeq));
        }

        public RTSPResponse Describe()
        {
            return Send(RTSPRequest.CreateDescribe(uri.AbsoluteUri, CSeq));
        }

        public RTSPResponse Setup(string url, int RTPClientPort, int RTCPClientPort, string session="")
        {
            return Send(RTSPRequest.CreateSetup(url, CSeq, RTPClientPort, RTCPClientPort, session));
        }
        public RTSPResponse Play(string session)
        {
            return Send(RTSPRequest.CreatePlay(uri.AbsoluteUri, CSeq, session));
        }

        public void Teardown()
        {
            if (tcpClient == null) return;

            string localPath = uri.LocalPath;
            if (localPath[localPath.Length - 1] != '/')
                localPath += '/';

            Send(RTSPRequest.CreateTeardown(String.Format("rtsp://{0}{1}", uri.DnsSafeHost, localPath), CSeq, this.Parameters.Session));
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
    }
}

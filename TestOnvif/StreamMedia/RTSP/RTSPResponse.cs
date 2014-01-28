using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace TestOnvif
{
    class RTSPResponse
    {
        private RTSPResponse() { }

        /// <summary>
        /// Посылает запрос через tcpClient и получает ответ
        /// </summary>
        /// <param name="request">Посылаемый запрос</param>
        /// <param name="tcpClient">Соединение, с помощью которого нужно послать запрос</param>
        /// <returns>RTSP-ответ, завернутый в класс RTSPResponse</returns>
        public static RTSPResponse Get(RTSPRequest request, TcpClient tcpClient)
        {
            string req = request.ToString();
            string response = Send(req, tcpClient);
            return RTSPResponse.Parse(response);
        }

        /// <summary>
        /// Создает объект RTSPResponse из строки
        /// </summary>
        /// <param name="str">Текст в формате RTSP</param>
        /// <returns>Объект типа RTSPResponse</returns>
        private static RTSPResponse Parse(string str)
        {
            RTSPResponse response = new RTSPResponse();
            // разделяем RTSP ответ на голову и тело
            string[] headAndBody = str.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
            string head = headAndBody[0];
            // если в ответе есть тело, то получаем его
            response.Body = headAndBody.Length > 1 ? headAndBody[1] : null;
            // разделяем голову на заголовки
            string[] headers = head.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            { // извлекаем статус
                string[] parts = headers[0].Split(' ');
                response.StatusCode = int.Parse(parts[1]);
                string status = parts[2];
                for (int i = 3; i < parts.Length; i++)
                    status += " " + parts[i];
                response.Status = status;
            }

            for (int i = 1; i < headers.Length; i++)
            {
                // положение двоеточия в заколовке
                int colonIndex = headers[i].IndexOf(':');
                // имя заголовка
                string headerName = headers[i].Substring(0, colonIndex);
                // длина значения заголовка
                int headerValueLength = headers[i].Length - colonIndex - 1;
                // значение заголовка
                string headerValue = headers[i].Substring(colonIndex + 1, headerValueLength).Trim();
                // заполняем все заголовки
                switch (headerName)
                {
                    case "CSeq":
                        response.CSeq = int.Parse(headerValue);
                        break;
                    case "Connection":
                        response.Connection = headerValue;
                        break;
                    case "Content-Base":
                        response.ContentBase = headerValue;
                        break;
                    case "Content-Type":
                        response.ContentType = headerValue;
                        break;
                    case "Content-Length":
                        response.ContentLength = int.Parse(headerValue);
                        break;
                    case "Public":
                        response.Public = headerValue.Split(new string[] { ", " }, StringSplitOptions.None);
                        break;
                    case "Session":
                        response.Session = headerValue;
                        break;
                    case "Transport":
                        response.Transport = headerValue;
                        TransportParse(ref response, headerValue);
                        break;
                    case "RTP-Info":
                        response.RTPInfo = headerValue;
                        RTPInfoParse(ref response, headerValue);
                        break;
                }
            }
            return response;
        }
        
        private static void TransportParse(ref RTSPResponse response, string transportString)
        {
            string[] paramSplits=transportString.Split(';');
            foreach(string param in paramSplits)
            {
                string[] valueSplits = param.Split('=');
                if (valueSplits.Length == 2)
                {
                    string name = valueSplits[0];
                    string value = valueSplits[1];

                    switch (name)
                    {
                        case "client_port":
                            //...
                            break;

                        case "server_port":
                            //...
                            break;

                        case "ssrc":
                            response.SSRT = uint.Parse(value, System.Globalization.NumberStyles.HexNumber);
                            break;
                    }
                }
            }

        }

        private static void RTPInfoParse(ref RTSPResponse response, string RTPInfoString)
        {

            string[] trackSplits = RTPInfoString.Replace(" ",string.Empty).Split(',');

            foreach (string track in trackSplits)
            {
                RtpTrackInfo rtspTrack = new RtpTrackInfo();

                string[] paramSplits = track.Split(';');

                foreach (string param in paramSplits)
                {
                    string[] valueSplits = param.Split('=');

                    if (valueSplits.Length > 1)
                    {
                        string name = valueSplits[0];
                        string value = valueSplits[1];
  
                        switch (name)
                        {
                            case "url":
                                if(valueSplits.Length==2)
                                {
                                    rtspTrack.URL=value;
                                }
                                else if (valueSplits.Length==3)
                                {
                                    rtspTrack.URL=String.Format("{0}={1}",value,valueSplits[2] );
                                }
                                break;

                            case "seq":
                                rtspTrack.Seq = uint.Parse(value, System.Globalization.NumberStyles.Integer);
                                //...
                                break;

                            case "rtptime":
                                rtspTrack.RTPTime = uint.Parse(value, System.Globalization.NumberStyles.Integer);
                                //...
                                break;
                        }
                    }                 
                }

                response.RtpTracks.Add(rtspTrack);
            }
        }

       

        private static string Send(string request, TcpClient tcpClient)
        {
            byte[] dataToSend = Encoding.ASCII.GetBytes(request);

            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.Write(dataToSend, 0, dataToSend.Length);

            StringBuilder message = new StringBuilder();
            if (networkStream.CanRead)
            {
                byte[] dataRecieved = new byte[1024];             
                int numberOfBytesRead = 0;
                do
                {
                    numberOfBytesRead = networkStream.Read(dataRecieved, 0, dataRecieved.Length);
                    message.AppendFormat(Encoding.ASCII.GetString(dataRecieved, 0, numberOfBytesRead));
                }
                while (networkStream.DataAvailable);
            }
            return message.ToString();
        }

        public int StatusCode { get; private set; }

        public string Status { get; private set; }

        public int CSeq { get; private set; }

        public string Connection { get; private set; }

        public string ContentBase { get; private set; }

        public string ContentType { get; private set; }

        public int ContentLength { get; private set; }

        public string[] Public { get; private set; }

        public string Session { get; private set; }

        public string Transport { get; private set; }

        public string RTPInfo { get; private set; }

        public string Body { get; private set; }

        public uint SSRT { get; set; }

        public List<RtpTrackInfo> RtpTracks=new  List<RtpTrackInfo>();

        internal struct RtpTrackInfo
        {
            public string URL { get; set; }
            public uint Seq { get; set; }
            public uint RTPTime { get; set; }
        }

    }



}

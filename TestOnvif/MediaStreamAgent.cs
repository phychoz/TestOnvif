using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    public class MediaStreamAgent : MediaDeviceAgent, IDisposable
    {
        public MediaStreamAgent(MediaDevice device) : base(device) { }

        private Uri mediaStreamUri;

        private RTSPSession rtspSession;

        private RTSPChannel videoChannel;
        private RTSPChannel audioChannel;

        private SDP sdp;

        public Uri MediaStreamUri
        {
            get { return mediaStreamUri; }
            set { mediaStreamUri = value; }
        }

        internal RTSPChannel VideoChannel
        {
            get { return videoChannel; }
            set { videoChannel = value; }
        }

        internal RTSPChannel AudioChannel
        {
            get { return audioChannel; }
            set { audioChannel = value; }
        }


        public void Start()
        {
            RTSPChannelParameters videoParameters = new RTSPChannelParameters {};
            RTSPChannelParameters audioParameters = new RTSPChannelParameters {};

            rtspSession = RTSPSession.Open(this.MediaDevice.ONVIF.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri);
            //rtspSession.RTSPServerResponse += new RTSPSession.RTSPResponseHandler(rtsp_RTSPServerResponse);


            // OPTIONS возвращает команды сервера
            // OPTIONS, DESCRIBE, SETUP, PLAY, PAUSE, GET_PARAMETER, TEARDOWN, SET_PARAMETER
            RTSPResponse respons = rtspSession.Options();

            // DESCRIBE возвращает SDP файл 
            respons = rtspSession.Describe();

            string ContentBase = respons.ContentBase;

            // Парсим SDP пакет
            sdp = SDP.Parse(respons.Body);
            videoParameters.Codec = sdp.GetCodec(MediaType: "video");
            videoParameters.SampleRate = sdp.GetSampleRate(MediaType: "video");

            string VideoControl = sdp.GetControl(MediaType: "video");

            audioParameters.Codec = sdp.GetCodec(MediaType: "audio");
            audioParameters.SampleRate = sdp.GetSampleRate(MediaType: "audio");

            string AudioControl = sdp.GetControl(MediaType: "audio"); 

            string VideoSetupUri = String.Format("{0}{1}", ContentBase, VideoControl);
            string AudioSetupUri = String.Format("{0}{1}", ContentBase, AudioControl);

            int[] ports = GetPortRange(4);
        
            videoParameters.RTPPort = ports[0];
            videoParameters.RTCPPort = ports[1];

            audioParameters.RTPPort= ports[2];
            audioParameters.RTCPPort = ports[3];


            respons = rtspSession.Setup(VideoSetupUri, videoParameters.RTPPort, videoParameters.RTCPPort); 
            rtspSession.Parameters.Session = respons.Session;

            videoParameters.SSRT = respons.SSRT;

            respons = rtspSession.Setup(AudioSetupUri, audioParameters.RTPPort, audioParameters.RTCPPort, rtspSession.Parameters.Session);

            audioParameters.SSRT = respons.SSRT;


            respons = rtspSession.Play(rtspSession.Parameters.Session); 

            videoChannel = new RTSPChannel(videoParameters);
            audioChannel = new RTSPChannel(audioParameters);

            //audioChannel.DataRecieved += MediaDevice.AVProcessor.AudioDataRecieved;
            //videoChannel.DataRecieved += MediaDevice.AVProcessor.VideoDataRecieved;

            audioChannel.DataRecieved += MediaDevice.Decoder.AudioDataRecieved;
            videoChannel.DataRecieved += MediaDevice.Decoder.VideoDataRecieved;

            videoChannel.StartRecieving();
            audioChannel.StartRecieving();
        }

        public void Stop()
        {
            if (rtspSession != null)
            {
                rtspSession.Teardown();
                rtspSession.Close();
            }

            audioChannel.DataRecieved -= MediaDevice.Decoder.AudioDataRecieved;
            videoChannel.DataRecieved -= MediaDevice.Decoder.VideoDataRecieved;

            //audioChannel.DataRecieved -= MediaDevice.AVProcessor.AudioDataRecieved;
            //videoChannel.DataRecieved -= MediaDevice.AVProcessor.VideoDataRecieved;

            videoChannel.StopRecieving();
            audioChannel.StopRecieving();

        }

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

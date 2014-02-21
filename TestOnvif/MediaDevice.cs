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
    public abstract class MediaDeviceAgent
    {
        private MediaDevice mediaDevice;

        public MediaDevice MediaDevice
        {
            get { return mediaDevice; }
        }

        public MediaDeviceAgent(MediaDevice device)
        {
            this.mediaDevice = device;
        }
    }

    public class MediaDevice 
    {
        public MediaDevice(string name, Uri uri)
        {
            MediaDeviceUri = uri;
            DisplayName = name;

            mediaStream = new MediaStreamAgent(this);
            onvif = new ONVIFAgent(this);
            //avProcessor = new AVProcessorAgent(this);

            Decoder = new AVDecoderAgent(this);
        }

        private string displayName = string.Empty;

        public string DisplayName
        {
            get { return displayName; }
            private set { displayName = value; }
        }

        private Uri mediaDeviceUri;

        private MediaStreamAgent mediaStream;
        private ONVIFAgent onvif;

        public  AVDecoderAgent Decoder {get; private set;}

        //private AVProcessorAgent avProcessor;

        //public AVProcessorAgent AVProcessor
        //{
        //    get { return avProcessor; }
        //    private set { avProcessor = value; }
        //}

        public MediaStreamAgent MediaStream
        {
            get { return mediaStream; }
            private set { mediaStream = value; }
        }

        public Uri MediaDeviceUri
        {
            get { return mediaDeviceUri; }
            private set { mediaDeviceUri = value; }
        }

        public ONVIFAgent ONVIF
        {
            get { return onvif; }
            private set { onvif = value; }
        }


        private CircularBuffer<VideoFrame> videoBuffer;
        private CircularBuffer<AudioFrame> audioBuffer;

        public bool AudioBufferIsComplete()
        {
            bool Result = false;
            if (audioBuffer != null)
            {
                Result= audioBuffer.IsComplete;
            }
            return Result;

        }

        public bool VideoBufferIsComplete()
        {
            bool Result = false;
            if (videoBuffer != null)
            {
                Result = videoBuffer.IsComplete;
            }
            return Result;
        }

        public AudioFrame GetAudioFrame()
        {
            return audioBuffer.Get();
        }

        public VideoFrame GetVideoFrame()
        {
            return videoBuffer.Get();
        }

        public bool Start()
        {
            audioBuffer = new CircularBuffer<AudioFrame>();
            videoBuffer = new CircularBuffer<VideoFrame>();

            Decoder.AudioFrameRecievedEventHandler += new EventHandler<AudioFrameRecievedEventArgs>(Decoder_AudioFrameRecievedEventHandler);
            Decoder.VideoFrameRecievedEventHandler += new EventHandler<VideoFrameRecievedEventArgs>(Decoder_VideoFrameRecievedEventHandler);

            //avProcessor.Start();
            Decoder.Start();

            mediaStream.Start();

            return true;
        }


        public void Stop()
        {
            mediaStream.Stop();
            Decoder.Stop();

            Decoder.AudioFrameRecievedEventHandler -= new EventHandler<AudioFrameRecievedEventArgs>(Decoder_AudioFrameRecievedEventHandler);
            Decoder.VideoFrameRecievedEventHandler -= new EventHandler<VideoFrameRecievedEventArgs>(Decoder_VideoFrameRecievedEventHandler);

            

            //avProcessor.Stop();
        }

        private void Decoder_VideoFrameRecievedEventHandler(object sender, VideoFrameRecievedEventArgs e)
        {
            //...
            videoBuffer.Add(e.VideoFrame);

        }

        private void Decoder_AudioFrameRecievedEventHandler(object sender, AudioFrameRecievedEventArgs e)
        {
            //..
            audioBuffer.Add(e.AudioFrame);
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


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FFmpegWrapper;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TestOnvif
{
    public class AVDeviceClient : MediaDeviceClient
    {
        public AVDeviceClient(MediaDevice device) : base(device) { }

        CodecParams inVideoParams;

        public CodecParams InVideoParams
        {
            get { return inVideoParams; }
            set { inVideoParams = value; }
        }

        CodecParams outVideoParams;

        public CodecParams OutVideoParams
        {
            get { return outVideoParams; }
            set { outVideoParams = value; }
        }

        FFmpegMedia ffmpegMedia;

        public FFmpegMedia FFmpegMedia
        {
            get { return ffmpegMedia; }
            set { ffmpegMedia = value; }
        }

        CircularBuffer<MediaData> videoBuffer;
        CircularBuffer<MediaData> audioBuffer;


        private Thread videoWorker;
        private Thread audioWorker;

        private AutoResetEvent videoEvent;
        private AutoResetEvent audioEvent;

        MediaData curVideoItem;
        MediaData curAudioItem;


        public FFmpegMedia FFmpegProcessor
        {
            get { return ffmpegMedia; }
            set { ffmpegMedia = value; }
        }


        public void Start()
        {
            audioBuffer = new CircularBuffer<MediaData>();
            videoBuffer = new CircularBuffer<MediaData>();

            ffmpegMedia = new FFmpegMedia();

            FFmpegMedia.LogDataReceived += (log) => { Logger.Write(log, EnumLoggerType.LogFile); };

            inVideoParams = MediaDevice.ONVIFClient.GetInputCodecParams();

            outVideoParams = new CodecParams(CodecType.MPEG4, inVideoParams.Width, inVideoParams.Height);

            ffmpegMedia.VideoDecoderParams = inVideoParams;
            ffmpegMedia.VideoEncoderParams = outVideoParams;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "PanasonicVideo");

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            string file = String.Format(@"{0}_{1}x{2}_to_{3}_{4}x{5}_{6:yyyy-MM-dd_HH-mm-ss}.{7}",
                inVideoParams.ID, inVideoParams.Width, inVideoParams.Height,
                outVideoParams.ID, outVideoParams.Width, outVideoParams.Height,
                DateTime.Now,
                "mkv");

            string filepath = Path.Combine(path, file);

            ffmpegMedia.OutputFilename = filepath;
            //ffmpegProcessor.Start();

            ffmpegMedia.VideoFrameReceived += ProcessVideoFrame;
            ffmpegMedia.AudioFrameReceived += ProcessAudioFrame;

            videoEvent = new AutoResetEvent(true);
            audioEvent = new AutoResetEvent(true);

            videoWorker = new Thread(ProcessVideo);
            audioWorker = new Thread(ProcessAudio);

            ffmpegMedia.Open();

            videoWorker.Start();
            audioWorker.Start();

            //videoWorker = Task.Factory.StartNew(() => ProcessVideo());
            //audioWorker = Task.Factory.StartNew(() => ProcessAudio());

        }

        public void FFmpegStop()
        {
            if (ffmpegMedia != null)
            {
                audioBuffer.IsComplete = true;
                videoBuffer.IsComplete = true;

                videoEvent.Set();
                audioEvent.Set();

                //Task.WaitAll(videoWorker, audioWorker);

                //videoWorker.Join();
                //audioWorker.Join();

                videoWorker.Abort();
                audioWorker.Abort();

                ffmpegMedia.Close();
                ffmpegMedia = null;
            }
        }


        private void ProcessVideoFrame(IntPtr data, int linesize, int width, int height, uint number, uint time, int flag)
        {

            videoBuffer.Add(new MediaData() { Data = data, Size = linesize, Type = MediaType.Video, Time = time });

            videoEvent.Set();
            //Logger.Write(String.Format("{0}; {1}; {2}; {3}", time, videoBuffer.Count, videoBuffer.First().Time, videoBuffer.Last().Time), EnumLoggerType.FFmpegLog);
        }

        private void ProcessAudioFrame(IntPtr data, int linesize, uint time)
        {
            audioBuffer.Add(new MediaData() { Data = data, Size = linesize, Type = MediaType.Audio, Time = time });

            audioEvent.Set();
        }



        public void ProcessAudio()
        {
            while (true)
            {
                audioEvent.WaitOne();

                if (/*(audioBuffer.Count == 0) && */(audioBuffer.IsComplete == true))
                    break;
                curAudioItem = audioBuffer.Get();
                if (curAudioItem != null)//audioBuffer.TryGet(out curAudioItem) == true)
                {
                    //lock (ffmpegMedia)
                    //{
                    //    ffmpegMedia.WriteAudioDataToFile(curAudioItem.Data, curAudioItem.Size, curAudioItem.Time, 0);
                    //}

                    OnPlayAudio(curAudioItem.Data, curAudioItem.Size);
                    //videoForm.PlayAudio(curAudioItem.Data, curAudioItem.Size);

                }
            }

        }

        public void ProcessVideo()
        {
            while (true)
            {
                videoEvent.WaitOne();

                if (/*(videoBuffer.Count == 0) &&*/ (videoBuffer.IsComplete == true))
                    break;
                curVideoItem = videoBuffer.Get();
                if (curVideoItem != null) //videoBuffer.TryGet(out curVideoItem) == true)
                {
                    //lock (ffmpegMedia)
                    //{
                    //    ffmpegMedia.WriteVideoDataToFile(curVideoItem.Data, curVideoItem.Size, curVideoItem.Time, 0);
                    //}

                    using (Bitmap bitmap = new Bitmap(inVideoParams.Width, inVideoParams.Height, curVideoItem.Size, PixelFormat.Format24bppRgb, curVideoItem.Data))
                    {
                        OnShowVideo(bitmap);
                        //videoForm.ShowVideo(bitmap);
                        //SaveImage(bitmap);
                    }
                }

            }

        }

        DateTime prevRtpTime = DateTime.Now;
        DateTime currRtpTime = DateTime.Now;
        int BitmapNumber = 0;

        //private void SaveImage(Bitmap bitmap)
        //{
        //    //int count = 0;
        //    //if (thresImage != null)
        //    //{
        //    //    count = CvInvoke.cvCountNonZero(thresImage);
        //    //}
        //    uint msec = curVideoItem.Time;

        //    currRtpTime = startVideoSessionTime + TimeSpan.FromMilliseconds(msec);
        //    string filename = String.Format("d:\\test_bitmap_folder\\number_{0}_frame_time_{1}.png", BitmapNumber, currRtpTime.ToString("HH_mm_ss.fff"));
        //    ProcessBitmap(bitmap, filename);


        //    //double luminosity = CalculateAverageLightness(bitmap);
        //    //if (luminosity > 0.75)
        //    //{
        //    //    //rayscaleFilter(bitmap);

        //    //    uint msec = curVideoItem.Time;

        //    //    currRtpTime = startVideoSessionTime + TimeSpan.FromMilliseconds(msec);

        //    //    string filename = String.Format("d:\\test_bitmap_folder\\number_{0}_frame_time_{1}_luminosity_{2}.png", BitmapNumber, currRtpTime.ToString("HH_mm_ss.fff"), Math.Round(luminosity, 3));

        //    //    //Logger.Write(String.Format("StartSessionTime={0}", rtpTime.ToString("HH:mm:ss.fff")), EnumLoggerType.DebugLog);
        //    //    bitmap.Save(filename, ImageFormat.Png);

        //    //    if (currRtpTime != prevRtpTime)
        //    //    {
        //    //        TimeSpan span = (currRtpTime - prevRtpTime);

        //    //        string message = string.Format("{0}; {1:HH:mm:ss.fff}; {2:HH:mm:ss.fff}; {3}", BitmapNumber, DateTime.Now, currRtpTime, span.TotalMilliseconds);
        //    //        Logger.Write(message, EnumLoggerType.Output);

        //    //        //Console.WriteLine("{0} {1:HH_mm_ss.fff}, {2}", BitmapNumber, DateTime.Now, span.TotalMilliseconds.ToString());

        //    //        prevRtpTime = currRtpTime;

        //    //        BitmapNumber++;

        //    //    }
        //    //}
        //}


        public static bool GrayscaleFilter(Bitmap bitmap)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - bitmap.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < bitmap.Height; ++y)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        p[0] = p[1] = p[2] = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);

                        p += 3;
                    }
                    p += nOffset;
                }
            }


            bitmap.UnlockBits(bmData);

            return true;
        }

        private double GetAverageLuminosity(Bitmap bitmap)
        {
            double luminosity = 0;

            int width = bitmap.Width;
            int height = bitmap.Height;
            unsafe
            {
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                uint* ptr = (uint*)data.Scan0;
                int length = bitmap.Width * bitmap.Height;

                for (int i = 0; i < length; i++)
                {
                    uint alfa = ptr[i] & 0xFF000000;

                    uint blue = ptr[i] & 0x00FF0000;
                    uint green = ptr[i] & 0x0000FF00;
                    uint red = ptr[i] & 0x000000FF;

                    //luminosity = (byte)(((0.2126 * red) + (0.7152 * green)) + (0.0722 * blue));
                    luminosity += (0.299 * red + 0.587 * green + 0.114 * blue);
                }

                bitmap.UnlockBits(data);
            }

            return luminosity / (width * height) / 255.0;
        }

        public static double CalculateAverageLightness(Bitmap bm)
        {
            double lum = 0;

            var width = bm.Width;
            var height = bm.Height;
            var bppModifier = bm.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var srcData = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            //Luminance (standard, objective): (0.2126*R) + (0.7152*G) + (0.0722*B)
            //Luminance (perceived option 1): (0.299*R + 0.587*G + 0.114*B)
            //Luminance (perceived option 2, slower to calculate): sqrt( 0.241*R^2 + 0.691*G^2 + 0.068*B^2 )

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        lum += (0.299 * p[idx + 2] + 0.587 * p[idx + 1] + 0.114 * p[idx]);
                    }
                }
            }

            bm.UnlockBits(srcData);

            var avgLum = lum / (width * height);


            return avgLum / 255.0;
        }

        public void VideoDataRecieved(IntPtr ptr, int size, bool key, uint pts)
        {
            ffmpegMedia.VideoDataProcessing(ptr, size, key, pts);
        }

        public void AudioDataRecieved(IntPtr ptr, int size, bool key, uint pts)
        {
            ffmpegMedia.AudioDataProcessing(ptr, size, false, pts);
        }


        public delegate void ShowVideoEventHandler(Bitmap bitmap);
        public event ShowVideoEventHandler ShowVideo;

        public delegate void PlayAudioEventHandler(IntPtr ptr, int size);
        public event PlayAudioEventHandler PlayAudio;

        private void OnShowVideo(Bitmap bitmap)
        {
            if (ShowVideo != null)
                ShowVideo(bitmap);
        }

        private void OnPlayAudio(IntPtr ptr, int size)
        {
            if (PlayAudio != null)
                PlayAudio(ptr, size);
        }

    }
}

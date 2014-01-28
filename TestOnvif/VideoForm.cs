using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TestOnvif
{
    public partial class VideoForm : Form
    {
        PCMUPlayer audioPlayer;

        Bitmap bitmap;
        Graphics graphics ;

        public VideoForm(String uri, String file, int width, int height) 
        {
            InitializeComponent();

            this.Text = String.Format("{0}-->{1}",uri, file); 

            this.Width = width;
            this.Height = height;

            audioPlayer=new PCMUPlayer();

            bitmap = new Bitmap(width, height); //@"d:\background.jpg");
            this.BackgroundImage = bitmap;

            graphics = this.CreateGraphics();

         }

        public void UpdateCapture(string capture)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    this.Text = capture;
                }));
            }
            else
            {
                this.Text = capture;
            }
        }

        public void PlayAudio(IntPtr data, int size)
        {
            audioPlayer.PlayFromMemory(data, size);
        }


        //public void ShowVideo(Emgu.CV.IImage image)
        //{
        //    if (this.InvokeRequired)
        //        this.Invoke((Action)(() =>
        //        {
        //            imageBox1.Image = image;

        //        }));
        //    else
        //    {
        //        imageBox1.Image = image;
        //    }
        //}


        public void ShowVideo(Bitmap bmp)
        {
            if (this.InvokeRequired)
                this.Invoke((Action)(() =>
                {
                    //bitmap = bmp.Clone(new Rectangle(0, 0, this.Width, this.Height), PixelFormat.Format24bppRgb);
                    //this.BackgroundImage = bitmap;

                    //graphics.DrawImage(bmp, new Rectangle(0, 0, this.Width, this.Height));

                    using (Graphics g = this.CreateGraphics())
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, this.Width, this.Height));
                    }

                }));
            else
            {
                //bitmap = bmp.Clone(new Rectangle(0, 0, this.Width, this.Height), PixelFormat.Format24bppRgb);
                //this.BackgroundImage = bitmap;

                //graphics.DrawImage(bmp, new Rectangle(0, 0, this.Width, this.Height));

                using (Graphics g = this.CreateGraphics())
                {
                    g.DrawImage(bmp, new Rectangle(0, 0, this.Width, this.Height));
                }
            }
        }
    }
}


//private void ProcessAudio()
//{
//    foreach (MediaData item in mediaDevice.AudioBuffer.GetConsumingEnumerable())
//    {
//        //mediaDevice.FFmpegProcessor.WriteAudioDataToFile(item.Data, item.Size);
//        if (mediaDevice.AudioBuffer.IsAddingCompleted == false)
//        {
//            audioPlayer.PlayFromMemory(item.Data, item.Size);
//        }
//    }
//}

//private void ProcessVideo()
//{
//    foreach (MediaData item in mediaDevice.VideoBuffer.GetConsumingEnumerable())
//    {
//        if (mediaDevice.VideoBuffer.IsAddingCompleted == false)
//        {
//            mediaDevice.FFmpegProcessor.WriteVideoDataToFile(item.Data, item.Size);

//            using (Bitmap bitmap = new Bitmap(mediaDevice.InVideoParams.Width, mediaDevice.InVideoParams.Height, item.Size, PixelFormat.Format24bppRgb, item.Data))
//            {
//                using (Graphics g = this.CreateGraphics())
//                {
//                    g.DrawImage(bitmap, new Rectangle(0, 0, this.Width, this.Height));
//                }
//            }
//           //Thread.Sleep(1000);
//        }
//    }

//}

//MediaData item;
//while (mediaDevice.AudioBuffer.IsCompleted == false)
//{
//    if (mediaDevice.AudioBuffer.TryTake(out item) == true)
//    {
//        audioPlayer.PlayFromMemory(item.Data, item.Size);
//    }
//}

//MediaData item;
//while (mediaDevice.VideoBuffer.IsCompleted == false)
//{
//    if (mediaDevice.VideoBuffer.TryTake(out item) == true)
//    {
//        using (Bitmap bitmap = new Bitmap(mediaDevice.InVideoParams.Width, mediaDevice.InVideoParams.Height, item.Size, PixelFormat.Format24bppRgb, item.Data))
//        {
//            using (Graphics g = this.CreateGraphics())
//            {
//                g.DrawImage(bitmap, new Rectangle(0, 0, this.Width, this.Height));
//            }
//        }
//    }
//}

//private void ShowFrame(IntPtr data, int linesize, int width, int height)
//{
//    if (this.InvokeRequired)
//        this.Invoke((Action)(() =>
//        {
//            DrawFrame(data, linesize, width, height);

//        }));
//    else
//    {
//        DrawFrame(data, linesize, width, height);
//    }
//}

//private void DrawFrame(IntPtr data, int linesize, int width, int height)
//{
//    if (data == IntPtr.Zero || linesize == 0 || width == 0 || height == 0) return;

//    using (Bitmap bitmap = new Bitmap(width, height, linesize, PixelFormat.Format24bppRgb, data))
//    {
//        using (Graphics g = this.CreateGraphics())
//        {
//            g.DrawImage(bitmap, new Rectangle(0, 0, this.Width, this.Height));
//        }
//    }
//}


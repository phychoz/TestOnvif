﻿using System;
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
using TestOnvif;
 

namespace Onvif.Controls
{
    public partial class VideoForm : Form, IMediaForm
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


            this.MouseWheel+=new MouseEventHandler(VideoForm_MouseWheel);

            this.MouseClick +=new MouseEventHandler(VideoForm_MouseClick);

            MediaServiceController.Controller.VideoDataReady+=new EventHandler<VideoDataEventArgs>(Controller_VideoDataReady);
            MediaServiceController.Controller.AudioDataReady +=new EventHandler<AudioDataEventArgs>(Controller_AudioDataReady);

            this.FormClosed += (o, a) => 
            {
                MediaServiceController.Controller.VideoDataReady -= new EventHandler<VideoDataEventArgs>(Controller_VideoDataReady);
                MediaServiceController.Controller.AudioDataReady -= new EventHandler<AudioDataEventArgs>(Controller_AudioDataReady);
            };

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

        private void Controller_VideoDataReady(object sender, VideoDataEventArgs e)
        {
            ShowVideo(e.Bitmap);
        }

        private void Controller_AudioDataReady(object sender, AudioDataEventArgs e)
        {
            PlayAudio(e.Ptr, e.Size);
        }


        public void PlayAudio(IntPtr data, int size)
        {
            audioPlayer.PlayFromMemory(data, size);
        }

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

        public void UpdateControls()
        {
            if (MediaServiceController.Controller.IsConnected == true)
            {
                if (MediaServiceController.Controller.IsStreaming == true)
                {
                    //this.Show();
                }
                else
                {
                    //this.Close();
                    
                }
            }
            else
            {

            }

        }

        public void VideoForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { MediaServiceController.Controller.ONVIF.MoveAndZoomCamera20(0F, 0F, -0.7F); }));

            if (e.Delta > 0)
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { MediaServiceController.Controller.ONVIF.MoveAndZoomCamera20(0F, 0F, 0.7F); }));
        }

        public void VideoForm_MouseClick(object sender, MouseEventArgs e)
        {
            int centerPan = this.Width / 2;
            int centerTilt = this.Height / 2;

            float pan = 2 * (float)(e.X - centerPan) / (float)this.Width;
            float tilt = 2 * (float)(centerTilt - e.Y) / (float)this.Height;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { MediaServiceController.Controller.ONVIF.MoveAndZoomCamera20(pan, tilt, 0); }));
        }

    }
}

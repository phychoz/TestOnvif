using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Net;
namespace TestOnvif
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Колекция доступных ONVIF устройств
        /// </summary>
        MediaDevice[] mediaDeviceCollection;

        /// <summary>
        /// камера с которой работаем
        /// </summary>
        MediaDevice mediaDevice;

        VideoForm videoForm;// = new VideoForm();

        System.Windows.Forms.Timer timer;
        
        public MainForm()
        {
            InitializeComponent();

            Logger.LoggingEnabled = true;

            // получаем список доступных ONVIF устройств
            mediaDeviceCollection = ONVIFClient.GetAvailableMediaDevices();

            // получаем информацию по каждому устройству
            if (mediaDeviceCollection.Count() > 0)
            {
                for (int index = 0; index < mediaDeviceCollection.Length; index++)
                {
                    string information = ONVIFClient.GetDeviceInformation(mediaDeviceCollection[index].MediaDeviceUri);

                    this.DeviceComboBox.Items.Add(information);
                }

                mediaDevice = mediaDeviceCollection[0];

                this.DeviceComboBox.SelectedIndex = 0;
            }
 
            timer = new System.Windows.Forms.Timer() { /*Interval = 1000*/ };
            timer.Tick += (obj, arg) => 
            { 
                TimerLabel.Text = "Time: " + DateTime.Now.ToString("HH:mm:ss");
                //CameraLabel.Text = "Time: " + CameraDateTime.Get(address);//mediaDevice.GetSystemDateAndTime();
            };
            timer.Start();

            Logger.EchoEvent += (message) => 
            {
                if (this.InvokeRequired == true)
                {
                    this.Invoke((Action)(() => { this.LoggerTextBox.AppendText(message); }));
                }
                else
                    this.LoggerTextBox.AppendText(message);
            };
        }


        private void ConnectButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.Connect("admin", "123456");

            foreach (deviceio.Profile profile in mediaDevice.ONVIFClient.MediaProfiles)
            {
                MediaProfileComboBox.Items.Add(profile.Name);
            }

            MediaProfileComboBox.SelectedIndex = mediaDevice.ONVIFClient.MediaProfileIndex;
        }

        private void VideoStartButton_Click(object sender, EventArgs e)
        {
            if (mediaDevice != null)
            {
                bool result = mediaDevice.StartMedia();
                if (result == true)
                {
                    VideoFormStart();
                     //mediaDevice.RtspStart();

                    VideoStartButton.Enabled = false;
                    MediaProfileComboBox.Enabled = false;

                    VideoStopButton.Enabled = true;
                }
            }
        }

        private void VideoStopButton_Click(object sender, EventArgs e)
        {
            if (mediaDevice != null)
            {
                mediaDevice.StopMedia();
                VideoFormStop();

                VideoStartButton.Enabled = true;
                VideoStopButton.Enabled = false;
                MediaProfileComboBox.Enabled = true;
            }
        }

        #region ONVIF

        private void WsDicoveryButton_Click(object sender, EventArgs e)
        {
            //BackgroundWorker worker = new BackgroundWorker();

            //worker.DoWork += (s, arg) =>
            //    {
            //        mediaDeviceCollection = GetAvailableMediaDevices();
            //    };

            //worker.RunWorkerCompleted += (s, arg) =>
            //    {
            //        if (mediaDeviceCollection != null)
            //        {
            //            DiscoverForm df = new DiscoverForm(mediaDeviceCollection);
            //            df.ShowDialog();                    
            //        }

            //        this.Cursor = Cursors.Default;
            //        WsDicoveryButton.Enabled = true;

            //    };

            //worker.RunWorkerAsync();
            //WsDicoveryButton.Enabled = false;
            //this.Cursor = Cursors.WaitCursor;
        }

        private void FindDeviceButton_Click(object sender, EventArgs e)
        {

        }

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void setDateTimeButton_Click(object sender, EventArgs e)
        {
            if (mediaDevice != null)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (obj, arg) =>
                {
                    mediaDevice.ONVIFClient.SetSystemDateAndTime(DateTime.Now);
                };
                worker.RunWorkerCompleted += (obj, arg) =>
                {
                    this.Enabled = true;
                    this.Cursor = Cursors.Default;
                };
                worker.RunWorkerAsync();
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;
            }
        }

        private void SetDateTimefromNtpButton_Click(object sender, EventArgs e)
        {
            if (mediaDevice != null)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (obj, arg) =>
                {
                    mediaDevice.ONVIFClient.SetSystemDateAndTimeNTP("192.168.10.251", "UTC-4");
                };
                worker.RunWorkerCompleted += (obj, arg) =>
                {
                    this.Enabled = true;
                    this.Cursor = Cursors.Default;
                };

                worker.RunWorkerAsync();
                Logger.Write(String.Format("SetSystemDateAndTimeNTP"), EnumLoggerType.DebugLog);
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;
                
                
            }
        }
        private void RebootButton_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Reboot will take 2 minutes\nYou are sure that want to reboot device now?", "",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (RebootForm rf = new RebootForm(mediaDevice.ONVIFClient.Reboot()))
                {
                    rf.ShowDialog();
                }
            }
        }

        private void getDeviceInformationButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(mediaDevice.ONVIFClient.GetDeviceInformation());
        }

        private void GetSystemDateAndTimeButton_Click(object sender, EventArgs e)
        {
            string nowTimeString = DateTime.Now.ToString("HH:mm:ss.fff");

            string mediaTimeString=string.Empty;

            DateTime? media = mediaDevice.ONVIFClient.GetSystemDateAndTime();

            if (media != null)
                mediaTimeString = ((DateTime)media).ToString("HH:mm:ss.fff");

            string result = string.Format("now={0}; media={1}", nowTimeString, mediaTimeString);
            Logger.Write(result, EnumLoggerType.Output);
            //MessageBox.Show(mediaDevice.GetSystemDateAndTime());

        }

        private void getHostnameButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(mediaDevice.ONVIFClient.GetHostname());
        }

        private void leftButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(-0.7F, 0F, 0F);
        }

        private void rightButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(0.7F, 0F, 0F);
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(0F, 0.7F, 0F);
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(0F, -0.7F, 0F);
        }

        private void ZoomInButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(0F, 0F, -0.7F);
        }

        private void ZoomOutButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MoveAndZoomCamera(0F, 0F, 0.7F);
        }

        private void getStreamUriButton_Click(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.GetCurrentMediaProfileRtspStreamUri();
        } 
        #endregion


        private void WriteToFileCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //if(WriteToFileCheckBox.Checked)
            //    mediaDevice.FFmpegProcessor.WriteVideoFrameToFile = true;
            //else
            //    mediaDevice.FFmpegProcessor.WriteVideoFrameToFile = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mediaDevice != null)
                mediaDevice.StopMedia();
        }

        private void MediaClientGetProfilesButton_Click(object sender, EventArgs e)
        {
            MediaClientProfilesForm mcpf = new MediaClientProfilesForm(mediaDevice.ONVIFClient.MediaProfiles);
            mcpf.ShowDialog();
        }

        private void GetConfigurationButton_Click(object sender, EventArgs e)
        {

        }

        private void OpenVideoButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\";
            openFileDialog.Filter = @"video files (*.mkv)|*.mkv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (String.IsNullOrEmpty(openFileDialog.FileName) == false)
                    {
                        string filename = openFileDialog.FileName;
                        //this.Text = inputMediaFile;

                        //Start();
                        //mediaDevice.OpenVideo(filename);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void WebButton_Click(object sender, EventArgs e)
        {
            WebForm form = new WebForm(new Uri(@"http://192.168.10.203/admin/index.html"));
            form.Show();
        }

        private void MediaProfileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mediaDevice.ONVIFClient.MediaProfileIndex = MediaProfileComboBox.SelectedIndex;

        }


        private void VideoFormStart()
        {
            string uri = mediaDevice.ONVIFClient.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri;
            string filename=mediaDevice.AVClient.FFmpegMedia.OutputFilename;

            int width=  mediaDevice.AVClient.InVideoParams.Width;
            int height =mediaDevice.AVClient.InVideoParams.Height;

            videoForm = new VideoForm(uri, filename,width, height);

            mediaDevice.AVClient.ShowVideo += videoForm.ShowVideo;
            mediaDevice.AVClient.PlayAudio += videoForm.PlayAudio;

            videoForm.MouseWheel += videoForm_MouseWheel;

            videoForm.MouseClick +=videoForm_MouseClick;

            videoForm.Show();
        }

        private void videoForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { mediaDevice.ONVIFClient.MoveAndZoomCamera20(0F, 0F, -0.7F); }));

            if (e.Delta > 0)
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { mediaDevice.ONVIFClient.MoveAndZoomCamera20(0F, 0F, 0.7F); }));
        }

        private void videoForm_MouseClick(object sender, MouseEventArgs e)
        {
            int centerPan = videoForm.Width / 2;
            int centerTilt = videoForm.Height / 2;

            float pan = 2 * (float)(e.X - centerPan) / (float)videoForm.Width;
            float tilt = 2 * (float)(centerTilt - e.Y) / (float)videoForm.Height;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { mediaDevice.ONVIFClient.MoveAndZoomCamera20(pan, tilt, 0); }));
        }

        private void VideoFormStop()
        {
            if (videoForm != null)
            {
                mediaDevice.AVClient.ShowVideo -= videoForm.ShowVideo;
                mediaDevice.AVClient.PlayAudio -= videoForm.PlayAudio;

                videoForm.MouseWheel -= videoForm_MouseWheel;

                videoForm.MouseClick -= videoForm_MouseClick;

                videoForm.Close();
                videoForm = null;
            }

        }



    }

}

//VideoPanel.GetType().GetMethod("SetStyle", System.Reflection.BindingFlags.Instance |
//    System.Reflection.BindingFlags.NonPublic).Invoke(pictureBox, new object[] { System.Windows.Forms.ControlStyles.UserPaint |
//        System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.DoubleBuffer, true });

//VideoPanel.MouseWheel += new MouseEventHandler(VideoPanel_MouseWheel);
//VideoPanel.MouseClick+=new MouseEventHandler(VideoPanel_MouseClick);
//MouseWheel += new MouseEventHandler(VideoPanel_MouseWheel);
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
//    if ( data == IntPtr.Zero || linesize == 0 || width == 0 || height == 0) return;

//    using (Bitmap bitmap = new Bitmap(width, height, linesize, PixelFormat.Format24bppRgb, data))
//    {
//        //graphics.DrawImage(bitmap, new Rectangle(0, 0, VideoPanel.Width, VideoPanel.Height));
//        using (Graphics g = VideoPanel.CreateGraphics())
//        {
//            g.DrawImage(bitmap, new Rectangle(0, 0, VideoPanel.Width, VideoPanel.Height));
//        }
//    }
//}

//private void VideoPanel_MouseWheel(object sender, MouseEventArgs e)
//{
//    if (currentMediaDevice != null)
//    {
//        if (e.Delta < 0)
//            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { currentMediaDevice.MoveAndZoomCamera(0F, 0F, -0.7F); }));

//        if (e.Delta > 0)
//            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { currentMediaDevice.MoveAndZoomCamera(0F, 0F, 0.7F); }));
//    }
//}

//private void VideoPanel_MouseClick(object sender, MouseEventArgs e)
//{
//    if (currentMediaDevice != null)
//    {
//        int centerPan = VideoPanel.Width / 2;
//        int centerTilt = VideoPanel.Height / 2;

//        float pan = 2 * (float)(e.X - centerPan) / (float)VideoPanel.Width;
//        float tilt = 2 * (float)(centerTilt - e.Y) / (float)VideoPanel.Height;

//        ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => { currentMediaDevice.MoveAndZoomCamera(pan, tilt, 0); }));

//    }
//}

//private void SelectDeviceButton_Click(object sender, EventArgs e)
//{
//    if (mediaDevices.Length > 0)
//    {
//        int index = MediaBox.SelectedIndex;
//        currentMediaDevice = mediaDevices[index];
//        CreateMediaDevice(currentMediaDevice);
//        currentMediaDevice.GetHostname();

//        TreeNode[] children = new TreeNode[] 
//        {
//            new TreeNode(){ Text=String.Format("{0}",currentMediaDevice.HostName)},
//            new TreeNode(){ Text=String.Format("Firmware {0}",currentMediaDevice.Firmware)},
//            new TreeNode(){Text=String.Format("Serial {0}",currentMediaDevice.Serial)},
//            new TreeNode(){Text=String.Format("Hardware {0}",currentMediaDevice.Hardware)}
//        };
//        TreeNode parent = new TreeNode(currentMediaDevice.Model, children);
//        DeviceTree.Nodes.Add(parent);
//        //panel2.Enabled = true;
//    }


//}

//VideoCapture1.Screen_Zoom_Ratio = 0;
//VideoCapture1.Screen_Zoom_ShiftX = 0;
//VideoCapture1.Screen_Zoom_ShiftY = 0;

//if (VideoCapture.Filter_Supported_VMR9())
//{
//    VideoCapture1.Video_Renderer = VFVideoRenderer.VMR9;
//}
//else
//{
//    VideoCapture1.Video_Renderer = VFVideoRenderer.VideoRenderer;
//}

//VideoCapture1.IP_Camera_URL = uri.OriginalString;
//VideoCapture1.IP_Camera_Type = VFIPSource.RTSP_UDP;

//VideoCapture1.IP_Camera_FFMPEG_Capture = true;
//VideoCapture1.Mode = VFVideoCaptureMode.IPCapture;

//VideoCapture1.Output_Filename = @"test.avi";
//VideoCapture1.Output_Format = VFVideoCaptureOutputFormat.AVI;

//VideoCapture1.Audio_Codec_Name = "PCM";
//VideoCapture1.Audio_Codec_Channels = 2;
//VideoCapture1.Audio_Codec_BPS = 16;
//VideoCapture1.Audio_Codec_SampleRate = 48000;

//VideoCapture1.Start();

//MediaBase media = new PathMedia(uri.OriginalString);

//vlcControl.Media = media;
//vlcControl.Play();
//VideoForm videoForm = new VideoForm(uri);
//videoForm.Show();
//#region AUDIO_REGION

//FileStream g711Stream;

//Stream waveStream;
//BinaryWriter waveWriter;

//PCMUPlayer player = new PCMUPlayer();
//G711Decoder g711Decoder = new G711Decoder();

//StreamMedia.Audio.WaveHeader waveHeader;// = new StreamMedia.Audio.WaveHeader();

//public void WaveWriterStart()
//{
//    waveHeader = new StreamMedia.Audio.WaveHeader();
//    if (waveStream != null)
//        return;

//    try
//    {
//        waveStream = File.Open(String.Format(@"D:\RawPacket\payload_{0:yyyy-MM-dd_hh-mm-ss}.wav", DateTime.Now), FileMode.Create);
//        waveWriter = new BinaryWriter(waveStream);
//        //WriteWaveHeader();

//    }
//    catch (Exception e) { }
//}

//void WriteWaveHeader()
//{
//    waveWriter.Write(waveHeader.Riff.ID.ToCharArray());
//    waveWriter.Write(waveHeader.Riff.Length);
//    waveWriter.Write(waveHeader.Riff.Type.ToCharArray());

//    waveWriter.Write(waveHeader.Format.ID.ToCharArray());
//    waveWriter.Write(waveHeader.Format.Size);
//    waveWriter.Write(waveHeader.Format.Tag);
//    waveWriter.Write(waveHeader.Format.Channels);
//    waveWriter.Write(waveHeader.Format.Samples);
//    waveWriter.Write(waveHeader.Format.Average);
//    waveWriter.Write(waveHeader.Format.Align);
//    waveWriter.Write(waveHeader.Format.Bits);

//    waveWriter.Write(waveHeader.Data.ID.ToCharArray());
//}


//public void WaveWriterClose()
//{
//    if (waveStream == null) return;

//    uint filesize = (uint)waveWriter.BaseStream.Length - 8;

//    waveWriter.Seek(4, SeekOrigin.Begin);
//    waveWriter.Write(filesize);

//    waveWriter.Seek(32, SeekOrigin.Current); //data.Size);
//    waveWriter.Write(waveHeader.Data.Size);

//    waveWriter.Close();
//    waveStream.Close();
//}

//void rtpAudioHandler_RawAudioFromRtp(IntPtr data, int size)
//{
//    //int length = packet.PayloadLength;
//    byte[] payload = new byte[size];

//    waveHeader.Data.Size += (uint)size;
//    Marshal.Copy(data, payload, 0, size);
//    waveWriter.Write(payload);
//    //byte[] outWav = g711Decoder.Decode(payload);

//    //waveWriter.Write(outWav);

//}

//#endregion


//delegate void AudioDelegate(IntPtr ptr, int count);
//AudioDelegate audioDelegate;
//GCHandle audioGCH;

//void SetAudioHandler()
//{
//    //audioDelegate = rtpAudioHandler_RawAudioFromRtp;
//    audioDelegate = player.PlayFromMemory;
//    audioGCH = GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(audioDelegate));

//    ffmpegVideo.SetAudioHandler((IntPtr)audioGCH.Target);

//}



//delegate void PaintDelegate(Bitmap bmp);
//PaintDelegate paintDelegate;
//GCHandle paintGCH;

//void SetVideoHandler()
//{

//    paintDelegate = OnPaint;

//    paintGCH = GCHandle.Alloc(Marshal.GetFunctionPointerForDelegate(paintDelegate));
//    ffmpegVideo.SetVideoHandler((IntPtr)paintGCH.Target);
//    //GC.Collect();

//    //ffmpegVideo.VideoFrameReceived += OnPaint;


//}

//public void OnPaint(Bitmap bmp)
//{

//    if (this.InvokeRequired)
//        this.Invoke((Action)(() =>
//        {
//            //pictureBox.Image = bmp ;
//            //Clipboard.SetImage(bmp);
//            Graphics g = pictureBox.CreateGraphics();
//            Rectangle rect =new Rectangle(0,0,pictureBox.Width, pictureBox.Height);
//            g.DrawImage(bmp, rect);
//            //bmp.Dispose();
//            g.Dispose();

//        }));
//    else
//    {
//       // pictureBox.Image = bmp ;
//        //Clipboard.SetImage(bmp);
//        Graphics g = pictureBox.CreateGraphics();
//        Rectangle rect = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);
//        g.DrawImage(bmp, rect);
//        g.Dispose();
//    }

//    //bmp.Dispose();
//    //string file = String.Format(@"d:\TESTBMP\test_{0:mm ss ffff}.bmp", DateTime.Now);
//    //bmp.Save(file);
//    //Console.WriteLine("It worked");
//}

//FFmpegVideo ffmpegVideo = new FFmpegVideo();


//private void ffmpegStartButton_Click(object sender, EventArgs e)
//{
//    //WaveWriterStart();
//    new Thread(() =>
//    {
//        try
//        {
//            //FFmpegVideo ffpegVideo = new FFmpegVideo();
//            SetVideoHandler();
//            //SetAudioHandler();

//            ffmpegVideo.MediaSource = "rtsp://192.168.10.203/ONVIF/MediaInput?profile=1_def_profile3"; //@"d:\test.mp4";//

//            ffmpegVideo.AVRegisterAll();
//            ffmpegVideo.AVFormatNetworkInit();

//            ffmpegVideo.AVFormatOpenInput();
//            ffmpegVideo.AVFormatFindStreamInfo();
//            ffmpegVideo.AVCodecOpen2();
//            //ffpegVideo.SDLInitVideo();
//            ffmpegVideo.SwsGetCachedContext();

//            //ffpegVideo.ShowVideo();
//            ffmpegVideo.GetBitmap();

//            ffmpegVideo.SwsFreeContext();
//            ffmpegVideo.AVCodecClose();
//            ffmpegVideo.AVFormatNetworkDeinit();
//            ffmpegVideo.AVFormatCloseInput();
//            //ffpegVideo.SDLQuit();

//            //ffpegVideo.ShowVideo();
//            //new Thread(() => ffpegVideo.ShowVideo("rtsp://192.168.10.203/ONVIF/MediaInput?profile=1_def_profile6")).Start();
//        }
//        catch (Exception ex)
//        {
//            MessageBox.Show(ex.ToString());
//            //Console.WriteLine(ex.Message);
//        }
//        finally
//        {
//            //paintGCH.Free();
//            //Console.ReadKey();
//        }
//    }).Start();
//}



//void jpegHandler_JpegFrameReceived(IntPtr ptr, int count)
//{
//    byte[] data = new byte[count];
//    Marshal.Copy(ptr, data, 0, count);
//    MemoryStream memory = null;
//    try
//    {
//        memory = new MemoryStream(data);
//        Image image = Image.FromStream(memory);

//        if (this.InvokeRequired)
//            this.BeginInvoke((Action)delegate { ShowFrame(image); });
//        else
//            ShowFrame(image);
//        //VideoBox.Image = image;}
//    }
//    catch (Exception ex)
//    {
//        Logger.Write(ex);
//    }
//    finally
//    {
//        if (memory != null)
//            memory.Close();
//    }

//}

//void ShowFrame(Image image)
//{
//    using (Bitmap bitmap = new Bitmap(image))
//    {
//        using (Graphics g = VideoPanel.CreateGraphics())
//        {
//            g.DrawImage(bitmap, new Rectangle(0, 0, VideoPanel.Width, VideoPanel.Height));
//        }
//    }

//    //lock (locker)
//    //{
//    //    frameCount++;
//    //}
//}


//void timer_Tick(object sender, EventArgs e)
//{
//    lock (locker)
//    {
//        this.Text = "FPS=" + frameCount;
//        frameCount = 0;
//    }
//}

//#region FFmpegProcess

////private void UpdateRecControl()
////{

////    //RecStartButton.Enabled = false;
////    //RecStopButton.Enabled = true;
////}
////private void RecStopButton_Click(object sender, EventArgs e)
////{
////    StopRecord();
////}

////private void StopRecord()
////{
////    if (isRecord)
////    {
////        VirtualKeyDown();
////        //RecStartButton.Enabled = true;
////        //RecStopButton.Enabled = false;
////        isRecord = false;
////    }
////}
////[DllImport("User32.dll")]
////private static extern bool SetForegroundWindow(IntPtr hWnd);

////[DllImport("user32.dll", CharSet = CharSet.Auto)]
////static public extern IntPtr GetForegroundWindow();

////[DllImport("user32.dll")]
////static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

////[DllImport("user32.dll")]
////static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

////[DllImport("user32.dll", SetLastError = true)]
////static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

////[DllImport("User32.Dll", EntryPoint = "PostMessageA")]
////static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

////[DllImport("user32.dll")]
////static extern byte VkKeyScan(char ch);

////[DllImport("user32.dll")]
////private static extern int ShowWindow(IntPtr hWnd, int showState);

////[StructLayout(LayoutKind.Sequential)]
////struct COORD
////{
////    public short x;
////    public short y;
////}

////[DllImport("kernel32.dll", EntryPoint = "SetConsoleCursorPosition", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
////private static extern int SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);
////[DllImport("user32.dll")]

////static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

////[DllImport("user32.dll")]

////static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

////[DllImport("user32.dll")]

////static extern IntPtr RemoveMenu(IntPtr hMenu, uint nPosition, uint wFlags);

////const int SC_CLOSE = 0xF060;
////const int MF_SYSMENU = 0x00002000;
////const int MF_DISABLED = 0x00000002;
////const int MF_GRAYED = 0x00000001;
////const int MF_BYCOMMAND = 0x00000000;

////const int WM_KEYDOWN = 0x100;
////const int VK_CONTROL = 0x11;
////const int WM_KEYUP = 0x0101;
////const int KEYEVENTF_KEYUP = 2;

////private void SendCtrlC(IntPtr hWnd)
////{
////    SetForegroundWindow(hWnd);
////    keybd_event(VK_CONTROL, 0, 0, 0);
////    keybd_event(0x43, 0, 0, 0);
////    keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
////    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

////}

////private void VirtualKeyDown()//char key)
////{
////    //string process=String.Format("{0}\\Video\\ffmpeg.exe", Environment.CurrentDirectory);
////    IntPtr handle = ffmpeg.MainWindowHandle; // FindWindow(null, process);
////    if (handle != IntPtr.Zero)
////        SendCtrlC(handle);
////}

////private void button1_Click(object sender, EventArgs e)
////{
////    IntPtr handle = ffmpeg.MainWindowHandle;

////    if (handle != IntPtr.Zero)
////    {
////        IntPtr hSystemMenu = GetSystemMenu(handle, false);
////        EnableMenuItem(hSystemMenu, SC_CLOSE, MF_GRAYED);
////        RemoveMenu(hSystemMenu, SC_CLOSE, MF_BYCOMMAND);
////    }
////}

////Process ffmpeg;
////bool isRecord = false;

////private void RecStartButton_Click(object sender, EventArgs e)
////{
////    if (!isRecord)
////    {
////        ffmpeg = new Process();
////        //ffmpeg.StartInfo.UseShellExecute = false;
////        ffmpeg.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "\\Video";
////        string fileName = String.Format("d:\\Test_Panasonic_Video\\record_{0:yyyy-MM-dd_hh-mm-ss}.mp4", DateTime.Now);
////        ffmpeg.StartInfo.Arguments = String.Format("-i rtsp://192.168.10.203/ONVIF/MediaInput?profile=1_def_profile{0} -y -vcodec copy {1}", 8, fileName);
////        ffmpeg.StartInfo.FileName = "ffmpeg.exe ";

////        //ffmpeg.EnableRaisingEvents = true;
////        //ffmpeg.Exited += (obj, args) =>
////        //{
////        //    MessageBox.Show(String.Format("{0}, {1}, {2}", ffmpeg.ProcessName, ffmpeg.ExitTime, ffmpeg.ExitCode));
////        //};
////        ffmpeg.Start();

////        //if (this.InvokeRequired)
////        //    this.Invoke((Action)(() => { UpdateRecControl(); }));
////        //else
////        UpdateRecControl();

////        isRecord = true;
////        //IntPtr handle;
////        //do
////        //{
////        //    Thread.Sleep(10);
////        //    handle = ffmpeg.MainWindowHandle;
////        //}
////        //while (handle== IntPtr.Zero);

////        //IntPtr hSystemMenu = GetSystemMenu(handle, false);
////        //EnableMenuItem(hSystemMenu, SC_CLOSE, MF_GRAYED);
////        //RemoveMenu(hSystemMenu, SC_CLOSE, MF_BYCOMMAND);



////    }
////} 
//#endregion
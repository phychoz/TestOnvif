using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Onvif.Controls;
using TestOnvif;
using TestOnvif.deviceio;
//using TestOnvif.F
namespace Onvif.Controls
{
    public interface IMediaForm
    {
        void UpdateControls();
    }

    interface IMediaFormManager
    {
        void AddForm(IMediaForm form);
        void RemoveForm(IMediaForm form);
        void UpdateControls();
    }


    class MediaServiceController : IMediaService, IMediaFormManager
    {
        private MediaServiceController() 
        { 
            service = new MediaService();  
            MediaFormManager = this; 
            MediaForms = new List<IMediaForm>();
        }

        private static MediaServiceController controller = null;

        public static MediaServiceController Controller
        {
            get
            {
                if (controller == null)
                {
                    controller = new MediaServiceController();
                }
                return controller;
            }
        }

        private MediaService service = null;

        public bool IsConnected
        {
            get { return service.IsConnected; }
        }

        public bool IsStreaming
        {
            get { return service.IsStreaming; }
        }

        public ONVIFAgent ONVIF
        {
            get { return service.MediaDevice.ONVIF; }
        }

        private VideoForm videoForm;
        private MainForm mainForm;

        public IMediaFormManager MediaFormManager = null;
        private List<IMediaForm> MediaForms = null;

        public MainForm CreateMainForm()
        {
            mainForm = new MainForm();
            mainForm.UpdateControls();

            MediaFormManager.AddForm(mainForm);

            return mainForm;
        }

        public object[] FindDevices()
        {
            object[] collection = service.FindDevices();

            mainForm.BindMediaDeviceCollection(collection.Cast<MediaDevice>().ToArray());

            MediaFormManager.UpdateControls();

            return collection;
        }

        public bool Connect(MediaDevice device, string login, string password)
        {
            bool Result = service.Connect(device, login, password);

            if (Result == true)
            {
                Profile[] profiles = device.ONVIF.MediaProfiles;

                mainForm.BindMediaProfileCollection(profiles);

                MediaFormManager.UpdateControls();

            }
            return Result;
        }

        public void Disconnect()
        {
            service.Disconnect();

            if (videoForm != null)
            {
                MediaDevice device = service.MediaDevice;

                //device.AVProcessor.ShowVideo -= videoForm.ShowVideo;
                //device.AVProcessor.PlayAudio -= videoForm.PlayAudio;

                //device.AVProcessor.ShowVideo -= videoForm.ShowVideo;
                //device.AVProcessor.PlayAudio -= videoForm.PlayAudio;

                videoForm.Close();
                MediaFormManager.RemoveForm(videoForm);
            };

            MediaFormManager.UpdateControls();
        }

        public bool Start(Profile profile)
        {
            bool Result = service.Start(profile);

            if (Result == true)
            {
                MediaDevice device = service.MediaDevice;

                service.VideoDataReady +=service_VideoDataReady; //new EventHandler(service_VideoDataReady);
                service.AudioDataReady += service_AudioDataReady; //new EventHandler(service_AudioDataReady);

                string uri = device.ONVIF.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri;
                string filename = "";//device.AVProcessor.FFmpegMedia.OutputFilename;

                int width = 640;//device.AVProcessor.InVideoParams.Width;
                int height = 480;//device.AVProcessor.InVideoParams.Height;

                videoForm = new VideoForm(uri, filename, width, height);
                if (videoForm != null)
                {
                    //device.AVProcessor.ShowVideo += videoForm.ShowVideo;
                    //device.AVProcessor.PlayAudio += videoForm.PlayAudio;

                    MediaFormManager.AddForm(videoForm);

                    videoForm.Show();
                }
                MediaFormManager.UpdateControls();
            }

            return Result;
        }

        private void service_VideoDataReady(object sender, VideoDataEventArgs e)
        {
            OnVideoDataReady(e.Bitmap);
        }

        private void service_AudioDataReady(object sender, AudioDataEventArgs e)
        {
            OnAudioDataReady(e.Ptr, e.Size);
        }


        public void Stop()
        {
            service.Stop();

            service.VideoDataReady -=service_VideoDataReady;
            service.AudioDataReady -= service_AudioDataReady; 

            if (videoForm != null)
            {
                //MediaDevice device = service.MediaDevice;

                //device.AVProcessor.ShowVideo -= videoForm.ShowVideo;
                //device.AVProcessor.PlayAudio -= videoForm.PlayAudio;

                videoForm.Close();
                MediaFormManager.RemoveForm(videoForm);
            };

            MediaFormManager.UpdateControls();

        }

        public object ExecuteCommand(string command, object[] parameters = null)
        {
            return service.ExecuteCommand(command, parameters);
        }

        public event EventHandler<VideoDataEventArgs> VideoDataReady;
        public event EventHandler<AudioDataEventArgs> AudioDataReady;

        private void OnVideoDataReady(System.Drawing.Bitmap bitmap)
        {
            if (VideoDataReady != null)
                VideoDataReady(this, new VideoDataEventArgs { Bitmap = bitmap });
        }

        private void OnAudioDataReady(IntPtr ptr, int size)
        {
            if (AudioDataReady != null)
                AudioDataReady(this, new AudioDataEventArgs { Ptr = ptr, Size = size });
        }


        public void AddForm(IMediaForm form)
        {
            MediaForms.Add(form);
        }

        public void RemoveForm(IMediaForm form)
        {
            MediaForms.Remove(form);
        }

        public void UpdateControls()
        {
            foreach (IMediaForm form in MediaForms)
            {
                form.UpdateControls();
            }
        }
    }
}

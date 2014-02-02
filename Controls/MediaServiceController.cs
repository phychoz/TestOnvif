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


    class MediaServiceController : IMediaFormManager
    {
        private MediaServiceController() { MediaFormManager = this; }

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

        private VideoForm videoForm;
        private MainForm mainForm;

        public IMediaFormManager MediaFormManager = null;
        private List<IMediaForm> MediaForms = new List<IMediaForm>();

        public MainForm CreateMainForm()
        {
            mainForm = new MainForm();
            MediaFormManager.AddForm(mainForm);

            return mainForm;
        }

        public void FindDevices()
        {
            MediaDevice[] collection = MediaService.Instance.FindDevices();

            mainForm.BindMediaDeviceCollection(collection);

            MediaFormManager.UpdateControls();
        }

        public void Connect(MediaDevice device, string login, string password)
        {

            if (MediaService.Instance.Connect(device, login, password) == true)
            {

                Profile[] profiles = device.ONVIFClient.MediaProfiles;

                mainForm.BindMediaProfileCollection(profiles);

                MediaFormManager.UpdateControls();

            }
        }

        public void Disconnect()
        {
            MediaService.Instance.Disconnect();
            MediaFormManager.UpdateControls();
        }

        public void Start(Profile profile)
        {
            MediaService.Instance.Start(profile);

            MediaDevice device = MediaService.Instance.MediaDevice;

            string uri = device.ONVIFClient.GetCurrentMediaProfileRtspStreamUri().AbsoluteUri;
            string filename = device.AVProcessor.FFmpegMedia.OutputFilename;

            int width = device.AVProcessor.InVideoParams.Width;
            int height = device.AVProcessor.InVideoParams.Height;

            videoForm = new VideoForm(uri, filename, width, height);
            if (videoForm != null)
            {
                device.AVProcessor.ShowVideo += videoForm.ShowVideo;
                device.AVProcessor.PlayAudio += videoForm.PlayAudio;

                MediaFormManager.AddForm(videoForm);

                videoForm.Show();
            }
            MediaFormManager.UpdateControls();

        }

        public void Stop()
        {
            MediaService.Instance.Stop();
            if (videoForm != null)
            {
                MediaDevice device = MediaService.Instance.MediaDevice;
                device.AVProcessor.ShowVideo -= videoForm.ShowVideo;
                device.AVProcessor.PlayAudio -= videoForm.PlayAudio;

                videoForm.Close();
                MediaFormManager.RemoveForm(videoForm);
            };

            MediaFormManager.UpdateControls();

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpegWrapper;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Discovery;
using System.Xml;

namespace TestOnvif
{
    public class ONVIFClient : MediaClient
    {
        public ONVIFClient(MediaDevice device) : base(device) { }

        deviceio.DeviceClient deviceioClient;

        ptz.PTZClient ptzClient;

        ptz20.PTZClient ptzClient20;

        deviceio.MediaClient mediaClient;
        deviceio.Profile[] mediaProfiles;

        public deviceio.Profile[] MediaProfiles
        {
            get { return mediaProfiles; }
            set { mediaProfiles = value; }
        }

        //deviceio.Profile currentMediaProfile;

        public deviceio.Profile CurrentMediaProfile
        {
            get
            {
                return mediaProfiles[mediaProfileIndex]; ;
            }
            //set { currentMediaProfile = value; }
        }

        int mediaProfileIndex = 0;

        public int MediaProfileIndex
        {
            get { return mediaProfileIndex; }
            set { mediaProfileIndex = value; }
        }

        DeviceInformation deviceInformation;

        public DeviceInformation DeviceInformation
        {
            get { return deviceInformation; }
            set { deviceInformation = value; }
        }


        internal void Connect(string login, string password)
        {
            PasswordDigestBehavior passwordDigestBehavior = new PasswordDigestBehavior(login, password);

            HttpTransportBindingElement httpBinding = new HttpTransportBindingElement()
            {
                AuthenticationScheme = AuthenticationSchemes.Digest
            };

            //stringUri = System.Configuration.ConfigurationManager.AppSettings["Uri"];

            EndpointAddress endpointAddress = new EndpointAddress(MediaDevice.MediaDeviceUri);

            TextMessageEncodingBindingElement messageElement = new TextMessageEncodingBindingElement()
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None)
            };

            CustomBinding binding = new CustomBinding(messageElement, httpBinding);
            deviceioClient = new deviceio.DeviceClient(binding, endpointAddress);


            if (deviceioClient.Endpoint.Behaviors.FirstOrDefault(b => (b as PasswordDigestBehavior) != null) == null)
                deviceioClient.Endpoint.Behaviors.Add(passwordDigestBehavior);

            ptzClient = new ptz.PTZClient(binding, endpointAddress);

            ptzClient20 = new ptz20.PTZClient(binding, endpointAddress);

            mediaClient = new deviceio.MediaClient(binding, endpointAddress);

            if (mediaClient.Endpoint.Behaviors.FirstOrDefault(b => (b as PasswordDigestBehavior) != null) == null)
                mediaClient.Endpoint.Behaviors.Add(passwordDigestBehavior);

            deviceInformation = new DeviceInformation();

            mediaProfiles = mediaClient.GetProfiles();

            // currentMediaProfile = mediaProfiles[mediaProfileIndex];

        }

        internal void Disconnect()
        {

        }


        public Uri GetCurrentMediaProfileRtspStreamUri()
        {
            deviceio.StreamSetup deviceSetup = new deviceio.StreamSetup()
            {
                Stream = deviceio.StreamType.RTPUnicast,
                Transport = new deviceio.Transport()
            };
            deviceSetup.Transport.Protocol = deviceio.TransportProtocol.RTSP;

            return new Uri(mediaClient.GetStreamUri(deviceSetup, CurrentMediaProfile.token).Uri);
        }

        public CodecParams GetInputCodecParams()
        {
            return new CodecParams(GetVideoCodecType(CurrentMediaProfile.VideoEncoderConfiguration.Encoding),
                                    CurrentMediaProfile.VideoEncoderConfiguration.Resolution.Width,
                                    CurrentMediaProfile.VideoEncoderConfiguration.Resolution.Height);
        }

        private CodecType GetVideoCodecType(deviceio.VideoEncoding type)
        {
            switch (CurrentMediaProfile.VideoEncoderConfiguration.Encoding)
            {
                case deviceio.VideoEncoding.JPEG:
                    return CodecType.JPEG;

                case deviceio.VideoEncoding.H264:
                    return CodecType.H264;

                case deviceio.VideoEncoding.MPEG4:
                    return CodecType.MPEG4;

                default:
                    return CodecType.JPEG;
            }
        }


        public string GetDeviceInformation()
        {
            string result = String.Empty;
            string firmware = String.Empty;
            string serial = String.Empty;
            string hardware = String.Empty;
            string model = String.Empty;

            if (deviceioClient != null)
            {
                try
                {
                    deviceioClient.GetDeviceInformation(out model, out firmware, out serial, out hardware);

                    deviceInformation.Model = model;
                    deviceInformation.Firmware = firmware;
                    deviceInformation.Serial = serial;
                    deviceInformation.Hardware = hardware;

                    result = string.Format("Model {0}, FirmwareVersion {1}, SerialNumber {2}, HardwareId {3}",
                                     model, firmware, serial, hardware);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            return result;
        }

        //public string GetSystemDateAndTime()
        //{
        //    string result = String.Empty;
        //    if (deviceioClient != null)
        //    {
        //        try
        //        {
        //            deviceio.SystemDateTime curDeviceTime = deviceioClient.GetSystemDateAndTime();

        //            DateTime dateTime = new DateTime(curDeviceTime.LocalDateTime.Date.Year,
        //                curDeviceTime.LocalDateTime.Date.Month,
        //                curDeviceTime.LocalDateTime.Date.Day,
        //                curDeviceTime.LocalDateTime.Time.Hour,
        //                curDeviceTime.LocalDateTime.Time.Minute,
        //                curDeviceTime.LocalDateTime.Time.Second);

        //            result = dateTime.ToString("MM/dd/yyyy HH:mm:ss");
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Write(ex);
        //        }
        //    }
        //    return result;
        //}

        public DateTime? GetSystemDateAndTime()
        {
            DateTime? dateTime = null;
            if (deviceioClient != null)
            {
                try
                {
                    deviceio.SystemDateTime curDeviceTime = deviceioClient.GetSystemDateAndTime();

                    dateTime = new DateTime(curDeviceTime.LocalDateTime.Date.Year,
                    curDeviceTime.LocalDateTime.Date.Month,
                    curDeviceTime.LocalDateTime.Date.Day,
                    curDeviceTime.LocalDateTime.Time.Hour,
                    curDeviceTime.LocalDateTime.Time.Minute,
                    curDeviceTime.LocalDateTime.Time.Second);

                    //result = dateTime.ToString("MM/dd/yyyy HH:mm:ss");
                }
                catch (Exception ex)
                {
                    dateTime = null;
                    Logger.Write(ex);
                }
            }
            return dateTime;
        }

        public void SetSystemDateAndTime(DateTime time)
        {
            if (deviceioClient == null) return;
            try
            {
                //DateTime curSystemTime = DateTime.Now;
                deviceio.TimeZone timeZone = new deviceio.TimeZone() { TZ = @"UTC+0" };
                deviceio.DateTime curDeviceTime = new deviceio.DateTime()
                {
                    Date = new deviceio.Date()
                    {
                        Year = time.Date.Year,
                        Month = time.Date.Month,
                        Day = time.Date.Day,
                    }
                    ,
                    Time = new deviceio.Time()
                    {
                        Hour = time.Hour,
                        Minute = time.Minute,
                        Second = time.Second
                    }
                };

                deviceioClient.SetSystemDateAndTime(deviceio.SetDateTimeType.Manual, false, timeZone, curDeviceTime);

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

        }

        public void SetSystemDateAndTimeNTP(string server, string utc)
        {
            if (deviceioClient == null) return;
            try
            {
                deviceio.TimeZone timeZone = new deviceio.TimeZone() { TZ = utc };

                deviceio.NetworkHost[] ntp = new deviceio.NetworkHost[] 
                {
                    new deviceio.NetworkHost()
                    { 
                        Type = deviceio.NetworkHostType.IPv4, IPv4Address = server 
                    } 
                };

                DateTime time = DateTime.Now;
                deviceio.DateTime curDeviceTime = new deviceio.DateTime()
                {
                    Date = new deviceio.Date()
                    {
                        Year = time.Date.Year,
                        Month = time.Date.Month,
                        Day = time.Date.Day,
                    }
                    ,
                    Time = new deviceio.Time()
                    {
                        Hour = time.Hour,
                        Minute = time.Minute,
                        Second = time.Second
                    }
                };

                deviceioClient.SetNTP(false, ntp);
                deviceioClient.SetSystemDateAndTime(deviceio.SetDateTimeType.NTP, false, timeZone, curDeviceTime);

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

        }


        public string GetHostname()
        {
            string result = String.Empty;
            string hostName = String.Empty;
            if (deviceioClient != null)
            {
                try
                {
                    deviceio.HostnameInformation hostNameInfo = deviceioClient.GetHostname();
                    hostName = hostNameInfo.Name;
                    deviceInformation.HostName = hostName;
                    result = string.Format("Name {0}", hostName);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            return result;
        }



        private void GetConfiguration()
        {
            if (ptzClient != null)
            {
                try
                {
                    ptz.PTZConfiguration[] ptzConfigurationsList = ptzClient.GetConfigurations();

                    ptz.PTZConfiguration ptzConfiguration = ptzConfigurationsList[0];

                    ptz.PTZConfigurationOptions ptzConfigurationOptions = ptzClient.GetConfigurationOptions(ptzConfiguration.token);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
        }


        public void MoveAndZoomCamera20(float x, float y, float z)
        {
            try
            {
                ptz20.Vector2D panTilt = new ptz20.Vector2D();
                ptz20.Vector1D zoom = new ptz20.Vector1D();

                panTilt.x = x;
                panTilt.y = y;
                zoom.x = z;

                ptz20.PTZSpeed ptzSpeed = new ptz20.PTZSpeed() { PanTilt = panTilt, Zoom = zoom };

                ptzClient20.ContinuousMove(mediaProfiles[0].token, ptzSpeed, "PT1S");
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            //ptzClient.Stop(mediaProfiles[0].token, true, true);
        }

        public void MoveAndZoomCamera(float x, float y, float z)
        {
            try
            {
                ptz.Vector2D panTilt = new ptz.Vector2D();
                ptz.Vector1D zoom = new ptz.Vector1D();

                panTilt.x = x;
                panTilt.y = y;
                zoom.x = z;

                ptz.PTZSpeed ptzSpeed = new ptz.PTZSpeed() { PanTilt = panTilt, Zoom = zoom };

                ptzClient.ContinuousMove(mediaProfiles[0].token, ptzSpeed, "PT1S");
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            //ptzClient.Stop(mediaProfiles[0].token, true, true);
        }

        public string Reboot()
        {
            string result = String.Empty;
            try
            {
                result = deviceioClient.SystemReboot();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                result = ex.Message;
            }
            return result;
        }




        public static string GetDeviceInformation(Uri uri)
        {
            HttpTransportBindingElement httpBinding = new HttpTransportBindingElement()
            {
                AuthenticationScheme = AuthenticationSchemes.Digest//AuthenticationSchemes.Digest
            };

            EndpointAddress endpointAddress = new EndpointAddress(uri);

            TextMessageEncodingBindingElement messageElement = new TextMessageEncodingBindingElement()
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None)
            };

            CustomBinding binding = new CustomBinding(messageElement, httpBinding);

            deviceio.DeviceClient deviceioDeviceClient = new deviceio.DeviceClient(binding, endpointAddress);

            string result = String.Empty;

            string firmware = String.Empty;
            string serial = String.Empty;
            string hardware = String.Empty;
            string model = String.Empty;

            if (deviceioDeviceClient != null)
            {
                try
                {
                    deviceioDeviceClient.GetDeviceInformation(out model, out firmware, out serial, out hardware);

                    result = string.Format("{0}", model);

                    //result = string.Format("Model {0}, FirmwareVersion {1}, SerialNumber {2}, HardwareId {3}",
                    //                 model, firmware, serial, hardware);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                    result = string.Format("{0} GetDeviceInformation() error....");
                }
            }

            return result;
        }

        public static MediaDevice[] GetAvailableMediaDevices()
        {
            UdpDiscoveryEndpoint udpDiscoveryEndpoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            DiscoveryClient discoveryClientc = new DiscoveryClient(udpDiscoveryEndpoint);
            FindCriteria findCriteria = new FindCriteria();
            findCriteria.ContractTypeNames.Add(new XmlQualifiedName("NetworkVideoTransmitter", @"http://www.onvif.org/ver10/network/wsdl"));
            findCriteria.Duration = TimeSpan.FromSeconds(1);
            findCriteria.MaxResults = 15;
            FindResponse findResponse = discoveryClientc.Find(findCriteria);

            MediaDevice[] cameras = new MediaDevice[findResponse.Endpoints.Count];

            for (int index = 0; index < cameras.Length; index++)
            {
                Uri uri = findResponse.Endpoints[index].ListenUris.First(x => x.HostNameType == UriHostNameType.IPv4);

                string name = ONVIFClient.GetDeviceInformation(uri);

                cameras[index] = new MediaDevice(name, uri);
            }

            return cameras;
        }

    }
}

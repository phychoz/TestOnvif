using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /*
            v=  (protocol version)
            o=  (owner/creator and session identifier).
            s=  (session name)
          
            i=* (session information)
            u=* (URI of description)
            e=* (email address)
            p=* (phone number)
            c=* (connection information - not required if included in all media)
            b=* (bandwidth information)
          
            One or more time descriptions (see below)
          
            z=* (time zone adjustments)
            k=* (encryption key)
            a=* (zero or more session attribute lines)
          
            Zero or more media descriptions (see below)
   */

    class SessionDescription
    {
        public string ProtocolVersion { get; private set; }

        public string Owner { get; private set; }

        public string SessionName { get; private set; }

        public string SessionInformation { get; private set; }

        public string URI { get; private set; }

        public string EmailAddress { get; private set; }

        public string PhoneNumber { get; private set; }

        public string ConnectionInformation { get; private set; }

        public string BandwidthInformation { get; private set; }

        public List<TimeDescription> TimeDescriptions = null;

        public string TimeZoneAdjustments { get; private set; }

        public string EncryptionKey { get; private set; }

        public List<string> SessionAttributes = null;

        private List<MediaDescription> MediaDescriptions = null;

        public MediaDescription GetMediaDescriptionByName(string Name)
        {
            return MediaDescriptions.FirstOrDefault(media => media.MediaNameAndTransportAddress.Contains(Name) == true);
        }

        public SessionDescription()
        {
            TimeDescriptions = new List<TimeDescription>();
            SessionAttributes = new List<string>();
            MediaDescriptions = new List<MediaDescription>();
        }

        private MediaDescription LastMediaDescription
        {
            get { return this.MediaDescriptions.Last(); }
        }

        private bool SessionMode = true;

        public void Add(string type, string value)
        {
            switch (type)
            {
                case "v=":
                    this.ProtocolVersion = value;
                    break;
                case "o=":
                    this.Owner = value;
                    break;
                case "s=":
                    this.SessionName = value;
                    break;
                case "i=":
                    {
                        if (SessionMode == false)
                        {
                            this.LastMediaDescription.Add(type, value);
                        }
                        else
                        {
                            this.SessionInformation = value;
                        }
                    }
                    break;
                case "u=":
                    this.URI = value;
                    break;
                case "e=":
                    this.EmailAddress = value;
                    break;
                case "p=":
                    this.PhoneNumber = value;
                    break;
                case "c=":
                    {
                        if (this.SessionMode == false)
                        {
                            this.LastMediaDescription.Add(type, value);
                        }
                        else
                        {
                            this.ConnectionInformation = value;
                        }
                    }
                    break;
                case "b=":
                    {
                        if (this.SessionMode == false)
                        {
                            this.LastMediaDescription.Add(type, value);
                        }
                        else
                        {
                            this.BandwidthInformation = value;
                        }
                    }
                    break;
                case "t=":
                    {
                        this.TimeDescriptions.Add(new TimeDescription(value));
                        break;
                    }
                case "r=":
                    {
                        if (this.TimeDescriptions.Count > 0)
                        {
                            TimeDescription lastTimeDescription = this.TimeDescriptions.Last();
                            if (lastTimeDescription != null)
                                lastTimeDescription.Repetitions.Add(value);
                        }
                        else
                        {
                            //...
                        }
                    }
                    break;
                case "z=":
                    this.TimeZoneAdjustments = value;
                    break;

                case "k=":
                    {
                        if (this.SessionMode == false)
                        {
                            this.LastMediaDescription.Add(type, value);
                        }
                        else
                        {
                            this.EncryptionKey = value;
                        }
                    }
                    break;

                case "a=":
                    {
                        if (this.SessionMode == false)
                        {
                            this.LastMediaDescription.Add(type, value);
                        }
                        else
                        {
                            this.SessionAttributes.Add(value);
                        }
                    }
                    break;

                case "m=":
                    {
                        if (this.SessionMode == true) this.SessionMode = false; // 
                        this.MediaDescriptions.Add(new MediaDescription(value));
                        break;
                    }

                default:
                    //...
                    break;
            }
        }

    }
}

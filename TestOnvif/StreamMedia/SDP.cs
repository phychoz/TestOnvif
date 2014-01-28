using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /// <summary>
    /// Класс для объектного представления данных в формате SDP
    /// <see cref="http://www.faqs.org/rfcs/rfc2327.html"/>
    /// </summary>
    class SDP
    {
        private SDP()
        {
            Session = new SessionDescription();
        }
        public SessionDescription Session = null;

        public static SDP Parse(string str)
        {
            SDP sdp = new SDP();
            string[] lines = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) == true) continue;

                if (line.Length < 2) continue;

                string type = line.Substring(0, 2);
                string value = line.Substring(2);

                sdp.Session.Add(type, value);
            }

            return sdp;
        }
    }

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
                        if (SessionMode==false)
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
                        if (this.SessionMode ==false)
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

    /*        
       t=  (time the session is active)     
       r=* (zero or more repeat times)
     */
    class TimeDescription
    {
        public string Time { get; private set; }
        public List<string> Repetitions = null;

        public TimeDescription()
        {
            Repetitions = new List<string>();
        }

        public TimeDescription(string time)
        {
            Time = time;
            Repetitions = new List<string>();
        }
    }

    /*
        m=  (media name and transport address)    
        i=* (media title)
        c=* (connection information - optional if included at session-level)
        b=* (bandwidth information)
        k=* (encryption key)
        a=* (zero or more media attribute lines)
     */
    public class MediaDescription
    {
        public string MediaNameAndTransportAddress { get; private set; }
        public string MediaTitle { get; private set; }

        public string ConnectionInformation { get; private set; }

        public string BandwidthInformation { get; private set; }

        public string EncryptionKey { get; private set; }

        private List<MediaAttribute> MediaAttributes = null;

        public string GetAttributeValueByName(string AttributeName)
        {
            string value = string.Empty;
            var attribute = MediaAttributes.FirstOrDefault(a => a.Name == AttributeName);
            if (attribute != null)
                value = attribute.Value;

            return value;
        }

        public MediaDescription()
        {
            MediaAttributes = new List<MediaAttribute>();
        }

        public MediaDescription(string name)
        {
            MediaNameAndTransportAddress = name;
            MediaAttributes = new List<MediaAttribute>();
        }

        public void Add(string type, string value)
        {
            switch (type)
            {
                case "i=":
                    this.MediaTitle = value;
                    break;

                case "c=":
                    this.ConnectionInformation = value;
                    break;

                case "b=":
                    this.BandwidthInformation = value;
                    break;

                case "k=":

                    this.EncryptionKey = value;
                    break;

                case "a=":
                    //MediaAttribute attribute = MediaAttributeFactory.Create(value);
                    //if(attribute!=null)
                    //    this.MediaAttributes.Add(attribute);
                    this.MediaAttributes.Add(MediaAttribute.Create(value));
                    break;

                case "m=":
                    {
                        this.MediaNameAndTransportAddress = value;
                        break;
                    }

                default:
                    //...
                    break;
            }
        }
    }

    public class MediaAttribute
    {
        private string name;

        public string Name
        {
            get { return name; }
        }
        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public static MediaAttribute Create(string Param)
        {
            MediaAttribute attribute = null;
            string[] splits = Param.Split(':');
            if (splits.Count() == 2)
            {
                string name = splits[0];
                string value = splits[1];

                attribute = new MediaAttribute(name, value);
            }
            else if (splits.Count() == 1)
            {
                string name = splits[0];
                attribute = new MediaAttribute(name);
            }
            return attribute;

        }

        MediaAttribute(string name)
        {
            this.name = name;
        }

        MediaAttribute(string _name, string _value)
        {
            this.name = _name;
            this.value = _value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
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

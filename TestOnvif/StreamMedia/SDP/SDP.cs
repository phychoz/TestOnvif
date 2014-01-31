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
    public class SDP
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

    public static class exSDP
    {
        public static string GetCodec(this SDP sdp, string MediaType)
        {
            string codec = string.Empty;

            MediaDescription description = sdp.Session.GetMediaDescriptionByName(MediaType);

            string attribute = description.GetAttributeValueByName("rtpmap");
            if (string.IsNullOrEmpty(attribute) == false)
            {
                string[] split = attribute.Split(' ');
                if (split.Length == 2)
                {
                    string[] values = split[1].Split('/');
                    codec = values[0];
                }
            }
            return codec;
        }

        public static int GetSampleRate(this SDP sdp, string MediaType)
        {
            int result = int.MaxValue;

            MediaDescription description = sdp.Session.GetMediaDescriptionByName(MediaType);

            string attribute = description.GetAttributeValueByName("rtpmap");
            if (string.IsNullOrEmpty(attribute) == false)
            {
                string[] split = attribute.Split(' ');
                if (split.Length == 2)
                {
                    string[] values = split[1].Split('/');
                    string str = values[1];

                    int.TryParse(str, out result);
                }
            }

            return result;
        }

        public static string GetControl(this SDP sdp, string MediaType)
        {
            return sdp.Session.GetMediaDescriptionByName(MediaType).GetAttributeValueByName("control");
        }
    }   
}

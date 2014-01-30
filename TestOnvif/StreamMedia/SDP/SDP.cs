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
}

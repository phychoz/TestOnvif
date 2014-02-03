using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif.Tools
{

    public class DeviceInformation
    {
        string hostName = String.Empty;

        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }
        string firmware = String.Empty;

        public string Firmware
        {
            get { return firmware; }
            set { firmware = value; }
        }

        string serial = String.Empty;

        public string Serial
        {
            get { return serial; }
            set { serial = value; }
        }
        string hardware = String.Empty;

        public string Hardware
        {
            get { return hardware; }
            set { hardware = value; }
        }

        string model = String.Empty;

        public string Model
        {
            get { return model; }
            set { model = value; }
        }

    }
}

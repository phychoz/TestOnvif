using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    abstract class RFCHandler
    {
        public abstract void HandleRtpPacket(RtpPacket packet);

        public event FrameReceivedHandler FrameReceived;
        public delegate void FrameReceivedHandler(IntPtr data, int count, bool key, uint pts);
        protected void OnFrameReceived(IntPtr data, int count, bool key, uint pts)
        {
            if (FrameReceived != null)
                FrameReceived(data, count, key, pts);
        }
    }

    class RFCHandlerFactory
    {
        public static RFCHandler Create(string RfcHandlerName)
        {
            RFCHandler handler = null;
            switch (RfcHandlerName)
            {
                case "JPEG":
                    handler = new RFC2435Handler();
                    break;

                case "MP4V-ES":
                    handler = new RFC3016Handler();
                    break;

                case "H264":
                    handler = new RFC3984Handler();
                    break;

                case "G711":
                    handler = new RFC5391Handler();
                    break;

                default:
                    //...
                    break;

            }
            return handler;
        }
    }

}

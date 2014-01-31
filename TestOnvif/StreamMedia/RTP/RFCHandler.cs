using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace TestOnvif
{
    abstract class PayloadHandler
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

    class PayloadHandlerFactory
    {
        public static PayloadHandler Create(string RfcHandlerName)
        {
            PayloadHandler handler = null;
            switch (RfcHandlerName)
            {
                case "JPEG":
                    handler = new RFC2435PayloadHandler();
                    break;

                case "MP4V-ES":
                    handler = new RFC3016PayloadHandler();
                    break;

                case "H264":
                    handler = new RFC3984PayloadHandler();
                    break;

                case "PCMU":
                    handler = new RFC5391PayloadHandler();
                    break;

                default:
                    //...
                    break;

            }
            return handler;
        }
    }

}

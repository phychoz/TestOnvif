using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// RTP Payload Format for ITU-T Recommendation G.711.1
    /// http://tools.ietf.org/rfc/rfc5391.txt
    /// </summary>
    class RFC5391Handler : RFCHandler
    {
        //public event AudioDataReceivedHandler AudioDataReceived;
        //public delegate void AudioDataReceivedHandler(byte [] data);

        //G711Decoder g711Decoder = new G711Decoder();

        public override void HandleRtpPacket(RtpPacket packet)
        {
            //int length = packet.PayloadLength;
            //byte[] payload = new byte[length];

            //Marshal.Copy(packet.Payload, payload, 0, length);

            //byte[] outWav = g711Decoder.Decode(payload);
            //AudioDataReceived(outWav);

            OnFrameReceived(packet.Payload, packet.PayloadLength, false, packet.Timestamp);
            
        }
    }
}

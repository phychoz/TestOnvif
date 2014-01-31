using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// RTP Payload Format for MPEG-4 Audio/Visual Streams
    /// http://tools.ietf.org/rfc/rfc3016.txt
    /// </summary>
    class RFC3016PayloadHandler:PayloadHandler
    {
        /// <summary>
        /// Приемный буффер, подбирается под размет принимаемого фрейма
        /// </summary>
        const int BUFFER_SIZE = 1000000;
        
        byte[] buffer = new byte[BUFFER_SIZE];

        int bufferOffset;

        bool isKeyFrame = false;

        public override void HandleRtpPacket(RtpPacket packet)
        {
            if (packet.IsMarker==false)
            {
                if ((Marshal.ReadByte(packet.Payload, 0) == 0 && 
                    Marshal.ReadByte(packet.Payload, 1) == 0 &&
                    Marshal.ReadByte(packet.Payload, 2) == 1) &&
                   (Marshal.ReadByte(packet.Payload, 3) == (byte)0xb6 || 
                    Marshal.ReadByte(packet.Payload, 3) == (byte)0xb0)) // начало фрейма {0,0,0,1,b6} или {0,0,0,1,b0}
                {
                    bufferOffset=0;
                }
                isKeyFrame=(((Marshal.ReadByte(packet.Payload, 4) & 0xc0) >> 6) == 0)?true:false; //ключевой кадр

                Marshal.Copy(packet.Payload, buffer, bufferOffset, packet.PayloadLength);
                bufferOffset+=packet.PayloadLength;

                return;
            }

            if (packet.IsMarker==true) //последний пакет фрейма
            {
                Marshal.Copy(packet.Payload, buffer, bufferOffset, packet.PayloadLength);
                bufferOffset += packet.PayloadLength;

                IntPtr ptr = Marshal.AllocHGlobal(bufferOffset);
                Marshal.Copy(buffer, 0, ptr, bufferOffset);

                OnFrameReceived(ptr, bufferOffset, isKeyFrame, packet.Timestamp);
                Marshal.FreeHGlobal(ptr);

                return;
            }
        }

    }
}
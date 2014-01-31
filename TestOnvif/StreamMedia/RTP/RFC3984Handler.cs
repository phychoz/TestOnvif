using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// RFC 3984 
    /// RTP Payload Format for H.264 Video
    /// http://www.ietf.org/rfc/rfc3984.txt
    /// </summary>
    class RFC3984PayloadHandler : PayloadHandler
    {
        /// <summary>
        /// Буффер, подбирается под размет принимаемого фрейма
        /// BUFFER_SIZE = (#pixels) x (8 bits) / (minimum compression ratio) + AUD, SPS, PPS, SEI,
        /// Для разрешения 640х480 BUFFER_SIZE=34000
        /// Для разрешения 1280х960 BUFFER_SIZE=48000
        /// </summary>
        private const int BUFFER_SIZE = 1000000;

        private byte[] buffer = new byte[BUFFER_SIZE];

        private int bufferOffset = 0;

        unsafe public override void HandleRtpPacket(RtpPacket packet)
        {
            int payloadLength = packet.PayloadLength;

            if (payloadLength == 0) return;

            int startSequenceLength = 4;
            byte* startSequence = stackalloc byte[startSequenceLength];

            startSequence[0] = 0;
            startSequence[1] = 0;
            startSequence[2] = 0;
            startSequence[3] = 1;

            byte firstByte = *(byte*)(packet.Payload + 0);
            byte unitType = (byte)(firstByte & 0x1f);

            if (unitType >= 1 && unitType <= 23)// не фрагметнированный пакет как правило AUD, SPS, PPS, SEI, 
            {
                Marshal.Copy((IntPtr)startSequence, buffer, bufferOffset, startSequenceLength);
                bufferOffset += startSequenceLength;//startSequence.Length;

                Marshal.Copy(packet.Payload, buffer, bufferOffset, payloadLength);
                bufferOffset += payloadLength;

                return;
            }
            if (unitType == 28) // фрагментированный пакет FU-A (IDR, non-IDR)
            {
                if (payloadLength > 1)
                {
                    byte secondByte=*(byte*)(packet.Payload + 1);
                    byte startBit = (byte)(secondByte >> 7);
                    byte endBit = (byte)((secondByte & 0x40) >> 6);

                    byte nalUnitType = (byte)(secondByte & 0x1f); // идентификатор пакета

                    int payloadDataOffset = 2;// смещение 2 байта firstByte+secondByte

                    if (startBit == 1 && endBit == 0) //начало фрагмента
                    {
                        Marshal.Copy((IntPtr)startSequence, buffer, bufferOffset, startSequenceLength);
                        bufferOffset += startSequenceLength;

                        buffer[bufferOffset] = (byte)((firstByte & 0xe0) | nalUnitType);
                        bufferOffset++;

                        Marshal.Copy(packet.Payload + payloadDataOffset, buffer, bufferOffset, payloadLength - payloadDataOffset);
                        bufferOffset += payloadLength - payloadDataOffset;
                        return;
                    }

                    if (startBit == 0 && endBit == 0) //середина 
                    {
                        Marshal.Copy(packet.Payload + payloadDataOffset, buffer, bufferOffset, payloadLength - payloadDataOffset);
                        bufferOffset += payloadLength - payloadDataOffset;
                        return;

                    }
                    if (startBit == 0 && endBit == 1) //конец
                    {
                        Marshal.Copy(packet.Payload + payloadDataOffset, buffer, bufferOffset, payloadLength - payloadDataOffset);
                        bufferOffset += payloadLength - payloadDataOffset;

                        IntPtr ptr = Marshal.AllocHGlobal(bufferOffset);
                        Marshal.Copy(buffer, 0, ptr, bufferOffset);

                        bool flag = (nalUnitType == 5) ? true : false;// ключевой кадр

                        OnFrameReceived(ptr, bufferOffset, flag, packet.Timestamp);

                        Marshal.FreeHGlobal(ptr);

                        bufferOffset = 0;
                    }
                }

                return;
            }
            // Эти пакеты приходить не должны !!!
            if (unitType == 24) // STAP-A
            {
                //...
                Logger.Write(String.Format("STAP-A unitType=={0}",unitType), EnumLoggerType.DebugLog);
                return;
            }

            if (unitType == 29)//FU-B
            {
                //...
                Logger.Write(String.Format("FU-B unitType=={0}", unitType), EnumLoggerType.DebugLog);
                return;
            }

            if (unitType == 25 || unitType == 26 || unitType == 27) // STAP-B ,MTAP-16, MTAP-24
                return;

            if (unitType == 0 || unitType == 30 || unitType == 31) // undefined
                return;


        }
    }
}

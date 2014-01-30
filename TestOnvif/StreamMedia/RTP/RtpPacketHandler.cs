using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace TestOnvif
{
    /// <summary>
    /// Класс для создания обработчиков входящего потока данных в RTP пакеты
    /// </summary>
    public class RtpPacketHandler
    {
        /// <summary>
        /// Маска для извлечения версии заголовка
        /// </summary>
        private const byte VERSION_MASK = 0xC0;         // 11000000b 

        /// <summary>
        /// Маска для извлечения флага заполнения
        /// </summary>
        private const byte PADDING_MASK = 0x20;         // 00100000b

        /// <summary>
        /// Маска для извлечения флага расширения
        /// </summary>
        private const byte EXTENSION_MASK = 0x10;       // 00010000b

        /// <summary>
        /// Маска для извлечения количества CSRC идентификаторов
        /// </summary>
        private const byte CSRCCOUNT_MASK = 0x0F;       // 00001111b

        /// <summary>
        /// Маска для извлечения флага маркера
        /// </summary>
        private const byte IS_MARKER_MASK = 0x80;       // 10000000b

        /// <summary>
        /// Маска для извлечения типа полезной нагрузки
        /// </summary>
        private const byte PAYLOAD_TYPE_MASK = 0x7F;    // 01111111b

        /// <summary>
        /// Буфер для полезной нагрузки RTP пакетов
        /// </summary>
        //private byte[] buffer = new byte[ushort.MaxValue];

        /// <summary>
        /// Формирует структуру RtpPacket и вызывает событие CreatedRtpPacket
        /// </summary>
        /// <param name="data">Указатель на область памяти 
        /// в неуправляемой куче, где содержится RTP пакет</param>
        /// <param name="count">Размер области памяти в байтах</param>
        unsafe public void HandleRtpPacket(IPEndPoint remoteEndPoint, IntPtr data, int count) //IntPtr data, int count)//
        {
            RtpPacket packet = new RtpPacket();

            int offset = 0;
            byte firstByte = *(byte*)(data + offset);
            offset++;
            byte secondByte = *(byte*)(data + offset);
            packet.Version = (byte)(firstByte >> 6);
            packet.Padding = (firstByte & PADDING_MASK) == PADDING_MASK;
            packet.HasExtension = (firstByte & EXTENSION_MASK) == EXTENSION_MASK;
            packet.CSRCCount = (byte)(firstByte & CSRCCOUNT_MASK);
            packet.IsMarker = (secondByte & IS_MARKER_MASK) == IS_MARKER_MASK;
            packet.PayloadType = (byte)(secondByte & PAYLOAD_TYPE_MASK);
            offset++;
            packet.SequenceNumber = BigEndian.ReadUInt16((void*)(data + offset));
            offset += 2;
            packet.Timestamp = BigEndian.ReadUInt32((void*)(data + offset));
            offset += 4;
            packet.SSRC = BigEndian.ReadUInt32((void*)(data + offset));

            if (RTSPSession.IsCurrentSessionSSRT(packet.SSRC) == false)
                return;

            if (packet.HasExtension)
            {
                offset = 12 + packet.CSRCCount * 4 + 0;
                packet.HeaderExtensionProfile = BigEndian.ReadUInt16((void*)(data+ offset));
                offset += 2;
                packet.HeaderExtensionLength = BigEndian.ReadUInt16((void*)(data+ offset));
                offset += 2;
                packet.HeaderExtension = data + offset;
                offset += packet.HeaderExtensionLength;
                packet.PayloadLength = count - offset;
                packet.Payload = data + offset;
            }
            else
            {
                offset = 12 + packet.CSRCCount * 4 + 0;
                packet.HeaderExtensionProfile = 0;
                packet.HeaderExtensionLength = 0;
                packet.HeaderExtension = IntPtr.Zero;
                packet.PayloadLength = count - offset;
                packet.Payload = data + offset;
            }

            RtpPacketRecieved(packet);
        }

        /// <summary>
        /// Событие вызывается при создании RTP пакета (в конце метода CreateRtpPacket)
        /// </summary>
        public event PacketHandler RtpPacketRecieved;

        /// <summary>
        /// Тип обработчика события создания RTP пакета
        /// </summary>
        /// <param name="packet">Созданный RTP пакет</param>
        public delegate void PacketHandler(RtpPacket packet);
    }
}

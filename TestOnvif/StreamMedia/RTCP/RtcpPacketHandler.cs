using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// </summary>
    class RtcpPacketHandler
    {
        /// <summary>
        /// Маска для извлечения флага заполнения
        /// </summary>
        private const byte _00100000 = 0x20;

        /// <summary>
        /// Маска для извлечения числа отчетов о приеме / количества источников и т. д.
        /// </summary>
        private const byte _00011111 = 0x1F;

        /// <summary>
        /// Буфер для полезной нагрузки RTP пакетов
        /// </summary>
        private byte[] buffer = new byte[ushort.MaxValue];

        /// <summary>
        /// Обрабатывает пришедшие данные и вызывает всякие события
        /// </summary>
        /// <param name="data">Указатель область памяти,
        /// где располагаются данные RTCP пакета</param>
        /// <param name="count">Размер области памяти в байтах</param>
        public void CreateRtcpPacket(IntPtr data, int count)
        {
            Marshal.Copy(data, buffer, 0, count);
            // сейчас в массиве buffer содержится один или более RTCP пакетов
            // их нужно обработать в цикле
            // сначала обрабатывается первый пакет, который находится в начале
            // буфера, потом данные в буфере сдвигаются к началу, и т. д.,
            // пока не будут обработаны все пакеты
            while (count > 0)
            {
                int version = buffer[0] >> 6;
                if (version != 2)
                    return;
                bool padding = (buffer[0] & _00100000) == _00100000;
                RtcpPacketType packetType = (RtcpPacketType)buffer[1];
                int length = (BigEndian.ReadUInt16(buffer, 2) + 1) * 4;

                switch (packetType)
                {
                    case RtcpPacketType.SenderReport:
                        RtcpSenderReportPacket packet = new RtcpSenderReportPacket();
                        packet.Version = (byte)(buffer[0] >> 6);
                        packet.Padding = ((buffer[0] & _00100000) == _00100000);
                        packet.ReceptionReportCount = (byte)(buffer[0] & _00011111);
                        packet.PayloadType = RtcpPacketType.SenderReport;
                        packet.SenderSSRC = BigEndian.ReadUInt32(buffer, 4);
                        packet.TimestampMSW = BigEndian.ReadUInt32(buffer, 8);
                        packet.TimestampLSW = BigEndian.ReadUInt32(buffer, 12);
                        packet.RTPTimestamp = BigEndian.ReadUInt32(buffer, 16);
                        packet.SenderPacketCount = BigEndian.ReadUInt32(buffer, 20);
                        packet.SenderOctetCount = BigEndian.ReadUInt32(buffer, 24);
                        CreatedRtcpSenderReport(packet);
                        break;
                    case RtcpPacketType.ReceiverReport:
                        // ничего не делать, т. к. нам не будут приходить пакеты такого типа
                        break;
                    case RtcpPacketType.SourceDescription:
                        // прочитать количество источников
                        int sourceCount = buffer[0] & _00011111;
                        int pos = 4;

                        RtcpSourceDescription[] result = new RtcpSourceDescription[sourceCount];

                        for (int i = 0; i < sourceCount; i++)
                        {
                            result[i] = new RtcpSourceDescription();
                            result[i].SSRC = BigEndian.ReadUInt32(buffer, pos);
                            pos += 4;
                            List<RtcpSourceDescriptionItem> items = new List<RtcpSourceDescriptionItem>();
                            while (buffer[pos] != 0)
                            {
                                RtcpSourceDescriptionType type = (RtcpSourceDescriptionType)buffer[pos];
                                pos += 1;
                                int itemLength = buffer[pos];
                                pos += 1;
                                string itemText = Encoding.UTF8.GetString(buffer, pos, itemLength);
                                pos += itemLength;
                                items.Add(new RtcpSourceDescriptionItem()
                                {
                                    Type = type,
                                    Text = itemText
                                });
                            }
                            result[i].Items = items.ToArray();

                            // выравниваем по 4 байтам
                            while (pos % 4 != 0)
                                pos++;
                        }

                        CreatedRtcpSourceDescription(result);
                        break;
                    case RtcpPacketType.Goodbye:
                        // ничего не делать
                        break;
                    case RtcpPacketType.ApplicationDefined:
                        // ничего не делать
                        break;
                }
                // смещаем оставшиеся данные в буфере к началу
                for (int i = length; i < count; i++)
                {
                    buffer[i - length] = buffer[i];
                    buffer[i] = 0;
                }
                count -= length;
            }
        }

        /// <summary>
        /// Событие вызывается при формировании отчета источника
        /// </summary>
        public event PacketHandler CreatedRtcpSenderReport;

        /// <summary>
        /// Событие вызывается при формировании описания источника
        /// </summary>
        public event SourceDescriptionHandler CreatedRtcpSourceDescription;

        /// <summary>
        /// Тип обработчика события получения отчета источника
        /// </summary>
        /// <param name="packet">Полученный RTCP пакет</param>
        public delegate void PacketHandler(RtcpSenderReportPacket packet);

        /// <summary>
        /// Тип обработчика события получения пакета типа Source Description
        /// </summary>
        /// <param name="sources">Описания всех источников</param>
        public delegate void SourceDescriptionHandler(RtcpSourceDescription[] sources);
    }
}

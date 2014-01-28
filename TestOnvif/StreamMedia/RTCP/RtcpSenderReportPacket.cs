using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /// <summary>
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// Читать RTP.pdf, "THE RTCP SR PACKET FORMAT".
    /// </summary>
    public class RtcpSenderReportPacket
    {
        /// <summary>
        /// Номер версии RTP
        /// Расположен в битах № 0-1 первого октета пакета
        /// </summary>
        public byte Version;

        /// <summary>
        /// Флаг заполнения нулями конца пакета
        /// Расположен в бите № 2 первого октета
        /// </summary>
        public bool Padding;

        /// <summary>
        /// Число блоков отчетов о приеме, содержащихся в этом пакете
        /// </summary>
        public byte ReceptionReportCount;

        /// <summary>
        /// Тип пакета. Данный пакет всегда имеет тип RtcpPacketType.SenderReport
        /// </summary>
        public RtcpPacketType PayloadType;

        /// <summary>
        /// Длина RTCP-пакета в 32-битных словах минус один, включая заголовок и заполнитель
        /// </summary>
        public ushort Length;

        /// <summary>
        /// Идентификатор источника синхронизации для отправителя пакета
        /// </summary>
        public uint SenderSSRC;

        /// <summary>
        /// Метка NTP в секундах начиная с 1 января 1900 года
        /// </summary>
        public uint TimestampMSW;

        /// <summary>
        /// Метка NTP младшая часть
        /// для перевода в секунды (uint)TimestampLSW/2^32
        /// </summary>
        public uint TimestampLSW;

        /// <summary>
        /// Временная метка RTP. Это число, равное initValue + ticksCount, где
        /// initValue - временная метка первого пришедшего RTP пакета в сеансе,
        /// ticksCount - число тиков таймера, прошедших с момента посылки
        /// первого RTP пакета. Для аудио частота таймера равна 8000 Hz (равна
        /// частоте дискретизации), для видео - 90000 Hz
        /// </summary>
        public uint RTPTimestamp;

        /// <summary>
        /// Число посланных RTP пакетов
        /// </summary>
        public uint SenderPacketCount;

        /// <summary>
        /// Полное число октетов поля данных (исключая заголовки и заполнители),
        /// переданных в информационных RTP-пакетах отправителем, начиная
        /// с начала передачи до момента генерации SR-пакета.  Это число
        /// сбрасывается в нуль, когда отправитель меняет свой
        /// SSRC-идентификатор. Это поле может быть использовано для оценки
        /// среднего потока данных.
        /// </summary>
        public uint SenderOctetCount;
    }
}

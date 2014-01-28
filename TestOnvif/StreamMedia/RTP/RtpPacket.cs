using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    // 0                   1                   2                   3
    // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|V=2|P|X|  CC   |M|     PT      |       sequence number         |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|                           timestamp                           |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|           synchronization source (SSRC) identifier            |
    //+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
    //|            contributing source (CSRC) identifiers             |
    //|                             ....                              |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //[
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|      defined by profile       |           length              |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //]
    //
    //
    /// <summary>
    /// Структура для обработки RTP-заголовка
    /// <see cref="http://www.ietf.org/rfc/rfc3550.txt"/> раздел 5.1
    /// </summary>
    public struct RtpPacket
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
        /// Флаг расширения RTP заголовка
        /// Расположен в бите № 3 в байте № 0
        /// </summary>
        public bool HasExtension;

        /// <summary>
        /// Число CSRC идентификаторов, следующих за заголовком
        /// Расположено в битах № 4-7 в байте № 0
        /// </summary>
        public byte CSRCCount;

        /// <summary>
        /// Флаг маркера
        /// Например, если он установлен в пакете h.264, значит данный
        /// пакет завершает один кадр
        /// Расположен в бите № 0 в байте № 1
        /// </summary>
        public bool IsMarker;

        /// <summary>
        /// Тип полезных данных
        /// Расположен в битах № 1-7 в байте № 1
        /// </summary>
        public byte PayloadType;

        /// <summary>
        /// Номер последовательности ?!
        /// Расположен в байтах № 2-3
        /// </summary>
        public ushort SequenceNumber;

        /// <summary>
        /// Временная метка
        /// Расположена в байтах № 4-7
        /// </summary>
        public uint Timestamp;

        /// <summary>
        /// Идентификатор источника синхронизации
        /// Расположен в байтах 8-11
        /// </summary>
        public uint SSRC;

        /// <summary>
        /// Определенная профилем дополнительная
        /// информация в расширении заголовка
        /// </summary>
        public ushort HeaderExtensionProfile;

        /// <summary>
        /// Длина расширения заголовка
        /// </summary>
        public ushort HeaderExtensionLength;

        /// <summary>
        /// Указатель на расширяющий заголовок
        /// </summary>
        public IntPtr HeaderExtension;

        /// <summary>
        /// Указатель на полезные данные
        /// </summary>
        public IntPtr Payload;
        //public byte[] Payload;
        /// <summary>
        /// Количество полезных данных в байтах
        /// </summary>
        public int PayloadLength;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /// <summary>
    /// Типы RTCP пакетов
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// </summary>
    public enum RtcpPacketType : byte
    {
        /// <summary>
        /// Отчет отправителя (камеры)
        /// </summary>
        SenderReport = 200,

        /// <summary>
        /// Отчет получателя (т. е. программы на PC)
        /// </summary>
        ReceiverReport = 201,

        /// <summary>
        /// Описание источника (камера / программа на PC)
        /// </summary>
        SourceDescription = 202,

        /// <summary>
        /// Завершение сеанса RTCP
        /// </summary>
        Goodbye = 203,

        /// <summary>
        /// Определено приложением
        /// </summary>
        ApplicationDefined = 204
    }
}

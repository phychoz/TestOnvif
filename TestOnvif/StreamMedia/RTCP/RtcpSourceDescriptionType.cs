using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
    /// <summary>
    /// Тип описания источника
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// </summary>
    public enum RtcpSourceDescriptionType : byte
    {
        /// <summary>
        /// Канонический идентификатор конечной системы
        /// </summary>
        CName = 1,

        /// <summary>
        /// Имя пользователя
        /// </summary>
        Name = 2,

        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        Email = 3,

        /// <summary>
        /// Телефонный номер
        /// </summary>
        Phone = 4,

        /// <summary>
        /// Географический адрес пользователя
        /// </summary>
        LOC = 5,

        /// <summary>
        /// Имя приложения или программного средства
        /// </summary>
        TOOL = 6,

        /// <summary>
        /// Уведомление/статус
        /// </summary>
        Note = 7,

        /// <summary>
        /// Элемент частного расширения SDES
        /// </summary>
        PRIV = 8
    }
}

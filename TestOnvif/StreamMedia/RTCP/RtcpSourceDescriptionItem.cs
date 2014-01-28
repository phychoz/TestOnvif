using System;
using System.Collections.Generic;
using System.Linq;

namespace TestOnvif
{
    /// <summary>
    /// Элемент описания источника SSRC
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// </summary>
    public class RtcpSourceDescriptionItem
    {
        /// <summary>
        /// Тип описания источника
        /// </summary>
        public RtcpSourceDescriptionType Type;

        /// <summary>
        /// Текст описания источника
        /// </summary>
        public string Text;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestOnvif
{
    /// <summary>
    /// Описание источника синхронизации
    /// </summary>
    public class RtcpSourceDescription
    {
        /// <summary>
        /// Идентификатор источника синхронизации
        /// </summary>
        public uint SSRC;

        /// <summary>
        /// Элементы описания источника синхронизации
        /// </summary>
        public RtcpSourceDescriptionItem[] Items;
    }
}

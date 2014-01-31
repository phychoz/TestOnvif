using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestOnvif
{
    /// <summary>
    /// Класс, обрабатывающий отчеты видеокамеры и посылающий видеокамере отчеты о качестве
    /// <see cref="http://book.itep.ru/4/44/rtc_4493.htm"/>
    /// </summary>
    public class RtcpReporter
    {
        private const string CNAME = "eeg9";
        /// <summary>
        /// Через этот объект осуществляется прием и посылка данных
        /// </summary>
        private UnicastUdpClient udpClient;

        /// <summary>
        /// Обрабатывает сырые UDP пакеты вызывает всякие свои события
        /// </summary>
        private RtcpPacketHandler rtcpHandler;

        /// <summary>
        /// Содержит адрес хоста, с которого идут отчетные пакеты
        /// </summary>
        private IPEndPoint remoteEndPoint;

        /// <summary>
        /// Это идентификатор источника синхронизации (нашей программы)
        /// Уникальный для каждого участиника
        /// </summary>
        private uint senderSSRC;

        /// <summary>
        /// Это идентификатор источника синхронизации (нашей программы)
        /// Уникальный для каждого участиника
        /// </summary>
        private uint recieverSSRC;

        /// <summary>
        /// Количество полученных RTP пакетов
        /// </summary>
        private uint receivedRtpPackets = 0;

        /// <summary>
        /// Временная метка последнего SR (LSR): 32 бита.
        /// Средние 32 бита из 64 битов временной метки NTP,
        /// полученные как часть самых последних пакетов отчетов отправителя RTCP (SR) из источника SSRC_n.
        /// Если SR еще не был получен, то временная метка LSR имеет нулевое значение.
        /// </summary>
        private uint lastSRTimestamp = 0;

        /// <summary>
        /// Задержка с момента последнего SR (DLSR): 32 бита. 
        /// Задержка в приемнике пакетов, выраженная в единицах, равных 1/65536 секунды,
        /// между получением последнего пакета SR из источника SSRC_n и посылкой этого блока отчета о приеме.
        /// Если пакет SR еще не был получен от SSRC_n, то поле DLSR имеет нулевое значение.
        /// </summary>
        private uint delaySinceLastSRTimestamp = 0;

        /// <summary>
        /// Джиттер прибытия: 32 бита. 
        /// Статистическая оценка разницы относительного времени прибытия информационных пакетов RTP,
        /// измеряемая в единицах временной метки и выражаемая целым числом без знака. 
        /// вычисление http://wiki.wireshark.org/RTP_statistics
        /// </summary>
        private double jitter = 0;

        private uint rtpTimestamp=0;
        private uint ntpTimestampLSW=0;
        private uint ntpTimestampMSW = 0;

        private DateTime senderReportTime;

        /// <summary>
        /// Начальная метка RTP
        /// </summary>
        private uint initTimestamp = 0;
        private DateTime startRtpSessionTime;

        private uint prevRtpTimestamp = 0;
        private DateTime prevRtpTime;

        private uint recieverReportCount=0;
        private uint senderReportCount = 0;

        /// <summary>
        /// Частота дескретизации кодека, видео 90кГц, аудио 8кГц
        /// необходима для перевода значений RTP timestamp в секунды при вычислении джиттера
        /// </summary>
        private int sampleFrequency;

        //private double msecSinceStart = 0;
        //private DateTime startRtpSessionTime = new DateTime();

        public void OnRtpTimeReporting(DateTime time)
        {
            if (RtpTimeReporting != null)
                RtpTimeReporting(time);
        }
        public event RtpTimeReportingHandler RtpTimeReporting;
        public delegate void RtpTimeReportingHandler(DateTime time);

        public void OnSessionTimeCorrecting(uint timestamp, DateTime time)
        {
            if (SessionTimeCorrecting != null)
                SessionTimeCorrecting(timestamp, time);
        }
        public event SessionTimeCorrectingHandler SessionTimeCorrecting;
        public delegate void SessionTimeCorrectingHandler(uint timestamp, DateTime time);


        public int Port { get; private set; }

        private object locker = new object();
        /// <summary>
        /// Создает новый объект, реализующий взаимодействие с помощью RTCP пакетов
        /// </summary>
        /// <param name="localRtcpPort">Порт, на который будут приходить RTCP пакеты на локальной машине</param>
        /// <param name="sfreq">Частота дескретизации кодека для видео 90000, для аудио 8000</param>
        public RtcpReporter(int localRtcpPort, int sampleRate)
        {
            udpClient = new UnicastUdpClient(localRtcpPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);

            rtcpHandler = new RtcpPacketHandler();

            udpClient.UdpPacketRecived += udpClient_ReceivedUdpPacket;

            rtcpHandler.CreatedRtcpSenderReport += rtcpHandler_CreatedRtcpSenderReport;
            rtcpHandler.CreatedRtcpSourceDescription += rtcpHandler_CreatedRtcpSourceDescription;

            Port = localRtcpPort;
            sampleFrequency = sampleRate;

            //udpSender = new UdpClient();
            //udpSender.Client.Bind(remoteEndPoint);

        }

        /// <summary>
        /// Обрабатывает RTP пакет с видео или звуком
        /// </summary>
        /// <param name="packet">RTP пакет с видео или звуком</param>
        public void HandleRtpPacket(RtpPacket packet)
        {
            lock (locker)
            {
                if (initTimestamp == 0)
                {
                    initTimestamp = packet.Timestamp;
                }

                if (senderSSRC == 0)
                {
                    senderSSRC = packet.SSRC;
                }

                if (receivedRtpPackets != 0)
                {
                    if (packet.Timestamp != prevRtpTimestamp)
                    {
                        DateTime time = DateTime.Now;
                        uint timestamp = packet.Timestamp;

                        DateTime rtpTime = startRtpSessionTime + TimeSpan.FromMilliseconds((double)(timestamp - initTimestamp) * 1000 / (double)sampleFrequency);
                        OnRtpTimeReporting(rtpTime);

                        double reciever = (time - prevRtpTime).TotalSeconds;
                        double sender = (double)(timestamp - prevRtpTimestamp) / (double)sampleFrequency;

                        jitter += (1.0 / 16.0) * (Math.Abs(reciever - sender) - jitter);

                        prevRtpTime = time;
                        prevRtpTimestamp = timestamp;
                    }
                }
                else
                {
                    prevRtpTime = DateTime.Now;
                    prevRtpTimestamp = packet.Timestamp;
                }
                receivedRtpPackets++;
            }
        }

        /// <summary>
        /// Обработчик события rtcpHandler.CreatedRtcpSourceDescription
        /// </summary>
        /// <param name="sources">Источники синхронизации</param>
        private void rtcpHandler_CreatedRtcpSourceDescription(RtcpSourceDescription[] sources)
        {
            //...
        }

        /// <summary>
        /// Обработчик события rtcpHandler.CreatedRtcpSenderReport
        /// </summary>
        /// <param name="packet">RTCP пакет отправителя данных (камеры)</param>
        private void rtcpHandler_CreatedRtcpSenderReport(RtcpSenderReportPacket packet)
        {
            lock (locker)
            {
                if (senderSSRC == 0)
                {
                    senderSSRC = packet.SenderSSRC;
                }
                if (senderReportCount == 0)
                {
                    uint timestamp = packet.RTPTimestamp;
                    uint msec = (uint)(Math.Round((double)packet.TimestampLSW / (double)uint.MaxValue, 3) * 1000);
                    DateTime time = new DateTime(1900, 1, 1).AddSeconds(packet.TimestampMSW).AddMilliseconds(msec);

                    TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time);
                    time = time+offset;

                    if (initTimestamp != 0)
                    {
                        double msecSinceStart = (double)(timestamp - initTimestamp) / sampleFrequency * 1000;
                        time-= TimeSpan.FromMilliseconds(msecSinceStart);
                        if (time != startRtpSessionTime)
                        {
                            startRtpSessionTime = time;
                            OnSessionTimeCorrecting(initTimestamp, startRtpSessionTime);
                            //Logger.Write(String.Format("StartSessionTime={0}", startRtpSessionTime.ToString("HH:mm:ss.fff")), EnumLoggerType.DebugLog);
                        }

                        
                    }
                }

                rtpTimestamp = packet.RTPTimestamp;
                ntpTimestampLSW = packet.TimestampLSW;
                ntpTimestampMSW = packet.TimestampMSW;

                senderReportTime = DateTime.Now;

                senderReportCount++;


            }
        }

        /// <summary>
        /// Обработчик события udpClient.ReceivedUdpPacket
        /// </summary>
        /// <param name="remoteEndPoint">Адрес хоста, с которого пришли данные</param>
        /// <param name="data">Указатель на пришедшие данные в неуправляемой памяти</param>
        /// <param name="count">Количество </param>
        private void udpClient_ReceivedUdpPacket(IPEndPoint remoteEndPoint, IntPtr data, int count)
        {
            //this.remoteEndPoint = remoteEndPoint;
            // обрабатывается  rtcp пакет от Sender-а (SR, SD)
            rtcpHandler.CreateRtcpPacket(data, count);

            // ответ Reciever-а (RR)
            // Не совсем правильно т.к. отчеты ресивера посылаются как ответ на отчета сендера,
            // В спецификации отправка ресивером RTCP пакетов должна быть не зависима от сендера и происходить максимально часто,
            // ограничение полоса пропускания RTCP не должна занимать более 5% трафика индивидуального отправителя 
            RtcpRecieverReportPacket packet = CreateRtcpRecieverReportPacket();
            byte[] rtcp = CreateRecieveReportBytes(packet);

            udpClient.Send(remoteEndPoint, rtcp);
        }

        private RtcpRecieverReportPacket CreateRtcpRecieverReportPacket()
        {
            lock (locker)
            {
                //DateTime recieverReportTime = DateTime.Now;
                if (senderReportCount != 0)
                {
                    //uint msec = (uint)(Math.Round((double)ntpTimestampLSW / (double)uint.MaxValue, 3) * 1000);
                    //DateTime time = new DateTime(1900, 1, 1).AddSeconds(ntpTimestampMSW).AddMilliseconds(msec);

                    //TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(time);
                    //senderReportTime = (time + offset);

                    delaySinceLastSRTimestamp = (uint)((DateTime.Now - senderReportTime).TotalSeconds * 65536);

                    lastSRTimestamp = (ntpTimestampMSW << 16) | (ntpTimestampLSW | 0xFFFF);
                }
                else
                {
                    delaySinceLastSRTimestamp = 0;
                    lastSRTimestamp = 0;
                }
            }
            return new RtcpRecieverReportPacket()
            {
                SenderSSRC = senderSSRC,
                RecieverSSRC = recieverSSRC,
                ReceivedRtpPackets = receivedRtpPackets,
                FractionLost=0,
                CumulativeNumberOfPacketsLost=0,
                Jitter=jitter,
                LastSRTimestamp = lastSRTimestamp,
                DelaySinceLastSRTimestamp=delaySinceLastSRTimestamp,

            };

        }


        /// <summary>
        /// Метод нужно допилить. Он работает, но гарантии, что он будет работать
        /// на других камерах, нет
        /// </summary>
        /// <param name="cname"></param>
        /// <returns></returns>
        private byte[] CreateRecieveReportBytes(RtcpRecieverReportPacket packet)
        {
            int packetSize = 
                32 + ((// receiver report
                1 + // source description: version + padding + source count
                1 + // source description: packet type
                2 + // source description: length
                4 + // source description: chunk1: ssrc
                1 + // source description: chunk1: type
                1 + // source description: chunk1: length
                CNAME.Length) / 4 + 1) * 4;

            byte[] data = new byte[packetSize];
            data[0] = 0x81;
            data[1] = 0xC9;
            data[2] = 0x00;
            data[3] = 0x07;
            BigEndian.WriteUInt32(data, 4, packet.SenderSSRC);

            BigEndian.WriteUInt32(data, 8, packet.RecieverSSRC);

            data[12] = packet.FractionLost;
            data[13] = 0;
            data[14] = 0;
            data[15] = 0;

            BigEndian.WriteUInt32(data, 16, packet.ReceivedRtpPackets);

            if (receivedRtpPackets > ushort.MaxValue)
            {
                // ...
            }

            BigEndian.WriteUInt32(data, 20, (uint)(packet.Jitter*1000)); 

            BigEndian.WriteUInt32(data, 24, packet.LastSRTimestamp);
            
            BigEndian.WriteUInt32(data, 28, packet.DelaySinceLastSRTimestamp); 

            data[32] = 0x81;
            data[33] = 0xCA;
            data[34] = 0x00;
            data[35] = (byte)((packetSize - 32) / 4 - 1);

            BigEndian.WriteUInt32(data, 36, packet.SenderSSRC);

            data[40] = 0x01;
            data[41] = (byte)CNAME.Length;

            Encoding.UTF8.GetBytes(CNAME, 0, CNAME.Length, data, 42);

            //Logger.Write(String.Format("recieverSSRC={0}, senderSSRC={1}, jitter={2}, LSR={3}, DSLR={4}",
            //    packet.RecieverSSRC, packet.SenderSSRC, packet.Jitter, packet.LastSRTimestamp, packet.DelaySinceLastSRTimestamp), EnumLoggerType.DebugLog);
      
            recieverReportCount++;

            return data;
        }

        /// <summary>
        /// Начинать слушать порт, на который приходят RTCP пакеты
        /// и слать в ответ свои
        /// </summary>
        public void StartReporting()
        {
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
            
            byte[] randomBytes = new byte[sizeof(UInt32)];
            random.GetNonZeroBytes(randomBytes);

            recieverSSRC = BitConverter.ToUInt32(randomBytes, 0);
            senderReportCount = 0;
            receivedRtpPackets = 0;
            recieverReportCount = 0;
            initTimestamp = 0;

            udpClient.StartReceiving();
      
        }

        /// <summary>
        /// Прекратить слушать порт, на который приходят RTCP пакеты
        /// и слать в ответ свои
        /// </summary>
        public void StopReporting()
        {
            udpClient.StopReceiving();
        }
    }
}

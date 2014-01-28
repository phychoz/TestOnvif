using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// Класс для прослушивания данных, поступающих
    /// из сети в режиме Unicast по протоколу UDP
    /// </summary>
    class UnicastUdpClient
    {
        /// <summary>
        /// Объект для создания прослушивателя multicast-рассылки
        /// </summary>
        private volatile Socket socket;

        /// <summary>
        /// Ожидаем ли мы получения данных
        /// </summary>
        private volatile bool receiving = false;

        private const ushort MaxUdpSize = 2048;//ushort.MaxValue;
        /// <summary>
        /// Буфер для получаемых данных
        /// </summary>
        private byte[] buffer = new byte[MaxUdpSize];

        private IPEndPoint ipep;

        private EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        public int Port { get; private set; }

        //public uint SSRC { get; set; }

        /// <summary>
        /// Создает прослушиватель UDP-пакетов, работающий в режиме Multicast
        /// <see cref="http://ru.wikipedia.org/wiki/Multicast"/>
        /// </summary>
        /// <param name="port">Порт, на который будут приходить данные</param>
        public UnicastUdpClient(int port)
        {
            //ipep = new IPEndPoint(IPAddress.Any, port);
            //socket.Bind(ipep);
            //socket.ReceiveBufferSize = int.MaxValue;
            Port = port;
        }

        /// <summary>
        /// Запускает поток для получения данных
        /// </summary>
        public void StartReceiving()
        {
            if (receiving == true) throw new Exception();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipep = new IPEndPoint(IPAddress.Any, Port);
            socket.Bind(ipep);
            socket.ReceiveBufferSize = int.MaxValue;
            receiving = true;
           socket.BeginReceiveFrom(buffer, 0, MaxUdpSize, SocketFlags.None, ref remoteEndPoint, Receive, socket);
       
        }

        /// <summary>
        /// Останавливает поток обработки данных
        /// </summary>
        public void StopReceiving()
        {
            if (socket != null)
            {
                lock (socket)
                {
                    receiving = false;
                }

                socket.Close();
            }
        }

        /// <summary>
        /// Метод будет асинхронно вызван при получении данных из сети
        /// </summary>
        private void Receive(IAsyncResult ar)
        {
            if (receiving == true)
            {
                int receivedBytesCount = socket.EndReceiveFrom(ar, ref remoteEndPoint);
                if (receivedBytesCount > 0)
                {
                    IntPtr data = Marshal.AllocHGlobal(receivedBytesCount);
                    Marshal.Copy(buffer, 0, data, receivedBytesCount);
                    UdpPacketRecived((IPEndPoint)remoteEndPoint, data, receivedBytesCount);
                    Marshal.FreeHGlobal(data);
                    //UdpPacketRecived((IPEndPoint)remoteEndPoint, buffer, receivedBytesCount);

                }
                if (receiving == true)
                {
                    lock (socket)
                    {
                        socket.BeginReceiveFrom(buffer, 0, MaxUdpSize,
                            SocketFlags.None, ref remoteEndPoint, Receive, socket);
                    }
                }
            }

        }

        public void Send(IPEndPoint remoteEndPoint, byte[] data)
        {
            if (receiving == true)
            {
                lock (socket)
                {
                    socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, remoteEndPoint, delegate { }, socket);
                }
            }
        }

        /// <summary>
        /// Событие вызывается при получении данных по UDP
        /// </summary>
        public event ReceiveUdpPacketHandler UdpPacketRecived;

        /// <summary>
        /// Тип обработчика события получения данных
        /// </summary>
        /// <param name="remoteEndPoint">Адрес, с которого пришли данные</param>
        /// <param name="data">Буфер, в котором содержатся полученные данные</param>
        /// <param name="count">Количество полученных байтов</param>
        //public delegate void ReceiveUdpPacketHandler(IPEndPoint remoteEndPoint, byte[] data, int count);
        public delegate void ReceiveUdpPacketHandler(IPEndPoint remoteEndPoint, IntPtr data, int count);
    }
}





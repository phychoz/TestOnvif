using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TestOnvif
{
    internal static class CameraDateTime
    {
        private static string timeRequest =
@"POST /onvif/device_service HTTP/1.1
Content-Type: application/soap+xml; charset=utf-8; action=""http://www.onvif.org/ver10/device/wsdl/GetSystemDateAndTime""
Host: 192.168.10.203
Content-Length: 282
Connection: Close

<s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header></s:Header><s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><GetSystemDateAndTime xmlns=""http://www.onvif.org/ver10/device/wsdl""/></s:Body></s:Envelope>";

        private static byte[] timeRequestData = Encoding.UTF8.GetBytes(timeRequest);

        private static byte[] _LocalDateTime = Encoding.UTF8.GetBytes("LocalDateTime>");
        private static byte[] _Year = Encoding.UTF8.GetBytes("Year>");
        private static byte[] _Month = Encoding.UTF8.GetBytes("Month>");
        private static byte[] _Day = Encoding.UTF8.GetBytes("Day>");
        private static byte[] _Hour = Encoding.UTF8.GetBytes("Hour>");
        private static byte[] _Minute = Encoding.UTF8.GetBytes("Minute>");
        private static byte[] _Second = Encoding.UTF8.GetBytes("Second>");

        private static byte[] buffer = new byte[16];

        /// <summary>
        /// Проверяет, совпадает ли весь меньший массив с концом большего массива
        /// </summary>
        /// <param name="first">Массив байтов</param>
        /// <param name="second">Массив байтов</param>
        /// <returns>true, если меньший массив полностью совпадает с концом большего и false в противном случае</returns>
        private static bool checkTails(byte[] first, byte[] second)
        {
            if (first.Length > second.Length)
            {
                byte[] tmp = first;
                first = second;
                second = tmp;
            }
            for (int i = 0; i < first.Length; i++)
            {
                if (second[second.Length - first.Length + i] != first[i])
                {
                    return false;
                }
            }
            return true;
        }

        static void enqueue(byte[] array, byte value)
        {
            for (int i = 1; i < array.Length; i++)
            {
                array[i - 1] = array[i];
            }
            array[array.Length - 1] = value;
        }

        /// <summary>
        /// Получает от камеры время.
        /// На компьютере Intel Core i5 8GB mem метод стабильно возвращается в течение 38 миллисекунд
        /// </summary>
        /// <param name="address">IP-адрес камеры</param>
        /// <returns></returns>
        public static DateTime Get(IPAddress address)
        {
            IPEndPoint ipe = new IPEndPoint(address, 80);
            TcpClient client = new TcpClient();
            client.Connect(ipe);
            NetworkStream ns = client.GetStream();

            ns.Write(timeRequestData, 0, timeRequestData.Length);

            // вошли ли мы внутрь тега LocalDateTime
            bool _ldt = false;
            //
            int _y = -1;
            //
            int _mon = -1;
            //
            int _d = -1;
            //
            int _h = -1;
            //
            int _min = -1;
            //
            int _s = -1;

            int y = -1, mon = -1, d = -1, h = -1, min = -1, s = -1;


            int _b = ns.ReadByte();
            // пока есть что читать из сети
            while (_b >= 0)
            {
                // сместить данные в буфере и добавить новый байт
                enqueue(buffer, (byte)_b);

                if (_ldt)
                {
                    if (_y >= 0)
                    {
                        _y--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            y = int.Parse(Encoding.UTF8.GetString(buffer, _y, buffer.Length - _y - 1));
                            _y = -1;
                        }
                    }
                    if (checkTails(buffer, _Year) && y == -1)
                    {
                        _y = buffer.Length;
                    }
                    if (_mon >= 0)
                    {
                        _mon--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            mon = int.Parse(Encoding.UTF8.GetString(buffer, _mon, buffer.Length - _mon - 1));
                            _mon = -1;
                        }
                    }
                    if (checkTails(buffer, _Month) && mon == -1)
                    {
                        _mon = buffer.Length;
                    }
                    if (_d >= 0)
                    {
                        _d--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            d = int.Parse(Encoding.UTF8.GetString(buffer, _d, buffer.Length - _d - 1));
                            _d = -1;
                        }
                    }
                    if (checkTails(buffer, _Day) && d == -1)
                    {
                        _d = buffer.Length;
                    }
                    if (_h >= 0)
                    {
                        _h--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            string str = Encoding.UTF8.GetString(buffer, _h, buffer.Length - _h - 1);
                            h = int.Parse(str);
                            _h = -1;
                        }
                    }
                    if (checkTails(buffer, _Hour) && h == -1)
                    {
                        _h = buffer.Length;
                    }
                    if (_min >= 0)
                    {
                        _min--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            min = int.Parse(Encoding.UTF8.GetString(buffer, _min, buffer.Length - _min - 1));
                            _min = -1;
                        }
                    }
                    if (checkTails(buffer, _Minute) && min == -1)
                    {
                        _min = buffer.Length;
                    }
                    if (_s >= 0)
                    {
                        _s--;
                        if (_b == 0x3C) // _b == "<"
                        {
                            s = int.Parse(Encoding.UTF8.GetString(buffer, _s, buffer.Length - _s - 1));
                            _s = -1;
                        }
                    }
                    if (checkTails(buffer, _Second) && s == -1)
                    {
                        _s = buffer.Length;
                    }
                }
                else
                {
                    if (checkTails(buffer, _LocalDateTime))
                    {
                        _ldt = true;
                    }
                }

                _b = ns.ReadByte();
            }

            if (y < 0 || min < 0 || d < 0 || h < 0 || min < 0 || s < 0)
            {
                throw new Exception();
            }
            return new DateTime(y, mon, d, h, min, s, 0);
        }
    }
}


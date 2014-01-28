using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif
{
        public class G711Decoder
        {
            private short[] decodeMap = {   -32124, -31100, -30076, -29052, -28028, -27004, -25980, -24956,
                                            -23932, -22908, -21884, -20860, -19836, -18812, -17788, -16764, 
                                            -15996, -15484, -14972, -14460, -13948, -13436, -12924, -12412, 
                                            -11900, -11388, -10876, -10364, -9852, -9340, -8828, -8316, 
                                            -7932, -7676, -7420, -7164, -6908, -6652, -6396, -6140,
                                            -5884, -5628, -5372, -5116, -4860, -4604, -4348, -4092, 
                                            -3900, -3772, -3644, -3516, -3388, -3260, -3132, -3004, 
                                            -2876, -2748, -2620, -2492, -2364, -2236, -2108, -1980,
                                            -1884, -1820, -1756, -1692, -1628, -1564, -1500, -1436, 
                                            -1372, -1308, -1244, -1180, -1116, -1052, -988, -924, 
                                            -876, -844, -812, -780, -748, -716, -684, -652,
                                            -620,-588, -556, -524, -492, -460, -428, -396,
                                            -372,-356, -340, -324, -308, -292, -276, -260,
                                            -244,-228, -212, -196, -180, -164, -148, -132,
                                            -120,-112, -104, -96, -88, -80, -72, -64,
                                            -56, -48,-40, -32, -24, -16, -8, 0, 
                                            32124, 31100, 30076, 29052, 28028, 27004, 25980, 24956, 
                                            23932, 22908, 21884, 20860, 19836, 18812, 17788, 16764, 
                                            15996, 15484, 14972, 14460, 13948, 13436, 12924, 12412, 
                                            11900, 11388, 10876, 10364, 9852, 9340, 8828, 8316, 
                                            7932, 7676, 7420, 7164, 6908, 6652, 6396, 6140, 
                                            5884, 5628, 5372, 5116, 4860, 4604, 4348, 4092, 
                                            3900, 3772, 3644, 3516, 3388, 3260, 3132, 3004, 
                                            2876, 2748, 2620, 2492, 2364, 2236, 2108, 1980, 
                                            1884, 1820, 1756, 1692, 1628, 1564, 1500, 1436, 
                                            1372, 1308, 1244, 1180, 1116, 1052, 988, 924,
                                            876, 844, 812, 780, 748, 716, 684, 652,
                                            620, 588, 556, 524, 492, 460, 428, 396,
                                            372, 356,340, 324, 308, 292, 276, 260,
                                            244, 228, 212,196, 180, 164, 148, 132,
                                            120, 112, 104, 96,88, 80, 72, 64,
                                            56, 48, 40, 32, 24, 16, 8, 0, };


            public  byte[] Decode(byte[] data)
            {
                int size = data.Length;
                byte[] decoded = new byte[size * 2];
                for (int i = 0; i < size; i++)
                {
                    decoded[2 * i] = (byte)(decodeMap[data[i]] & 0xff);
                    decoded[2 * i + 1] = (byte)(decodeMap[data[i]] >> 8);
                }
                return decoded;
            }
        }
    }

#region old
//public MuLawDecoder()
//{
//    muLawToPcmMap = new short[256];
//    for (byte i = 0; i < byte.MaxValue; i++)
//        muLawToPcmMap[i] = decode(i);
//    //WriteTable();


//}

//void WriteTable()
//{
//    System.IO.FileStream stream = new System.IO.FileStream(@"d:\mulan_table.txt", System.IO.FileMode.Create);
//    System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
//    //byte[] buffer = new byte[muLawToPcmMap.Length];             

//    //List<byte[]> buffer = new List<byte[]>();

//    for (int i = 0; i < muLawToPcmMap.Length; i++)
//    {
//        writer.Write(muLawToPcmMap[i]+",");
//        //byte[] value = BitConverter.GetBytes(muLawToPcmMap[i]);
//        //buffer.Add(value);
//    }
//    writer.Close();
//}

//private  short decode(byte mulaw)
//{
//    mulaw = (byte)~mulaw;
//    int sign = mulaw & 0x80;
//    int exponent = (mulaw & 0x70) >> 4;
//    int data = mulaw & 0x0f;
//    data |= 0x10;
//    data <<= 1;
//    data += 1;
//    data <<= exponent + 2;
//    data -= 0x84;
//    return (short)(sign == 0 ? data : -data);
//}

//public  short MuLawDecode(byte mulaw)
//{
//    return muLawToPcmMap[mulaw];
//}

//public  short[] MuLawDecode(byte[] data)
//{
//    int size = data.Length;
//    short[] decoded = new short[size];
//    for (int i = 0; i < size; i++)
//        decoded[i] = muLawToPcmMap[data[i]];
//    return decoded;
//}

//public  void MuLawDecode(byte[] data, out short[] decoded)
//{
//    int size = data.Length;
//    decoded = new short[size];
//    for (int i = 0; i < size; i++)
//        decoded[i] = muLawToPcmMap[data[i]];
//}
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace TestOnvif
{
    /// <summary>
    /// Класс для вывода звуковых данных в формате PCMU в микшер
    /// </summary>
    public class PCMUPlayer
    {
        private IntPtr ptrWaveOut;

        public PCMUPlayer()
        {               
            WinMM.WAVEFORMATEX waveFormat = new WinMM.WAVEFORMATEX()
            {
                FormatTag = WinMM.WaveFormat.WAVE_FORMAT_PCM,       //WAVE_FORMAT_MULAW  
                Channels = 1,                                      // один канал
                SamplesPerSec = 8000,                              // частота дискретизации 8 kHz
                AverageBytesPerSecond = 8000,                      // битрейт 8000 байт / сек.
                BlockAlign = 2,//1                                 // (nChannels × wBitsPerSample) / 8 
                BitsPerSample = 16,//8                             // количество бит на один отсчет
                Size = 0                                          // размер расширения данной структуры
            };

            WinMM.waveOutOpen(ref ptrWaveOut, WinMM.WAVE_MAPPER, ref waveFormat, IntPtr.Zero, IntPtr.Zero, WinMM.CALLBACK_NULL);
        }

        public void PlayFromRtpPacket(RtpPacket packet)
        {
            PlayFromMemory(packet.Payload, packet.PayloadLength);
        }

        public void PlayFromMemory(IntPtr ptr, int count)
        {
            WinMM.WAVEHDR waveHeader = new WinMM.WAVEHDR()
            {
                Data = ptr,
                BufferLength = (uint)count,
                BytesRecorded = 0,
                User = IntPtr.Zero,
                Flags = (WinMM.WaveHdrFlags)0,
                Loops = 0,
                Next = IntPtr.Zero,
                Reserved = IntPtr.Zero
            };

            IntPtr ptrWaveHeader = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WinMM.WAVEHDR)));

            Marshal.StructureToPtr(waveHeader, ptrWaveHeader, true);

            WinMM.waveOutPrepareHeader(ptrWaveOut, ptrWaveHeader, Marshal.SizeOf(typeof(WinMM.WAVEHDR)));

            WinMM.waveOutWrite(ptrWaveOut, ptrWaveHeader, Marshal.SizeOf(typeof(WinMM.WAVEHDR)));

            Marshal.DestroyStructure(ptrWaveHeader, typeof(WinMM.WAVEHDR));
        }

        public void Dispose()
        {
            WinMM.waveOutClose(ptrWaveOut);
        }
    }
}

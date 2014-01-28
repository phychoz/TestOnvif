using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    public class WinMM
    {
        public const uint WAVE_MAPPER = unchecked((uint)(-1));
        public const uint CALLBACK_NULL = 0x00000000;

        [Flags]
        public enum WaveHdrFlags : uint
        {
            WHDR_DONE = 1,
            WHDR_PREPARED = 2,
            WHDR_BEGINLOOP = 4,
            WHDR_ENDLOOP = 8,
            WHDR_INQUEUE = 16
        }
        public enum WaveFormat : ushort
        {
            WAVE_FORMAT_PCM = 0x0001,
            WAVE_FORMAT_MULAW = 0x0007
        }
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/dd390970%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEFORMATEX
        {
            public WaveFormat FormatTag;
            public ushort Channels;
            public uint SamplesPerSec;
            public uint AverageBytesPerSecond;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public ushort Size;
        }
        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/windows/desktop/dd743837%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEHDR
        {
            public IntPtr Data;
            public uint BufferLength;
            public uint BytesRecorded;
            public IntPtr User;
            public WaveHdrFlags Flags;
            public uint Loops;
            public IntPtr Next;
            public IntPtr Reserved;
        }
        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/windows/desktop/dd743866%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="phwo"></param>
        /// <param name="uDeviceID"></param>
        /// <param name="pwfx"></param>
        /// <param name="dwCallback"></param>
        /// <param name="dwCallbackInstance"></param>
        /// <param name="fdwOpen"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutOpen(ref IntPtr phwo, uint uDeviceID, ref WAVEFORMATEX pwfx,
            IntPtr dwCallback, IntPtr dwCallbackInstance, uint fdwOpen);
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/aa908929.aspx
        /// </summary>
        /// <param name="hwo"></param>
        /// <param name="pwh"></param>
        /// <param name="cbwh"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutPrepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/windows/desktop/dd743875%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="hwo"></param>
        /// <param name="pwh"></param>
        /// <param name="cbwh"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutUnprepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/windows/desktop/dd743876%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="hwo"></param>
        /// <param name="pwh"></param>
        /// <param name="cbwh"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutWrite(IntPtr hwo, IntPtr pwh, int cbwh);
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/aa908380.aspx
        /// </summary>
        /// <param name="hwo"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutReset(IntPtr hwo);
        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/windows/desktop/dd743856%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="hwo"></param>
        /// <returns></returns>
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutClose(IntPtr hwo);

    }
}

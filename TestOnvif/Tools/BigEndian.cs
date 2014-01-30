using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestOnvif
{
    /// <summary>
    /// Класс для чтения чисел в формате BigEndian
    /// </summary>
    unsafe static class BigEndian
    {
        private static ushort Reverse(ushort n)
        {
            return (ushort)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        private static short Reverse(short n)
        {
            return (short)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        private static uint Reverse(uint n)
        {
            return (uint)(((Reverse((ushort)n) & 0xffff) << 0x10) | (Reverse((ushort)(n >> 0x10)) & 0xffff));
        }

        private static int Reverse(int n)
        {
            return (int)(((Reverse((short)n) & 0xffff) << 0x10) | (Reverse((short)(n >> 0x10)) & 0xffff));
        }

        private static ulong Reverse(ulong n)
        {
            return (ulong)(((Reverse((uint)n) & 0xffffffffL) << 0x20) | (Reverse((uint)(n >> 0x20)) & 0xffffffffL));
        }

        private static long Reverse(long n)
        {
            return (long)(((Reverse((int)n) & 0xffffffffL) << 0x20) | (Reverse((int)(n >> 0x20)) & 0xffffffffL));
        }

        public static ushort ReadUInt16(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt16(data, offset));
        }

        public static ushort ReadUInt16(IntPtr data, int offset)
        {

            return Reverse((ushort)Marshal.ReadInt32(data, offset));//BitConverter.ToInt16(data, offset));
        }

        public static short ReadInt16(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToInt16(data, offset));
        }

        public static short ReadInt16(IntPtr data, int offset)
        {

            return Reverse(Marshal.ReadInt16(data, offset));//BitConverter.ToInt16(data, offset));
        }

        unsafe public static ushort ReadUInt16(void* data)
        {
            return Reverse(*(ushort*)data);
        }

        unsafe public static short ReadInt16(void* data)
        {
            return Reverse(*(short*)data);
        }

        public static uint ReadUInt24(byte[] data, int offset)
        {
            uint result = 0;
            result = data[offset];
            result <<= 8;
            result |= data[offset + 1];
            result <<= 8;
            result |= data[offset + 2];
            return result;
        }

        public static int ReadInt24(byte[] data, int offset)
        {
            int result = 0;
            result = data[offset];
            result <<= 8;
            result |= data[offset + 1];
            result <<= 8;
            result |= data[offset + 2];
            return result;
        }

        unsafe public static uint ReadUInt24(byte* data)
        {
            uint result = 0;
            result = data[0];
            result <<= 8;
            result |= data[1];
            result <<= 8;
            result |= data[2];
            return result;
        }

        unsafe public static int ReadInt24(byte* data)
        {
            int result = 0;
            result = data[0];
            result <<= 8;
            result |= data[1];
            result <<= 8;
            result |= data[2];
            return result;
        }

        public static uint ReadUInt32(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt32(data, offset));
        }
        public static uint ReadUInt32(IntPtr data, int offset)
        {
            return Reverse((uint)Marshal.ReadInt64(data, offset));//BitConverter.ToInt16(data, offset));
        }

        public static int ReadInt32(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToInt32(data, offset));
        }

        public static int ReadInt32(IntPtr data, int offset)
        {
            return Reverse(Marshal.ReadInt32(data, offset));//BitConverter.ToInt16(data, offset));
        }

        unsafe public static uint ReadUInt32(void* data)
        {
            return Reverse(*(uint*)data);
        }

        unsafe public static int ReadInt32(void* data)
        {
            return Reverse(*(int*)data);
        }

        public static ulong ReadUInt64(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt64(data, offset));
        }

        public static long ReadInt64(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToInt64(data, offset));
        }

        unsafe public static ulong ReadUInt64(void* data)
        {
            return Reverse(*(ulong*)data);
        }

        unsafe public static long ReadInt64(void* data)
        {
            return Reverse(*(long*)data);
        }

        public static void WriteUInt16(byte[] data, int offset, ushort n)
        {
            data[offset] = (byte)(n >> 8);
            data[offset + 1] = (byte)(n & 0xff);
        }

        unsafe public static void WriteUInt16(void* data, ushort n)
        {
            ((byte*)data)[0] = (byte)(n >> 8);
            ((byte*)data)[1] = (byte)(n & 0xff);
        }

        public static void WriteUInt32(byte[] data, int offset, uint n)
        {
            data[offset] = (byte)(n >> 24);
            data[offset + 1] = (byte)((n >> 16) & 0xFF);
            data[offset + 2] = (byte)((n >> 8) & 0xFF);
            data[offset + 3] = (byte)(n & 0xFF);
        }
    }
}

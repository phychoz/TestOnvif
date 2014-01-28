using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOnvif.StreamMedia.Audio
{

    public class WaveHeader
    {
        RIFFChunk riff = new RIFFChunk();
        FormatChunk format = new FormatChunk();
        DataChunk data = new DataChunk();

        public RIFFChunk Riff
        {
            get { return riff; }
            set { riff = value; }
        }

        public FormatChunk Format
        {
            get { return format; }
            set { format = value; }
        }

        public DataChunk Data
        {
            get { return data; }
            set { data = value; }
        }
    }


    public class RIFFChunk
    {
        public string ID { get; set; }
        public uint Length { get; set; }
        public string Type { get; set; }
        public RIFFChunk()
        {
            Length = 0;
            ID = "RIFF";
            Type = "WAVE";
        }
    }
    public class FormatChunk
    {
        public string ID { get; set; }
        public uint Size { get; set; }
        public ushort Tag { get; set; }
        public ushort Channels { get; set; }
        public uint Samples { get; set; }
        public uint Average { get; set; }
        public ushort Align { get; set; }
        public ushort Bits { get; set; }

        public FormatChunk()
        {
            ID = "fmt ";
            Size = 16;
            Tag = 1;
            Channels = 1;
            Samples = 8000;
            Bits = 16;
            Align = (ushort)(Channels * (Bits / 8));
            Average = Samples * Align;
        }
    }

    public class DataChunk
    {
        public string ID { get; set; }
        public uint Size { get; set; }

        public DataChunk()
        {
            Size = 0;
            ID = "data";
        }
    }

}

#pragma once

using namespace System;


namespace FFmpegWrapper 
{
	public delegate void VideoFrameReceivedHandler(IntPtr, int, int,int, unsigned long, unsigned long, int);//, int, int);
	public delegate void AudioFrameReceivedHandler(IntPtr, int, unsigned long);

	public delegate void LogDataReceivedHandler(String^ );

	public enum class CodecType
	{
		JPEG,
		THEORA,
		VP8,
		MPEG4,
		H264
	};

	public ref class CodecParams {
	public:
		CodecParams(CodecType codec, int wdth, int hght)
		{
			id=codec;
			width=wdth;
			height=hght;
		}

		CodecParams(){}
		~CodecParams(){}

		property CodecType ID
		{
			CodecType get()
			{
				return id;
			}
			void set(CodecType value)
			{
				id = value;
			}
		}

		property int Width
		{
			int get()
			{
				return width;
			}
			void set(int value)
			{
				width = value;
			}
		}

		property int Height
		{
			int get()
			{
				return height;
			}
			void set(int value)
			{
				height = value;
			}
		}

		CodecParams% operator = (CodecParams% v)
		{
			ID=v.ID;
			Width = v.Width;
			Height=v.Height;

			return (*this);   
		}

	private:
		CodecType id;
		int width;
		int height;
	};

	ref class SessionValueCounter 
	{
	public:
		property unsigned long ReceivedFrameCount
		{
			unsigned long get()
			{
				return receivedFrameCount;
			}
			void set(unsigned long value)
			{
				receivedFrameCount = value;
			}
		}

		property unsigned long CurrTimestamp
		{
			unsigned long get()
			{
				return currTimestamp;
			}
			void set(unsigned long value)
			{
				currTimestamp = value;
			}
		}

		property unsigned long LastTimestamp
		{
			unsigned long get()
			{
				return lastTimestamp;
			}
			void set(unsigned long value)
			{
				lastTimestamp = value;
			}
		}

		property unsigned long TimeInterval
		{
			unsigned long get()
			{
				return timeInterval;
			}
			void set(unsigned long value)
			{
				timeInterval = value;
			}
		}

		SessionValueCounter()
		{
			int ReceivedFrameCount=0;	
			int currTimestamp=0;
			int lastTimestamp=0;
			int timeInterval=0;
		}

	private :
		unsigned long receivedFrameCount;	
		unsigned long currTimestamp;
		unsigned long lastTimestamp;
		unsigned long timeInterval;
	};
	
	public ref class MediaData {
	public:
		
		int Size;
		int FrameNumber;
		long Dts;
		int Flags;
		//array<int>^ GetBitmap();
	private:
		array<int>^ bitmap;
		//void SetBitmap(IntPtr^, int);
	};
}
#pragma once

#include "FFmpegUtils.h"

namespace FFmpegWrapper
{
	//public delegate void VideoFrameReceivedHandler(IntPtr, int, int,int, long, long, int);// int, int);
	//public delegate void AudioFrameReceivedHandler(IntPtr, int);

	public ref class FFmpegPlayer
	{
	public:
		event VideoFrameReceivedHandler^ VideoFrameReceived;
		event AudioFrameReceivedHandler^ AudioFrameReceived;

		void Open(String^ file);

		void ProcessFile();
		//int ShowVideo();
		void GetAVFrame(long position);

		long GetVideoDuration();

		void Close();

		property String^ InputFile;

	private:

		int  video_stream_index;
		int  audio_stream_index;

		AVFrame* video_frame;
		AVFrame* audio_frame;
		AVFrame* rgb_frame;

		uint8_t * buffer;

		AVFormatContext* format_context;

		AVCodecContext* video_codec_context;
		AVCodec* video_codec;

		AVCodecContext* audio_codec_context;
		AVCodec* audio_codec;

		struct SwsContext* img_convert_context;

		void DecodeVideoFrame(IntPtr data,  int size, int type);
	};
}
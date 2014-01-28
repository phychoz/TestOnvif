#pragma once

//#define UNMANAGED 
//#define USE_SDL

#include "FFmpegUtils.h"

#ifdef UNMANAGED


#include "ffmpeg_native_writer.h"
#include "ffmpeg_native_reader.h"
#endif

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFmpegWrapper 
{
	//public delegate void VideoFrameReceivedHandler(Bitmap^);
	//delegate void LoggerDelegate(void *, int level, const char * format, va_list varg);

	public ref class FFmpegMedia 
	{
	public:
		property String^ OutputFilename;

		property CodecParams^ VideoEncoderParams;
		property CodecParams^ VideoDecoderParams;

		event VideoFrameReceivedHandler^ VideoFrameReceived;
		event AudioFrameReceivedHandler^ AudioFrameReceived;

		static event LogDataReceivedHandler^ LogDataReceived;

		void Open();
		void Close();

		void VideoDataProcessing(IntPtr data, int size,  bool key, unsigned long pts);
		void AudioDataProcessing(IntPtr data, int size, bool key, unsigned long pts);

		void WriteAudioDataToFile(IntPtr data, int size, unsigned long time, int duration);
		void WriteVideoDataToFile(IntPtr data, int size, unsigned long time, int duration);

		static void OnLogDataRecived(String ^ string);

	private:

#ifdef UNMANAGED
		ffmpeg_native_writer *writer;
		ffmpeg_native_reader *reader;

		VIDEO_FRAME_DECODED video_callback;
		AUDIO_FRAME_DECODED audio_callback;

		VideoFrameReceivedHandler^ videoReceivedHandler;
		AudioFrameReceivedHandler^ audioReceivedHandler;

		GCHandle videoGCHandle;
		GCHandle audioGCHandle;

		void VideoCalback(IntPtr data, int size, int width ,int height, unsigned long count, unsigned long time, int flag);
		void AudioCalback(IntPtr data,  int size, unsigned long time);


#endif

		SessionValueCounter^ videoCounter;
		SessionValueCounter^ audioCounter;

		AVFrame *dec_audio_frame;
		AVFrame *dec_video_frame;
		AVFrame *dec_rgb_frame;

		AVFrame *enc_video_frame;
		AVFrame *enc_audio_frame;

		uint8_t *dec_rgb_buffer;
		uint8_t *enc_video_buffer;
		uint8_t *enc_audio_buffer;

		//AVDictionary *output_dictionary;

		//AVFormatContext* in_format_context;
		AVFormatContext* out_format_context;

		AVCodec* in_video_codec;
		AVCodec* in_audio_codec;

		AVCodecContext* in_video_codec_context;	
		AVCodecContext* in_audio_codec_context;	

		AVCodec* out_video_codec;
		AVCodec* out_audio_codec;

		AVCodecContext* out_video_codec_context;	
		AVCodecContext* out_audio_codec_context;	

		AVStream* out_audio_stream;
		AVStream* out_video_stream;

		struct SwsContext* rgb_convert_context;
		struct SwsContext* yuv_convert_context;
		struct SwsContext* img_convert_context;

		struct SwrContext* swr_ctx;

		//SDL_Surface* screen;
		//SDL_Overlay* bmp;

		Object^ locker;

		//typedef void (__cdecl *LOGGERCALLBACK)(void *, int level, const char * format, va_list varg);
		//LOGGERCALLBACK logHandler;
		//void logger(void *, int level, const char * format, va_list varg);
		//LoggerDelegate^ logDel ;
		//GCHandle logGCHandle;


		void InputInit();
		void InputVideoCodecInit();
		void InputAudioCodecInit();
		void InputFramesAlloc();

		void OutputInit();
		void OutputFileInit();
		void OutputVideoCodecInit();
		void OutputAudioCodecInit();
		void OutputFramesAlloc();

		void OutputOpen();

		void VideoDataReset();
		void AudioDataReset();

		void OutputClose();
		void InputClose();

		int select_sample_rate(AVCodec *codec);
		int check_sample_fmt(AVCodec *codec, enum AVSampleFormat sample_fmt);
		int select_channel_layout(AVCodec *codec);
	};

}
#pragma unmanaged

//#pragma comment (lib, "C:\\ffmpeg\\lib\\avcodec.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\swscale.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avformat.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avutil.lib")

#pragma comment (lib, "C:\\FFmpeg2\\lib\\avcodec.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\swscale.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\avformat.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\avutil.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\swresample.lib")

#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDL.lib")
#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDLmain.lib")

extern "C" 
{
#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/mathematics.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>
#include <libswresample/swresample.h>

//#include "libavformat\\avformat.h" 
//#include "libavcodec\\avcodec.h"
//#include "libswscale\\swscale.h"
//#include "libavutil\\mathematics.h"
}

#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string>
//#include <SDL.h>
//#undef main

#pragma managed
#include "FFmpegMedia.h"

#using <mscorlib.dll>
#using <System.Drawing.dll>

using namespace System;
using namespace System::Runtime::InteropServices;


namespace FFmpegWrapper 
{

#pragma region LOGGER

	void logger(void *, int level, const char * format, va_list varg)
	{
		if(level<=AV_LOG_ERROR/*AV_LOG_WARNING*/)
		{
			int len=_vscprintf( format, varg )+1;
			char * buffer = (char *)malloc( len * sizeof(char) );

			vsprintf_s(buffer,len, format, varg);

			System::String ^ result=gcnew System::String(buffer);

			puts( buffer );
			free( buffer );

			FFmpegMedia::OnLogDataRecived(result);
		}
	}	

	void FFmpegMedia::OnLogDataRecived(String ^ result)
	{
		LogDataReceived(result);
	}

#pragma endregion

#pragma region FFMPEG_START

	void FFmpegMedia::Open()
	{
		//logDel= gcnew LoggerDelegate(this, &FFmpegMedia::logger);
		//IntPtr ptr = Marshal::GetFunctionPointerForDelegate(logDel);
		//logGCHandle=GCHandle::Alloc(logDel);
		//logHandler = static_cast<LOGGERCALLBACK>(ptr.ToPointer());
		//av_log_set_callback(logHandler);

		av_log_set_callback(logger);

		av_register_all();

		InputInit();

		//OutputInit();
	}

#pragma endregion

#pragma region INPUT_INIT

	void  FFmpegMedia::InputInit()
	{
#ifdef UNMANAGED
		reader=new ffmpeg_native_reader();
		reader->open("H264",VideoDecoderParams->Width, VideoDecoderParams->Height);

		videoReceivedHandler = gcnew VideoFrameReceivedHandler(this,&FFmpegMedia::VideoCalback );
		audioReceivedHandler = gcnew AudioFrameReceivedHandler(this,&FFmpegMedia::AudioCalback );

		videoGCHandle = GCHandle::Alloc(videoReceivedHandler);
		audioGCHandle = GCHandle::Alloc(audioReceivedHandler);

		IntPtr videoPtr = Marshal::GetFunctionPointerForDelegate(videoReceivedHandler);
		IntPtr audioPtr = Marshal::GetFunctionPointerForDelegate(audioReceivedHandler);

		 reader->register_video_handler(static_cast<VIDEO_FRAME_DECODED>(videoPtr.ToPointer()) );
		 reader->register_audio_handler(static_cast<AUDIO_FRAME_DECODED>(audioPtr.ToPointer()) );


#else
		videoCounter=gcnew SessionValueCounter();
		audioCounter=gcnew SessionValueCounter();

		InputVideoCodecInit();
		InputAudioCodecInit();
#endif
	}

	void  FFmpegMedia::InputVideoCodecInit()
	{
		switch (VideoDecoderParams->ID)
		{
		case CodecType::JPEG:
			in_video_codec = avcodec_find_decoder(AV_CODEC_ID_MJPEG);
			break;

		case CodecType::H264:
			in_video_codec = avcodec_find_decoder(AV_CODEC_ID_H264);
			break;

		case CodecType::MPEG4:
			in_video_codec = avcodec_find_decoder(AV_CODEC_ID_MPEG4);
			break;
		}

		in_video_codec_context = avcodec_alloc_context3(in_video_codec);
		in_video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;

		in_video_codec_context->pix_fmt = PIX_FMT_YUV420P;
		in_video_codec_context->width=VideoDecoderParams->Width;
		in_video_codec_context->height=VideoDecoderParams->Height;

		if (avcodec_open2(in_video_codec_context, in_video_codec, NULL) < 0)
		{
			throw gcnew Exception(L"ffmpeg: Unable to open video codec");
		}

		InputFramesAlloc();
#ifdef USE_SDL
		int err = SDL_Init(SDL_INIT_VIDEO);
		if (err < 0) 
		{
			throw gcnew Exception(L"Unable to init SDL: ");
		}

		screen = SDL_SetVideoMode(in_video_codec_context->width, in_video_codec_context->height, 0, 0);
		if (screen == NULL) 
		{
			throw gcnew Exception(L"Couldn't set video mode");
		}

		bmp = SDL_CreateYUVOverlay(in_video_codec_context->width, in_video_codec_context->height,
			SDL_YV12_OVERLAY, screen);

		img_convert_context = sws_getCachedContext(NULL,
			in_video_codec_context->width, in_video_codec_context->height,
			in_video_codec_context->pix_fmt,
			in_video_codec_context->width, in_video_codec_context->height,
			PIX_FMT_YUV420P, SWS_BICUBIC,
			NULL, NULL, NULL);

		if (img_convert_context == NULL) 
		{
			throw gcnew Exception(L"Cannot initialize the conversion context");
		}
#endif
	}


	void  FFmpegMedia::InputFramesAlloc()
	{
		dec_audio_frame=avcodec_alloc_frame();
		dec_video_frame = avcodec_alloc_frame();

		dec_rgb_frame = avcodec_alloc_frame();

		int buffer_size = avpicture_get_size(PIX_FMT_RGB24, in_video_codec_context->width, in_video_codec_context->height);
		dec_rgb_buffer=(uint8_t *)av_malloc(buffer_size*sizeof(uint8_t));
		avpicture_fill((AVPicture *)dec_rgb_frame, dec_rgb_buffer, PIX_FMT_RGB24, in_video_codec_context->width, in_video_codec_context->height);

		rgb_convert_context = sws_getCachedContext(NULL,
			in_video_codec_context->width, in_video_codec_context->height,
			in_video_codec_context->pix_fmt,
			in_video_codec_context->width, in_video_codec_context->height,
			PIX_FMT_BGR24, SWS_BICUBIC , /*SWS_FAST_BILINEAR,*/
			//PIX_FMT_YUV420P, SWS_BICUBIC,
			NULL, NULL, NULL);

		if (rgb_convert_context == NULL) {
			throw gcnew Exception(L"Cannot initialize the conversion context");
		}
	}

	void  FFmpegMedia::InputAudioCodecInit()
	{
		in_audio_codec = avcodec_find_decoder(AV_CODEC_ID_PCM_MULAW);

		in_audio_codec_context = avcodec_alloc_context3(in_audio_codec);
		in_audio_codec_context->codec_type = AVMEDIA_TYPE_AUDIO;

		in_audio_codec_context->sample_fmt = *in_audio_codec->sample_fmts;   
		//in_audio_codec_context->bit_rate = 64000;
		in_audio_codec_context->sample_rate = 8000;
		in_audio_codec_context->channels    = 1;

		if (avcodec_open2(in_audio_codec_context, in_audio_codec, NULL) < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to open video codec");
	}

#pragma endregion

#pragma region OUTPUT_INIT

	void FFmpegMedia::OutputInit() 
	{		
		locker=gcnew Object();
#ifdef UNMANAGED
		writer=new ffmpeg_native_writer();

		char* filename= (char*) ((Marshal::StringToHGlobalAnsi (OutputFilename)).ToPointer ());

		writer->open(filename, "MPEG4", VideoEncoderParams->Width, VideoEncoderParams->Height );
#else
		OutputFileInit();

		OutputVideoCodecInit();
		OutputAudioCodecInit();

		OutputOpen();
#endif 
	}

	void FFmpegMedia::OutputFileInit()
	{
		char* filename= (char*) ((Marshal::StringToHGlobalAnsi (OutputFilename)).ToPointer ());

		out_format_context = avformat_alloc_context();
		out_format_context->oformat = av_guess_format(NULL, filename, NULL);

		if (out_format_context->oformat == NULL)
		{
			throw gcnew Exception(L"Could not guess output format");
		}

		out_format_context->oformat->flags|=AVFMT_VARIABLE_FPS;
	}

	void FFmpegMedia::OutputVideoCodecInit()
	{
		out_video_stream = av_new_stream(out_format_context, 0);
		if (!out_video_stream) 
		{
			throw gcnew Exception(L"Could not alloc stream");
		}

		out_video_codec_context = out_video_stream->codec;
		out_video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;  

		switch (VideoEncoderParams->ID)
		{
		case CodecType::THEORA:
			out_video_codec = avcodec_find_encoder(AV_CODEC_ID_THEORA);
			break;

		case CodecType::VP8:
			out_video_codec = avcodec_find_encoder(AV_CODEC_ID_VP8);
			break;

		case CodecType::H264:
			out_video_codec = avcodec_find_encoder(AV_CODEC_ID_H264);
			//av_opt_set(out_video_codec_context->priv_data, "preset", "ultrafast", 0); 
			//out_video_codec_context->has_b_frames=0;
			//out_video_codec_context->max_b_frames=0;
			out_video_codec_context->qmin = 10;
			out_video_codec_context->qmax = 51;
			break;

		default:
			out_video_codec = avcodec_find_encoder(AV_CODEC_ID_MPEG4);
			//out_video_codec_context->has_b_frames=0;
			//out_video_codec_context->max_b_frames=0;
			break;
		}

		out_video_codec_context->bit_rate = 10000000;
		out_video_codec_context->width = VideoEncoderParams->Width;
		out_video_codec_context->height = VideoEncoderParams->Height;
		out_video_codec_context->time_base.den = 30;
		out_video_codec_context->time_base.num = 1;
		out_video_codec_context->gop_size = 30;
		out_video_codec_context->pix_fmt = PIX_FMT_YUV420P;
		out_video_codec_context->thread_count = 2;

		if(out_format_context->oformat->flags & AVFMT_GLOBALHEADER)
			out_video_codec_context->flags |= CODEC_FLAG_GLOBAL_HEADER;

		if(out_video_codec == NULL) 
		{
			throw gcnew Exception(L"Unsupported output codec!");
		}

		if(avcodec_open2(out_video_codec_context, out_video_codec,NULL) < 0)
		{
			throw gcnew Exception(L"Could not open codec!");
		}

		OutputFramesAlloc();
	}

	void FFmpegMedia::OutputFramesAlloc()
	{

		enc_video_frame = avcodec_alloc_frame();
		int buffer_size = avpicture_get_size(out_video_codec_context->pix_fmt, out_video_codec_context->width, out_video_codec_context->height);
		enc_video_buffer=(uint8_t *)av_malloc(buffer_size * sizeof(uint8_t));

		avpicture_fill((AVPicture *)enc_video_frame, enc_video_buffer, out_video_codec_context->pix_fmt, out_video_codec_context->width, out_video_codec_context->height);

		yuv_convert_context = sws_getCachedContext(NULL, 
			in_video_codec_context->width,in_video_codec_context->height,
			/*in_video_codec_context->pix_fmt*/PIX_FMT_BGR24,
			out_video_codec_context->width,out_video_codec_context->height,
			out_video_codec_context->pix_fmt,
			SWS_GAUSS , NULL, NULL, NULL);

		if (yuv_convert_context == NULL) {
			throw gcnew Exception(L"Could not open yuv_convert_context!");
		}

		//enc_audio_frame= av_frame_alloc();//avcodec_alloc_frame();
		////buffer_size=192000 + FF_INPUT_BUFFER_PADDING_SIZE;

		//buffer_size = av_samples_get_buffer_size(NULL, out_audio_codec_context->channels, 
		//	out_audio_codec_context->frame_size,
		//	out_audio_codec_context->sample_fmt, 
		//	0);

		//enc_audio_buffer =(uint8_t *)av_malloc(buffer_size);
	}

	void FFmpegMedia::OutputAudioCodecInit()
	{
		out_audio_stream = av_new_stream(out_format_context, 1);

		if (!out_audio_stream) {
			throw gcnew Exception(L"Could not alloc audio stream!");

		}
		out_audio_codec_context= out_audio_stream->codec;

		out_audio_codec = avcodec_find_encoder(AV_CODEC_ID_FLAC/*AV_CODEC_ID_PCM_S16LE*/);// ÁÅÇ ÊÎÄÈÐÎÂÀÍÈß

		if(out_audio_codec == NULL) {
			throw gcnew Exception(L"Unsupported output codec!");
		}

		//out_audio_codec_context->sample_fmt = *out_audio_codec->sample_fmts; 

		//out_audio_codec_context->bit_rate = 64000;
		out_audio_codec_context->sample_fmt = AV_SAMPLE_FMT_S16;

		if(!check_sample_fmt(out_audio_codec, out_audio_codec_context->sample_fmt))
		{
			throw gcnew Exception(L"Encoder does not support sample format");
		}
		// ÁÅÇ ÐÅÑÅÌÏËÈÍÃÀ Ò.Å ×ÀÑÒÎÒÀ ÄÅÑÊÐÅÒÈÇÀÖÈÈ ÄÎËÆÍÀ ÑÎÂÏÀÄÀÒÜ
		out_audio_codec_context->sample_rate = 8000;//select_sample_rate(out_audio_codec);;

		//out_audio_codec_context->channel_layout = select_channel_layout(out_audio_codec); //av_get_channel_layout("mono"); //2;//select_channel_layout(out_audio_codec);
		out_audio_codec_context->channels    = 1;//av_get_channel_layout_nb_channels(out_audio_codec_context->channel_layout);

		//out_audio_codec_context->frame_size=320;//320;

		if(out_format_context->oformat->flags & AVFMT_GLOBALHEADER)
			out_audio_codec_context->flags |= CODEC_FLAG_GLOBAL_HEADER;


		if(avcodec_open2(out_audio_codec_context, out_audio_codec, NULL) < 0){ 
			throw gcnew Exception(L"Could not open codec!");
		}

		//enc_audio_frame= av_frame_alloc();

		//enc_audio_frame->nb_samples     = out_audio_codec_context->frame_size;
		//enc_audio_frame->format         = out_audio_codec_context->sample_fmt;
		//enc_audio_frame->channel_layout = out_audio_codec_context->channel_layout;

		//int buffer_size = av_samples_get_buffer_size(NULL, 
		//	out_audio_codec_context->channels, 
		//	out_audio_codec_context->frame_size,
		//	out_audio_codec_context->sample_fmt, 
		//	0);

		//enc_audio_buffer =(uint8_t *)av_malloc(buffer_size);


		///* create resampler context */
		//if (out_audio_codec_context->sample_fmt != AV_SAMPLE_FMT_S16) {
		//	swr_ctx = swr_alloc();

		//	if (!swr_ctx) {
		//		throw gcnew Exception( "Could not allocate resampler context\n");
		//	}
		//	/* set options */
		//	av_opt_set_int (swr_ctx, "in_channel_count", out_audio_codec_context->channels, 0);
		//	av_opt_set_int (swr_ctx, "in_sample_rate", out_audio_codec_context->sample_rate, 0);
		//	av_opt_set_sample_fmt(swr_ctx, "in_sample_fmt", AV_SAMPLE_FMT_S16, 0);
		//	av_opt_set_int (swr_ctx, "out_channel_count", out_audio_codec_context->channels, 0);
		//	av_opt_set_int (swr_ctx, "out_sample_rate", out_audio_codec_context->sample_rate, 0);
		//	av_opt_set_sample_fmt(swr_ctx, "out_sample_fmt", out_audio_codec_context->sample_fmt, 0);

		//	/* initialize the resampling context */
		//	if (swr_init(swr_ctx) < 0) {
		//		throw gcnew Exception("Failed to initialize the resampling context\n");
		//	}
		//}

	}

	int FFmpegMedia::select_sample_rate(AVCodec *codec)
	{
		const int *p;
		int best_samplerate = 0;

		if (!codec->supported_samplerates)
			return 44100;

		p = codec->supported_samplerates;
		while (*p) {
			best_samplerate = FFMAX(*p, best_samplerate);
			p++;
		}
		return best_samplerate;
	}

	int  FFmpegMedia::check_sample_fmt(AVCodec *codec, enum AVSampleFormat sample_fmt)
	{
		const enum AVSampleFormat *p = codec->sample_fmts;

		while (*p != AV_SAMPLE_FMT_NONE) {
			if (*p == sample_fmt)
				return 1;
			p++;
		}
		return 0;
	}

	int FFmpegMedia:: select_channel_layout(AVCodec *codec)
	{
		const uint64_t *p;
		uint64_t best_ch_layout = 0;
		int best_nb_channels   = 0;

		if (!codec->channel_layouts)
			return AV_CH_LAYOUT_STEREO;

		p = codec->channel_layouts;
		while (*p) {
			int nb_channels = av_get_channel_layout_nb_channels(*p);

			if (nb_channels > best_nb_channels) {
				best_ch_layout    = *p;
				best_nb_channels = nb_channels;
			}
			p++;
		}
		return best_ch_layout;
	}

	void FFmpegMedia::OutputOpen() 
	{
		char* filename= (char*) ((Marshal::StringToHGlobalAnsi (OutputFilename)).ToPointer ());
		//av_dump_format(out_format_context, 0, filename, 0);

		if(avio_open(&out_format_context->pb, filename, AVIO_FLAG_WRITE)<0){
			throw gcnew Exception(L"Error occurred when opening output file");
		}

		if (avformat_write_header(out_format_context, NULL) < 0) {
			throw gcnew Exception(L"Error occurred when opening output file");
		}
	}

#pragma endregion

#pragma region VIDEO_PROCESSING

	void FFmpegMedia::VideoDataReset()
	{
		videoCounter=gcnew  SessionValueCounter();
	}

	void FFmpegMedia::VideoDataProcessing(IntPtr data,  int size, bool key, unsigned long time)
	{
#ifdef UNMANAGED
		reader->video_data_decoding(reinterpret_cast<uint8_t*>(data.ToPointer()), size, key, time);
#else

		videoCounter->CurrTimestamp = time;

		if (videoCounter->ReceivedFrameCount == 0)
			videoCounter->LastTimestamp = videoCounter->CurrTimestamp;

		double timeInterval = ((double)videoCounter->CurrTimestamp - (double)videoCounter->LastTimestamp)/90000; // èíòåðâàë â ñåêóíäàõ
		videoCounter->TimeInterval += (unsigned long)(timeInterval * 1000); //ìèëëèñåêóíäû

		AVPacket packet;
		av_new_packet(&packet, size);
		memcpy(packet.data, reinterpret_cast<uint8_t*>(data.ToPointer()), size);

		// mSec <--> ffmpeg 
		// AV_TIME_BASE * time_in_seconds = avcodec_timestamp
		// AV_TIME_BASE_Q * avcodec_timestamp = time_in_seconds 
		packet.pts=videoCounter->TimeInterval*AV_TIME_BASE/1000; 
		packet.dts=videoCounter->TimeInterval*AV_TIME_BASE/1000;

		packet.flags = (key==true) ? 1 : 0;

		int got_video_frame;
		if(avcodec_decode_video2(in_video_codec_context, dec_video_frame, &got_video_frame, &packet)<0)
		{
			throw gcnew Exception(L"Error while decoding video!");
		}

		if (got_video_frame>0) 
		{
#ifdef USE_SDL
			SDL_LockYUVOverlay(bmp);

			AVPicture pict;
			pict.data[0] = bmp->pixels[0];
			pict.data[1] = bmp->pixels[2];  // it's because YV12
			pict.data[2] = bmp->pixels[1];

			pict.linesize[0] = bmp->pitches[0];
			pict.linesize[1] = bmp->pitches[2];
			pict.linesize[2] = bmp->pitches[1];

			sws_scale(img_convert_context,
				dec_video_frame->data, dec_video_frame->linesize,
				0, in_video_codec_context->height,
				pict.data, pict.linesize);

			SDL_UnlockYUVOverlay(bmp);

			SDL_Rect rect;
			rect.x = 0;
			rect.y = 0;
			rect.w = in_video_codec_context->width;
			rect.h = in_video_codec_context->height;
			SDL_DisplayYUVOverlay(bmp, &rect);

#endif			
			// êîíâåðòèðîâàíèå ôðåéìà èç YUV420 â BGR24  
			sws_scale(rgb_convert_context, dec_video_frame->data, dec_video_frame->linesize, 0, dec_video_frame->height,
				dec_rgb_frame->data, dec_rgb_frame->linesize);

			 //WriteVideoDataToFile((IntPtr)(dec_rgb_frame->data[0]), dec_rgb_frame->linesize[0],videoCounter->TimeInterval, 0 );

			VideoFrameReceived((IntPtr)(dec_rgb_frame->data[0]), dec_rgb_frame->linesize[0], 
				in_video_codec_context->width, in_video_codec_context->height , 
				0,
				videoCounter->TimeInterval, packet.flags); //, in_video_codec_context->width, in_video_codec_context->height);
		}

		videoCounter->ReceivedFrameCount++;
		videoCounter->LastTimestamp=videoCounter->CurrTimestamp; //time

		av_free_packet(&packet);

#ifdef USE_SDL
		SDL_Event event;
		if (SDL_PollEvent(&event)) 
		{
			if (event.type == SDL_QUIT) 
			{
				//break;
			}
		}
#endif

#endif
	}

#ifdef UNMANAGED
	void FFmpegMedia::VideoCalback(IntPtr data, int size, int width ,int height, unsigned long count, unsigned long time, int flag)
	{
		VideoFrameReceived(data, size, width, height , 0, time, flag);
	}
#endif

	void FFmpegMedia::WriteVideoDataToFile(IntPtr data, int size, unsigned long time, int duration)
	{
#ifdef UNMANAGED
		writer->write_video_data_to_file(reinterpret_cast<uint8_t*>(data.ToPointer()), size, time, duration);
#else
		uint8_t* srcData[4] = { reinterpret_cast<uint8_t*>(data.ToPointer()), NULL, NULL, NULL };
		const int scrSize[4]={ size, NULL, NULL, NULL};

		// BGR24 -> YUV420
		 sws_scale(yuv_convert_context,srcData , scrSize, 0, dec_video_frame->height,
			 enc_video_frame->data, enc_video_frame->linesize);

		//enc_video_frame->pts=time*AV_TIME_BASE/1000; 

		AVPacket packet;
		av_init_packet(&packet);
		packet.data = NULL; 

		int got_output;
		int ret = avcodec_encode_video2(out_video_codec_context, &packet, enc_video_frame, &got_output);
		if (ret < 0) 
		{
			throw gcnew Exception(L"Error while encode video");
		}

		if (got_output) 
		{
			//if (out_video_codec_context->coded_frame->pts != AV_NOPTS_VALUE)
				//packet.pts = av_rescale_q(out_video_codec_context->coded_frame->pts, out_video_codec_context->time_base, out_video_stream->time_base);

			packet.pts=time;
			packet.dts=time;
			packet.duration=duration;

			if (out_video_codec_context->coded_frame->key_frame)
				packet.flags |= AV_PKT_FLAG_KEY;
			packet.stream_index = out_video_stream->index;

			System::Threading::Monitor::Enter(locker);
			try
			{
				int ret = av_interleaved_write_frame(out_format_context, &packet);
				//ret = av_write_frame(out_format_context, &packet);
				if (ret < 0) 
				{
					throw gcnew Exception(L"Error while write video");
				}
			}
			finally
			{
				System::Threading::Monitor::Exit(locker);
			}
			
		}

		av_free_packet(&packet);

#endif
	}

#pragma endregion

#pragma region AUDIO_PROCESSING

	void FFmpegMedia::AudioDataReset()
	{
		audioCounter=gcnew SessionValueCounter();
	}

	void FFmpegMedia::AudioDataProcessing(IntPtr data,  int size, bool key, unsigned long time)
	{
#ifdef UNMANAGED
		reader->audio_data_decoding(reinterpret_cast<uint8_t*>(data.ToPointer()), size, key, time);
#else
		audioCounter->CurrTimestamp = time;

		if (audioCounter->ReceivedFrameCount == 0)
			audioCounter->LastTimestamp = audioCounter->CurrTimestamp;

		double timeInterval = ((double)audioCounter->CurrTimestamp - (double)audioCounter->LastTimestamp)/8000; // èíòåðâàë â ñåêóíäàõ
		audioCounter->TimeInterval += (unsigned long)(timeInterval * 1000); //ìèëëèñåêóíäû

		AVPacket packet;
		av_new_packet(&packet, size);
		memcpy(packet.data, reinterpret_cast<uint8_t*>(data.ToPointer()), size);

		// mSec <--> ffmpeg 
		// AV_TIME_BASE * time_in_seconds = avcodec_timestamp
		// AV_TIME_BASE_Q * avcodec_timestamp = time_in_seconds 
		packet.pts=audioCounter->TimeInterval*AV_TIME_BASE/1000; 
		packet.dts=audioCounter->TimeInterval*AV_TIME_BASE/1000;

		int frame_finished;
		if (avcodec_decode_audio4(in_audio_codec_context, dec_audio_frame, &frame_finished, &packet) < 0)
		{
			throw gcnew Exception(L"Error while decoding audio");
		}

		if(frame_finished>0)
		{
			AudioFrameReceived((IntPtr)(dec_audio_frame->data[0]), dec_audio_frame->linesize[0], audioCounter->TimeInterval);
			//WriteAudioDataToFile((IntPtr)(dec_audio_frame/*->data[0]*/), /*dec_audio_frame->linesize[0], */audioCounter->TimeInterval, 30);
		}

		audioCounter->ReceivedFrameCount++;
		audioCounter->LastTimestamp=time;

		av_free_packet(&packet);
#endif
	}

#ifdef UNMANAGED
	void FFmpegMedia::AudioCalback(IntPtr data,  int size, unsigned long time)
	{
		AudioFrameReceived(data, size, time);
	}
#endif

	void FFmpegMedia::WriteAudioDataToFile( IntPtr data, int size, unsigned long time, int duration)
	{
#ifdef UNMANAGED
		writer->write_audio_data_to_file(reinterpret_cast<uint8_t*>(data.ToPointer()), size, time, duration);
#else
		// ÁÅÇ ÐÅÑÅÌÏËÈÍÃÀ !!!
		AVFrame *frame=avcodec_alloc_frame();

		frame->nb_samples = 320;//dec_audio_frame->nb_samples;  // ÁÅÇ ÝÒÎÃÎ - ÍÅ ÐÀÁÎÒÀÅÒ !!!

		avcodec_fill_audio_frame(frame, 
			in_audio_codec_context->channels, 
			in_audio_codec_context->sample_fmt, 
			reinterpret_cast<uint8_t*>(data.ToPointer()), size, 
			0);

		AVPacket packet;
		av_init_packet(&packet);
		packet.data = NULL;    		

		int got_output;
		int ret = avcodec_encode_audio2(out_audio_codec_context, &packet, frame, &got_output);

		if (ret < 0) 
		{
			throw gcnew Exception(L"Error encoding audio frame");
		}

		if (got_output) 
		{
			//if (out_audio_codec_context->coded_frame->pts != AV_NOPTS_VALUE)
			//	packet.pts = av_rescale_q(out_audio_codec_context->coded_frame->pts, out_audio_codec_context->time_base, out_audio_stream->time_base);


			packet.pts=time;
			packet.dts=time;
			packet.duration= duration;

			//if (out_audio_codec_context->coded_frame->key_frame)
			//	packet.flags |= AV_PKT_FLAG_KEY;

			packet.stream_index = out_audio_stream->index;

			System::Threading::Monitor::Enter(locker);
			try
			{
				int ret = av_interleaved_write_frame(out_format_context, &packet);
				//int ret = av_write_frame(out_format_context, &packet);
				if (ret < 0)
				{
					throw gcnew Exception(L"Error while write audio");
				} 
			}
			finally
			{
				System::Threading::Monitor::Exit(locker);		
			}
		}

		av_free_packet(&packet);
#endif
	}
#pragma endregion

#pragma region FFMPEG_STOP

	void FFmpegMedia::InputClose(){}

	void FFmpegMedia::OutputClose(){}

	void FFmpegMedia::Close()
	{
#ifdef UNMANAGED
		writer->close();
#else
		av_free(dec_rgb_frame);
		av_free(enc_video_frame);
		av_free(dec_video_frame);

		av_free(enc_audio_frame);
		av_free(dec_audio_frame);

		//av_write_trailer(out_format_context);
		//avcodec_close(out_video_codec_context);
		//avio_close(out_format_context->pb);

		avcodec_close(in_video_codec_context);

		sws_freeContext(rgb_convert_context);
		sws_freeContext(yuv_convert_context);
#endif
	}

#pragma endregion



}

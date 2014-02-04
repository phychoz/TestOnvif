#pragma unmanaged

//#pragma comment (lib, "C:\\ffmpeg\\lib\\avcodec.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\swscale.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avformat.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avutil.lib")

//#pragma comment (lib, "C:\\FFmpeg2\\lib\\avcodec.lib")
//#pragma comment (lib, "C:\\FFmpeg2\\lib\\swscale.lib")
//#pragma comment (lib, "C:\\FFmpeg2\\lib\\avformat.lib")
//#pragma comment (lib, "C:\\FFmpeg2\\lib\\avutil.lib")
//
//#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDL.lib")
//#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDLmain.lib")

extern "C" 
{
#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/mathematics.h>
}

#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string>
#include <exception>

#include "ffmpeg_native_reader.h"

namespace FFmpegWrapper
{
	void  ffmpeg_native_reader::open(char* c, int w, int h)
	{
		codec =c;
		width=w;
		height=h;

		audio_counter=new session_counter;

		audio_counter->curr_timestamp=0;
		audio_counter->last_timestamp=0;
		audio_counter->recieved_frame_count=0;
		audio_counter->time_interval=0;

		video_counter=new session_counter;

		video_counter->curr_timestamp=0;
		video_counter->last_timestamp=0;
		video_counter->recieved_frame_count=0;
		video_counter->time_interval=0;

		init_video_codec();
		init_audio_codec();
	}

	void  ffmpeg_native_reader::init_video_codec()
	{
		if(strcoll(codec, "JPEG")==0)
		{
			video_codec = avcodec_find_decoder(AV_CODEC_ID_MJPEG);
		}
		else if(strcoll(codec, "H264")==0)
		{
			video_codec = avcodec_find_decoder(AV_CODEC_ID_H264);
		}
		else if(strcoll(codec, "MPEG4")==0)
		{
			video_codec = avcodec_find_decoder(AV_CODEC_ID_MPEG4);
		}

		video_codec_context = avcodec_alloc_context3(video_codec);
		video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;

		video_codec_context->pix_fmt = PIX_FMT_YUV420P;
		video_codec_context->width=width;
		video_codec_context->height=height;

		if (avcodec_open2(video_codec_context, video_codec, NULL) < 0) 
			throw std::exception("ffmpeg: Unable to open video codec");

		alloc_frames();
	}


	void  ffmpeg_native_reader::alloc_frames()
	{
		audio_frame=avcodec_alloc_frame();
		video_frame = avcodec_alloc_frame();

		rgb_frame = avcodec_alloc_frame();

		int buffer_size = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
		rgb_buffer=(uint8_t *)av_malloc(buffer_size*sizeof(uint8_t));
		avpicture_fill((AVPicture *)rgb_frame, rgb_buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

		rgb_convert_context = sws_getCachedContext(NULL,
			video_codec_context->width, video_codec_context->height,
			video_codec_context->pix_fmt,
			video_codec_context->width, video_codec_context->height,
			PIX_FMT_BGR24, SWS_BICUBIC , /*SWS_FAST_BILINEAR,*/
			//PIX_FMT_YUV420P, SWS_BICUBIC,
			NULL, NULL, NULL);

		if (rgb_convert_context == NULL) {
			throw std::exception("Cannot initialize the conversion context");
		}
	}

	void  ffmpeg_native_reader::init_audio_codec()
	{
		audio_codec = avcodec_find_decoder(AV_CODEC_ID_PCM_MULAW);

		audio_codec_context = avcodec_alloc_context3(audio_codec);
		audio_codec_context->codec_type = AVMEDIA_TYPE_AUDIO;

		audio_codec_context->sample_fmt = *audio_codec->sample_fmts;   
		//in_audio_codec_context->bit_rate = 64000;
		audio_codec_context->sample_rate = 8000;
		audio_codec_context->channels    = 1;

		if (avcodec_open2(audio_codec_context, audio_codec, NULL) < 0) 
			throw std::exception("ffmpeg: Unable to open video codec");
	}

	void ffmpeg_native_reader::register_video_handler(VIDEO_FRAME_DECODED callback)
	{
		video_frame_decoded=callback;
	}

	void ffmpeg_native_reader::register_audio_handler(AUDIO_FRAME_DECODED callback)
	{
		audio_frame_decoded=callback;
	}

	void ffmpeg_native_reader::video_data_decoding(uint8_t* data,  int size, bool key, unsigned long time)
	{
		video_counter->curr_timestamp = time;

		if (video_counter->recieved_frame_count == 0)
			video_counter->last_timestamp = video_counter->curr_timestamp;

		double interval = ((double)video_counter->curr_timestamp - (double)video_counter->last_timestamp)/VIDEO_CLOCKRATE;
		video_counter->time_interval += (unsigned long)(interval * 1000); 

		AVPacket packet;
		av_new_packet(&packet, size);
		memcpy(packet.data, data, size);

		packet.pts=video_counter->time_interval;
		packet.dts=video_counter->time_interval;

		packet.flags = (key==true) ? 1 : 0;

		int got_video_frame;
		if(avcodec_decode_video2(video_codec_context, video_frame, &got_video_frame, &packet)<0)
		{
			throw std::exception("Error while decoding video!");
		}

		if (got_video_frame>0) 
		{
			sws_scale(rgb_convert_context, video_frame->data, video_frame->linesize, 0, video_frame->height,
				rgb_frame->data, rgb_frame->linesize);

			video_frame_decoded(rgb_frame->data[0], rgb_frame->linesize[0], 
				video_codec_context->width, video_codec_context->height ,
				0, video_counter->time_interval, packet.flags);
		}

		video_counter->recieved_frame_count++;
		video_counter->last_timestamp=time;

		av_free_packet(&packet);
	}

	void ffmpeg_native_reader::audio_data_decoding(uint8_t* data,  int size, bool key, unsigned long time)
	{
		audio_counter->curr_timestamp = time;

		if (audio_counter->recieved_frame_count == 0)
			audio_counter->last_timestamp = audio_counter->curr_timestamp;

		double interval = ((double)audio_counter->curr_timestamp - (double)audio_counter->last_timestamp)/AUDIO_CLOCKRATE; 
		audio_counter->time_interval += (unsigned long)(interval * 1000);

		AVPacket packet;
		av_new_packet(&packet, size);
		memcpy(packet.data, data, size);

		packet.pts=audio_counter->time_interval;
		packet.dts=audio_counter->time_interval;

		int frame_finished;
		if (avcodec_decode_audio4(audio_codec_context, audio_frame, &frame_finished, &packet) < 0)
		{
			throw std::exception("Error while decoding audio");
		}

		if(frame_finished>0)
		{
			audio_frame_decoded(audio_frame->data[0],audio_frame->linesize[0], audio_counter->time_interval);
		}

		audio_counter->recieved_frame_count++;
		audio_counter->last_timestamp=time;

		av_free_packet(&packet);
	}
}
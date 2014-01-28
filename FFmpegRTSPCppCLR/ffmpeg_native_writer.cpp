#pragma unmanaged
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avcodec.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\swscale.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avformat.lib")
//#pragma comment (lib, "C:\\ffmpeg\\lib\\avutil.lib")

#pragma comment (lib, "C:\\FFmpeg2\\lib\\avcodec.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\swscale.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\avformat.lib")
#pragma comment (lib, "C:\\FFmpeg2\\lib\\avutil.lib")

#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDL.lib")
#pragma comment (lib, "C:\\SDL\\lib\\x86\\SDLmain.lib")

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

#include "ffmpeg_native_writer.h"

namespace FFmpegWrapper 
{
	void ffmpeg_native_writer::open(char* file, char* c, int w, int h )
	{
		filename=file;

		codec =c;
		width=w;
		height=h;

		init_file();
		init_video_codec(codec, width, height);
		alloc_frames();
		init_audio_codec();
		open_file();

	}

	void ffmpeg_native_writer::init_file()
	{
		format_context = avformat_alloc_context();
		format_context->oformat = av_guess_format(NULL, filename, NULL);

		if (format_context->oformat == NULL)
		{
			throw std::exception("Could not guess output format");
		}
	}

	void ffmpeg_native_writer::init_video_codec( char* codec, int width, int height)
	{
		video_stream = av_new_stream(format_context, 0);
		if (!video_stream) 
		{
			//throw gcnew Exception(L"Could not alloc stream");
		}

		video_codec_context = video_stream->codec;
		video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;  

		if(strcoll(codec, "THEORA")==0)
		{
			video_codec = avcodec_find_encoder(AV_CODEC_ID_THEORA);
		}
		else if(strcoll(codec, "VP8")==0)
		{
			video_codec = avcodec_find_encoder(AV_CODEC_ID_VP8);
		}
		else if(strcoll(codec, "H264")==0)
		{
			video_codec = avcodec_find_encoder(AV_CODEC_ID_H264);
			//av_opt_set(out_video_codec_context->priv_data, "preset", "ultrafast", 0); 
			video_codec_context->qmin = 10;
			video_codec_context->qmax = 51;
		}
		else //if(strcoll(codec, "MPEG4")==0)
		{
			video_codec = avcodec_find_encoder(AV_CODEC_ID_MPEG4);
		}

		video_codec_context->bit_rate = 10000000;
		video_codec_context->width = width;
		video_codec_context->height = height;
		video_codec_context->time_base.den = 30;
		video_codec_context->time_base.num = 1;
		video_codec_context->gop_size = 10;
		video_codec_context->pix_fmt = PIX_FMT_YUV420P;
		video_codec_context->thread_count = 4;

		if(format_context->oformat->flags & AVFMT_GLOBALHEADER)
			video_codec_context->flags |= CODEC_FLAG_GLOBAL_HEADER;

		if(video_codec == NULL) 
		{
			throw std::exception("Unsupported output codec!");
		}

		if(avcodec_open2(video_codec_context, video_codec,NULL) < 0)
		{
			throw std::exception("Could not open codec!");
		}

		alloc_frames();
	}

	void ffmpeg_native_writer::alloc_frames()
	{

		video_frame = avcodec_alloc_frame();
		int buffer_size = avpicture_get_size(video_codec_context->pix_fmt, video_codec_context->width, video_codec_context->height);
		enc_video_buffer=(uint8_t *)av_malloc(buffer_size * sizeof(uint8_t));

		avpicture_fill((AVPicture *)video_frame, enc_video_buffer, video_codec_context->pix_fmt, video_codec_context->width, video_codec_context->height);

		yuv_convert_context = sws_getCachedContext(NULL, 
			width, height,
			/*in_video_codec_context->pix_fmt*/PIX_FMT_BGR24,
			video_codec_context->width,video_codec_context->height,
			video_codec_context->pix_fmt,
			SWS_GAUSS , NULL, NULL, NULL);

		if (yuv_convert_context == NULL) 
		{
			throw std::exception("Could not open yuv_convert_context!");
		}

			audio_frame= av_frame_alloc();

			buffer_size = av_samples_get_buffer_size(NULL, audio_codec_context->channels, 
			audio_codec_context->frame_size,
			audio_codec_context->sample_fmt, 
			0);

			enc_audio_buffer =(uint8_t *)av_malloc(buffer_size);

		//audio_frame=avcodec_alloc_frame();
		//buffer_size=192000 + FF_INPUT_BUFFER_PADDING_SIZE;
		//enc_audio_buffer =(uint8_t *)av_malloc(buffer_size);
	}

	void ffmpeg_native_writer::init_audio_codec()
	{
		audio_stream = av_new_stream(format_context, 1);

		if (!audio_stream) 
		{
			throw std::exception("Could not alloc audio stream!");
		}

		audio_codec_context= audio_stream->codec;

		audio_codec = avcodec_find_encoder(AV_CODEC_ID_FLAC);

		if(audio_codec == NULL) 
		{
			throw std::exception("Unsupported output codec!");
		}

		audio_codec_context->sample_fmt = *audio_codec->sample_fmts; 
		//out_audio_codec_context->bit_rate = 64000;
		audio_codec_context->sample_rate = 8000;
		//out_audio_codec_context->time_base = av_d2q(1.0 / 8000, 1000000);

		audio_codec_context->channel_layout = 2;//select_channel_layout(out_audio_codec);
		audio_codec_context->channels    = 1;//av_get_channel_layout_nb_channels(out_audio_codec_context->channel_layout);//1;

		audio_codec_context->frame_size=320;

		if(format_context->oformat->flags & AVFMT_GLOBALHEADER)
			audio_codec_context->flags |= CODEC_FLAG_GLOBAL_HEADER;


		//if( avcodec_fill_audio_frame(frame, c->channels, c->sample_fmt, (const uint8_t*)samples, buffer_size, 0) < 0) 
		//{
		//	throw std::exception( "Could not setup audio frame\n");
		//}

		if(avcodec_open2(audio_codec_context, audio_codec, NULL) < 0)
		{ 
			throw std::exception("Could not open codec!");
		}

	}

	void ffmpeg_native_writer::open_file() 
	{
		//av_dump_format(out_format_context, 0, filename, 0);

		if(avio_open(&format_context->pb, filename, AVIO_FLAG_WRITE)<0)
		{
			throw std::exception("Error occurred when opening output file");
		}

		if (avformat_write_header(format_context, NULL) < 0)
		{
			throw std::exception("Error occurred when opening output file");
		}
	}

	void ffmpeg_native_writer::write_video_data_to_file(uint8_t* data, int size, unsigned long time, int duration)
	{
		const int scrSize[4]={ size, NULL, NULL, NULL};

		sws_scale(yuv_convert_context,&data , scrSize, 0, height,
			video_frame->data, video_frame->linesize);

		AVPacket packet;

		int got_output;

		av_init_packet(&packet);
		packet.data = NULL; 
		packet.size = 0;

		packet.pts=time;//*AV_TIME_BASE;
		packet.dts=time;//*AV_TIME_BASE;
		packet.duration=duration;

		int ret = avcodec_encode_video2(video_codec_context, &packet, video_frame, &got_output);
		if (ret < 0) 
		{
			throw std::exception("Error while encode video");
		}

		if (got_output) 
		{
			if (video_codec_context->coded_frame->pts != AV_NOPTS_VALUE)
				packet.pts = av_rescale_q(video_codec_context->coded_frame->pts, video_codec_context->time_base, video_stream->time_base);

			//packet.pts=time;
			//packet.dts=time;
			//packet.duration=duration;

			if (video_codec_context->coded_frame->key_frame)
				packet.flags |= AV_PKT_FLAG_KEY;

			packet.stream_index = video_stream->index;

			int ret = av_interleaved_write_frame(format_context, &packet);
			//ret = av_write_frame(out_format_context, &packet);
			if (ret < 0) 
			{
				throw std::exception("Error while write video");
			}
		}
		av_free_packet(&packet);
	}

	void ffmpeg_native_writer::write_audio_data_to_file(uint8_t* data, int size, unsigned long time, int duration)
	{
		AVFrame *frame=avcodec_alloc_frame();

		frame->nb_samples = audio_codec_context->frame_size;
		frame->format = audio_codec_context->sample_fmt;
		frame->channel_layout = audio_codec_context->channel_layout;
		frame->data[0]=data;//reinterpret_cast<uint8_t*>(data.ToPointer());
		frame->linesize[0]=size;

		AVPacket packet;
		int got_output;

		av_init_packet(&packet);
		packet.data = NULL;    
		packet.size = 0;
		packet.pts=time;
		packet.dts=time;
		packet.duration= duration;

		int ret = avcodec_encode_audio2(audio_codec_context, &packet, frame, &got_output);

		if (ret < 0) 
		{
			throw std::exception("Error encoding audio frame");
		}

		if (got_output) 
		{
			//if (audio_codec_context->coded_frame->pts != AV_NOPTS_VALUE)
			//	packet.pts = av_rescale_q(audio_codec_context->coded_frame->pts, audio_codec_context->time_base, audio_stream->time_base);

			packet.pts=time;
			packet.dts=time;
			packet.duration= duration;

			if (audio_codec_context->coded_frame->key_frame)
				packet.flags |= AV_PKT_FLAG_KEY;

			packet.stream_index = audio_stream->index;

			int ret = av_interleaved_write_frame(format_context, &packet);
			//int ret = av_write_frame(out_format_context, &packet);
			if (ret < 0)
			{
				throw std::exception("Error while write audio");
			} 
			av_free_packet(&packet);
		}
	}
	void ffmpeg_native_writer::close()
	{
		sws_freeContext(yuv_convert_context);

		av_free(video_frame);
		av_free(audio_frame);

		av_write_trailer(format_context);
		avcodec_close(video_codec_context);
		avio_close(format_context->pb);
		
	}

}
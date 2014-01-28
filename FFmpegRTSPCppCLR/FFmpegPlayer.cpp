#pragma unmanaged

extern "C" 
{
#include <C:\\ffmpeg\\include\\libavformat\\avformat.h> 
#include <C:\\ffmpeg\\include\\libavcodec\\avcodec.h>
#include <C:\\ffmpeg\\include\\libswscale\\swscale.h>
#include <C:\\ffmpeg\\include\\libavutil\\mathematics.h>
}

#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include<string>
#include <SDL.h>
#undef main

#pragma managed

#using <mscorlib.dll>
#using <System.Drawing.dll>
#include "FFmpegPlayer.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;

namespace FFmpegWrapper 
{
	void  FFmpegPlayer::Open(String ^ file)
	{
		InputFile=file;

		av_register_all();

		cli::pin_ptr<AVFormatContext*> p_format_context=&format_context;

		int result = avformat_open_input(p_format_context, 
			(char*) ((Marshal::StringToHGlobalAnsi (InputFile)).ToPointer ()), 
			NULL, NULL);

		if (result < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to open input file");

		result = avformat_find_stream_info(format_context, NULL);

		if (result < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to find stream info");

		video_stream_index=-1;
		audio_stream_index=-1;

		unsigned int index;
		for (index = 0; index < format_context->nb_streams; ++index) 
		{
			if (format_context->streams[index]->codec->codec_type == AVMEDIA_TYPE_VIDEO && video_stream_index<0) 
			{
				video_stream_index=index;
				break;
			}
		}

		for (index = 0; index < format_context->nb_streams; ++index) 
		{
			if(format_context->streams[index]->codec->codec_type==AVMEDIA_TYPE_AUDIO && audio_stream_index < 0) 
			{
				audio_stream_index=index;
				break;
			}
		}

		if (index == format_context->nb_streams) 
			throw gcnew Exception(L"ffmpeg: Unable to find av stream info ");

		
		video_codec_context = format_context->streams[video_stream_index]->codec;
		video_codec = avcodec_find_decoder(video_codec_context->codec_id);

		int video_result = avcodec_open2(video_codec_context, video_codec, NULL);

		if (video_result < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to open video codec");


		audio_codec_context = format_context->streams[audio_stream_index]->codec;
		audio_codec = avcodec_find_decoder(audio_codec_context->codec_id);

		int audio_result = avcodec_open2(audio_codec_context, audio_codec, NULL);

		if (audio_result < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to open audio codec");


		audio_frame=avcodec_alloc_frame();
		video_frame = avcodec_alloc_frame();

		rgb_frame = avcodec_alloc_frame();

		int buffer_size = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
		buffer=(uint8_t *)av_malloc(buffer_size*sizeof(uint8_t));
		avpicture_fill((AVPicture *)rgb_frame, buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

		img_convert_context = sws_getCachedContext(NULL,
			video_codec_context->width, video_codec_context->height,
			video_codec_context->pix_fmt,
			video_codec_context->width, video_codec_context->height,
			PIX_FMT_BGR24,SWS_FAST_BILINEAR,
			//PIX_FMT_YUV420P, SWS_BICUBIC,
			NULL, NULL, NULL);

		if (img_convert_context == NULL)
		{
			throw gcnew Exception(L"Cannot initialize the conversion context");
		}

	}

	void FFmpegPlayer::ProcessFile()
	{
		int result;
		AVPacket packet;
		while (av_read_frame(format_context, &packet) >= 0) 
		{
			if (packet.stream_index == video_stream_index) 
			{

				int got_video_frame;
				result =avcodec_decode_video2(video_codec_context, video_frame, &got_video_frame, &packet);

				if(result<0)
				{
					throw gcnew Exception(L"Error while decoding");			
				}
				if (got_video_frame) 
				{
					sws_scale(img_convert_context, video_frame->data, video_frame->linesize, 0, video_frame->height, rgb_frame->data, rgb_frame->linesize);

					VideoFrameReceived((IntPtr)(rgb_frame->data[0]), rgb_frame->linesize[0], 
						video_codec_context->width, video_codec_context->height,
						0,0,0);
				}
			}
			else if (packet.stream_index == audio_stream_index) 
			{
				int got_audio_frame;
				int result = avcodec_decode_audio4(audio_codec_context, audio_frame, &got_audio_frame, &packet);

				if (result < 0) 
				{
					throw gcnew Exception(L"Error while decoding audio ");
				}
				if (got_audio_frame) 
				{
					//int data_size = av_samples_get_buffer_size(NULL, audio_codec_context->channels,
					//	frame->nb_samples,audio_codec_context->sample_fmt, 1);

					//int size=data_size*sizeof(uint8_t);
					//IntPtr ptr=(IntPtr)(frame->data[0]);

					AudioFrameReceived((IntPtr)(audio_frame->data[0]), audio_frame->linesize[0], 0);
				}
			}

			av_free_packet(&packet);

		}
	}


	void  FFmpegPlayer::GetAVFrame(long time)
	{

		//List<MediaData^>^mediaDataBuffer =gcnew List<MediaData^>(10); 

		AVPacket packet;

		AVFrame* frame = avcodec_alloc_frame();
		AVFrame *frame_rgb = avcodec_alloc_frame();

		uint8_t *buffer;
		int num_bytes = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
		buffer=(uint8_t *)av_malloc(num_bytes*sizeof(uint8_t));
		avpicture_fill((AVPicture *)frame_rgb, buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

		//int64_t seek_target = av_rescale((int64_t)position, AV_TIME_BASE_Q, format_context->streams[0]->time_base, 0);

		// Convert time into frame number
		//int64_t frameNumber=AV_TIME_BASE * static_cast<int64_t>(time);
		int64_t frameNumber = av_rescale(time,format_context->streams[0]->time_base.den,format_context->streams[0]->time_base.num);
		frameNumber/=1000;

		//int64_t frameNumber=format_context->start_time + time * AV_TIME_BASE;

		if(av_seek_frame(format_context, video_stream_index, frameNumber, AVSEEK_FLAG_BACKWARD /*AVSEEK_FLAG_ANY*/ /* AVSEEK_FLAG_FRAME*/)>=0){
			avcodec_flush_buffers(format_context->streams[video_stream_index]->codec);
			//if(avformat_seek_file(format_context, video_stream, INT64_MIN, frameNumber, INT64_MAX, AVSEEK_FLAG_ANY)>=0){

			bool iFrameDiscover=false;
			int frameCount=100;
			while (av_read_frame(format_context, &packet)>=0 && frameCount-->0)
			{
				if (packet.stream_index == video_stream_index) 
				{
					// Если найден следующий ключевой кадр
					if (packet.flags==1 && iFrameDiscover==true)
						break;

					// Если найден ключевой кадр
					if(packet.flags==1 )
						iFrameDiscover=true;

					// Флаг получения кадра: <0 ошибка декодирования, ==0 в процессе, >0 получен кадр,
					int gotVideoFrame=0;

					// Количество попыток декодирования
					int decodeCount = 10;				
					while(decodeCount-->0){
						int len =avcodec_decode_video2(video_codec_context, frame, &gotVideoFrame, &packet);

						if(len<0){
							throw gcnew Exception(L"Error while decoding");		
						}
						if (gotVideoFrame>0) {
							sws_scale(img_convert_context, frame->data, frame->linesize, 0, frame->height, frame_rgb->data, frame_rgb->linesize);

							// 
							//MediaData^ mediaData= gcnew MediaData();
							////mediaData->Ptr= Marshal::AllocHGlobal(frame_rgb->linesize[0]); //(IntPtr)(frame_rgb->data[0]);
							////mediaData->Ptr=(IntPtr)(frame_rgb->data[0]);
							////mediaData->Size=frame_rgb->linesize[0];
							//mediaData->FrameNumber=frameNumber;
							//mediaData->Dts=packet.dts;
							//mediaData->Flags=packet.flags;

							//mediaDataBuffer->Add(mediaData);

							VideoFrameReceived((IntPtr)(frame_rgb->data[0]), frame_rgb->linesize[0],video_codec_context->width, video_codec_context->height, frameNumber, packet.dts, packet.flags);//, video_codec_context->width, video_codec_context->height);
							break;
						}
					}
				}
			}

			//if(mediaDataBuffer->Count>1){
			//	int delta=Int32::MaxValue;
			//	int mediaDataIndex=0;
			//	for(int index=0;index<mediaDataBuffer->Count;index++)
			//	{
			//if (mediaDataBuffer[index]->Dts != time)
			//{
			//	int tmp = Math::Abs(time - (int)mediaDataBuffer[index]->Dts);
			//	if (tmp <= delta)
			//	{
			//		delta = tmp;
			//	}
			//	else
			//	{
			//		mediaDataIndex=index-1;
			//		break;
			//	}
			//	continue;
			//}

			//if (mediaDataBuffer[index]->Dts == time)
			//{
			//	mediaDataIndex=index;
			//	break;
			//}
			//}
			//mediaDataIndex=	index;
			//VideoFrameReceived(
			//	//(IntPtr)mediaDataBuffer[mediaDataIndex]->Ptr, 
			//	mediaDataBuffer[mediaDataIndex]->Size, 
			//	mediaDataBuffer[mediaDataIndex]->FrameNumber, 
			//	mediaDataBuffer[mediaDataIndex]->Dts, 
			//	mediaDataBuffer[mediaDataIndex]->Flags);
			//}
			//}

			//else if(mediaDataBuffer->Count==1)
			//{
			//	VideoFrameReceived(
			//		(IntPtr)mediaDataBuffer[0]->Ptr, 
			//		mediaDataBuffer[0]->Size, 
			//		mediaDataBuffer[0]->FrameNumber, 
			//		mediaDataBuffer[0]->Dts, 
			//		mediaDataBuffer[0]->Flags);
			//}

			av_free_packet(&packet);
		}

		/*av_free_packet(&packet);*/
		av_free(buffer);

		av_free(frame);
		av_free(frame_rgb);

	}


	void FFmpegPlayer::DecodeVideoFrame(IntPtr data,  int size, int type)
	{
		AVPacket packet;
		packet.data=reinterpret_cast<uint8_t*>(data.ToPointer());
		packet.size=size;

		//AVPacket packet;
		//av_new_packet(&packet, orginal_packet.size);
		//memcpy(packet.data, orginal_packet.data, orginal_packet.size);

		////packet.pts=AV_NOPTS_VALUE;
		////packet.dts=AV_NOPTS_VALUE;

		////packet.flags=orginal_packet.flags;

		AVFrame *frame = avcodec_alloc_frame();
		AVFrame *frame_rgb = avcodec_alloc_frame();

		uint8_t *buffer;
		int num_bytes = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
		buffer=(uint8_t *)av_malloc(num_bytes*sizeof(uint8_t));
		avpicture_fill((AVPicture *)frame_rgb, buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

		//if (packet.stream_index == video_stream) {

		int got_video_frame;
		int result =avcodec_decode_video2(video_codec_context, frame, &got_video_frame, &packet);

		//if(result<0) throw gcnew Exception(L"Error while decoding");		

		//PacketReceived((IntPtr)packet.data, packet.size);

		if (got_video_frame) 
		{
			sws_scale(img_convert_context, frame->data, frame->linesize, 0, frame->height, frame_rgb->data, frame_rgb->linesize);

			VideoFrameReceived((IntPtr)(frame_rgb->data[0]), frame_rgb->linesize[0], video_codec_context->width, video_codec_context->height,0,0,1);//, video_codec_context->width, video_codec_context->height);
		}
		//}

		av_free(buffer);

		av_free(frame);
		av_free(frame_rgb);

		//av_free_packet(&packet);
	}

	long FFmpegPlayer::GetVideoDuration()
	{
		return format_context->duration;
	}

	void FFmpegPlayer::Close()
	{
		avcodec_close(video_codec_context);
		sws_freeContext(img_convert_context);

		cli::pin_ptr<AVFormatContext*> p=&format_context;
		avformat_close_input(p);
	}
}
#pragma unmanaged

extern "C" 
{
#include <libavformat/avformat.h> 
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
	//#include <libavutil/mem.h>
}

#include <stdio.h>
#include<string>
#include <SDL.h>
#undef main

#pragma managed

#using <mscorlib.dll>
#using <System.Drawing.dll>
#include "FFmpegMedia.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
//using namespace FFmpegMedia;

void  FFmpegWrapper::FFmpegMedia::AVRegisterAll(){

	av_register_all();
}
void  FFmpegWrapper::FFmpegMedia::AVFormatNetworkInit(){

	avformat_network_init();
}

void  FFmpegWrapper::FFmpegMedia::AVFormatOpenInput(){

	cli::pin_ptr<AVFormatContext*> p_format_context=&format_context;
	//AVFormatContext* p=NULL;
	int result = avformat_open_input(p_format_context, 
		(char*) ((Marshal::StringToHGlobalAnsi (mediaSource)).ToPointer ()), 
		NULL, NULL);

	if (result < 0) 
		throw gcnew Exception(L"ffmpeg: Unable to open input file");

}

void FFmpegWrapper::FFmpegMedia::AVFormatFindStreamInfo
	(){

		int result = avformat_find_stream_info(format_context, NULL);

		if (result < 0) 
			throw gcnew Exception(L"ffmpeg: Unable to find stream info");

		video_stream_index=-1;
		video_stream_index=-1;

		int index;
		for (index = 0; index < format_context->nb_streams; ++index) {
			if (format_context->streams[index]->codec->codec_type == AVMEDIA_TYPE_VIDEO && video_stream_index<0) {
				video_stream_index=index;
				break;
			}
		}

		for (index = 0; index < format_context->nb_streams; ++index) {
			if(format_context->streams[index]->codec->codec_type==AVMEDIA_TYPE_AUDIO && audio_stream_index < 0) {
				audio_stream_index=index;
				break;
			}
		}

		//if (index == format_context->nb_streams) 
		//	throw gcnew Exception(L"ffmpeg: Unable to find av stream info ");


}
void FFmpegWrapper::FFmpegMedia::AVCodecOpen2(){


	video_codec_context = format_context->streams[video_stream_index]->codec;
	video_codec = avcodec_find_decoder(video_codec_context->codec_id);

	//if(video_codec->capabilities & CODEC_CAP_TRUNCATED)
	//video_codec_context->flags|=CODEC_FLAG_TRUNCATED;

	//if (video_codec_context->codec_id == CODEC_ID_H264){
	//	video_codec_context->flags2 |= CODEC_FLAG2_CHUNKS;
	//}
	//video_codec_context->flags |=CODEC_FLAG_GLOBAL_HEADER;

	int video_result = avcodec_open2(video_codec_context, video_codec, NULL);

	if (video_result < 0) 
		throw gcnew Exception(L"ffmpeg: Unable to open video codec");


	//audio_codec_context = format_context->streams[audio_stream]->codec;
	//audio_codec = avcodec_find_decoder(audio_codec_context->codec_id);

	//int audio_result = avcodec_open2(audio_codec_context, audio_codec, NULL);

	//if (audio_result < 0) 
	//	throw gcnew Exception(L"ffmpeg: Unable to open audio codec");

}
void FFmpegWrapper::FFmpegMedia::SDLInitVideo(){

	int result = SDL_Init(SDL_INIT_VIDEO);
	if (result < 0) 
		throw gcnew Exception(L"Unable to init SDL: ");//, SDL_GetError());

	screen = SDL_SetVideoMode(video_codec_context->width, video_codec_context->height, 0, 0);
	if (screen == NULL)
		throw gcnew Exception(L"Couldn't set video mode");

	bmp = SDL_CreateYUVOverlay(video_codec_context->width, video_codec_context->height,
		SDL_YV12_OVERLAY, screen);

}

void FFmpegWrapper::FFmpegMedia::SwsGetCachedContext(){

	img_convert_context = sws_getCachedContext(NULL,
		video_codec_context->width, video_codec_context->height,
		video_codec_context->pix_fmt,
		video_codec_context->width, video_codec_context->height,
		PIX_FMT_BGR24, SWS_BICUBIC, /*SWS_FAST_BILINEAR,*/
		//PIX_FMT_YUV420P, SWS_BICUBIC,
		NULL, NULL, NULL);

	if (img_convert_context == NULL) {
		throw gcnew Exception(L"Cannot initialize the conversion context");
	}
}

int  FFmpegWrapper::FFmpegMedia::ShowVideo(){

	AVFrame* frame = avcodec_alloc_frame();
	AVPacket packet;

	while (av_read_frame(format_context, &packet) >= 0) {
		if (packet.stream_index == video_stream_index) {
			int frame_finished;
			int result =avcodec_decode_video2(video_codec_context, frame, &frame_finished, &packet);
			if(result<0){
				//exit(1);
				throw gcnew Exception(L"Error while decoding");			
			}
			if (frame_finished) {
				SDL_LockYUVOverlay(bmp);

				AVPicture pict;
				pict.data[0] = bmp->pixels[0];
				pict.data[1] = bmp->pixels[2]; 
				pict.data[2] = bmp->pixels[1];

				pict.linesize[0] = bmp->pitches[0];
				pict.linesize[1] = bmp->pitches[2];
				pict.linesize[2] = bmp->pitches[1];

				sws_scale(img_convert_context,frame->data, frame->linesize,0,
					video_codec_context->height,pict.data, pict.linesize);

				SDL_UnlockYUVOverlay(bmp);
				SDL_Rect rect;
				rect.x = 0;
				rect.y = 0;
				rect.w = video_codec_context->width;
				rect.h = video_codec_context->height;
				SDL_DisplayYUVOverlay(bmp, &rect);
			}
		}
		av_free_packet(&packet);

		SDL_Event event;
		if (SDL_PollEvent(&event)) {
			if (event.type == SDL_QUIT) {
				break;
			}
		}
	}
	av_free(frame);
}


void  FFmpegWrapper::FFmpegMedia::GetAVFrame(){

	exit=false;

	AVPacket packet;

	AVFrame* frame = avcodec_alloc_frame();
	AVFrame *frame_rgb = avcodec_alloc_frame();

	uint8_t *buffer;
	int num_bytes = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
	buffer=(uint8_t *)av_malloc(num_bytes*sizeof(uint8_t));
	avpicture_fill((AVPicture *)frame_rgb, buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

	//Bitmap^ bitmap = gcnew  Bitmap(video_codec_context->width, video_codec_context->height, Imaging::PixelFormat::Format24bppRgb);
	//Rectangle rect = Rectangle(0,0,bitmap->Width,bitmap->Height);
	while (av_read_frame(format_context, &packet) >= 0) {
		if(exit)
			break;

		if (packet.stream_index == video_stream_index) {

			int got_video_frame;
			int result =avcodec_decode_video2(video_codec_context, frame, &got_video_frame, &packet);

			if(result<0){
				throw gcnew Exception(L"Error while decoding");			
			}
			//PacketReceived((IntPtr)packet.data, packet.size);
			if (got_video_frame) {

				//BitmapData^ bmpData =bitmap->LockBits(rect, ImageLockMode::ReadWrite, bitmap->PixelFormat);
				//avpicture_fill((AVPicture *)frame_rgb, reinterpret_cast<uint8_t*>(bmpData->Scan0.ToPointer()), PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

				sws_scale(img_convert_context, frame->data, frame->linesize, 0, frame->height, frame_rgb->data, frame_rgb->linesize);

				VideoFrameReceived((IntPtr)(frame_rgb->data[0]), frame_rgb->linesize[0], video_codec_context->width, video_codec_context->height);

				//bitmap->UnlockBits(bmpData);
				//VideoFrameReceived(bitmap);

			}
		}

		else if (packet.stream_index == audio_stream_index) {

			int got_audio_frame;
			int audio_result = avcodec_decode_audio4(audio_codec_context, frame, &got_audio_frame, &packet);

			if (audio_result < 0) {
				throw gcnew Exception(L"Error while decoding audio ");
			}
			if (got_audio_frame) {

				int data_size = av_samples_get_buffer_size(NULL, audio_codec_context->channels,
					frame->nb_samples,audio_codec_context->sample_fmt, 1);

				int size=data_size*sizeof(uint8_t);
				IntPtr ptr=(IntPtr)(frame->data[0]);

				AudioFrameReceived((IntPtr)(frame->data[0]), frame->linesize[0]);
				//audioHandler(ptr, frame->linesize[0]);

			}
		}

		av_free_packet(&packet);

	}
	av_free(buffer);
	//delete bitmap;
	av_free(frame);
	av_free(frame_rgb);

}


void  FFmpegWrapper::FFmpegMedia::AVFormatOpenOutput(){

	av_register_all();

	cli::pin_ptr<AVFormatContext*> p_out_format_context=&out_format_context;

	//int result = avformat_open_input(p_out_format_context, 
	//	(char*) ((Marshal::StringToHGlobalAnsi (outMediaSource)).ToPointer ()), 
	//	NULL, NULL);

	int result=avformat_alloc_output_context2(p_out_format_context, 
		NULL, 
		NULL,
		(char*) ((Marshal::StringToHGlobalAnsi (outMediaSource)).ToPointer ()));

	if (result < 0) 
		throw gcnew Exception(L"ffmpeg: Unable to open output file");

	if (avio_open(&out_format_context->pb, 
		(char*) ((Marshal::StringToHGlobalAnsi (outMediaSource)).ToPointer ()), 
		AVIO_FLAG_WRITE) < 0) {
			throw gcnew Exception(L"ffmpeg: Unable to open output file");
        }

	output_format =out_format_context->oformat;

	video_stream = NULL;

	cli::pin_ptr<AVCodec*> p_video_codec=&video_codec;

	if (output_format->video_codec != AV_CODEC_ID_NONE) {
        video_stream = add_video_stream(out_format_context, p_video_codec, output_format->video_codec);
    }

	if (video_stream)
		open_video(out_format_context, video_codec, video_stream);

}

AVStream *  FFmpegWrapper::FFmpegMedia::add_video_stream(AVFormatContext *oc, AVCodec **codec,
                                  enum AVCodecID codec_id)
{
    AVCodecContext *c;
    AVStream *st;

    /* find the video encoder */
    *codec = avcodec_find_encoder(codec_id);
    if (!(*codec)) {
		throw gcnew Exception(L"ffmpeg: codec not found");
        //fprintf(stderr, "codec not found\n");
        //exit(1);
    }

    st = avformat_new_stream(oc, *codec);
    if (!st) {
		throw gcnew Exception(L"ffmpeg: could not alloc stream");
        //fprintf(stderr, "Could not alloc stream\n");
        //exit(1);
    }

    c = st->codec;

    avcodec_get_context_defaults3(c, *codec);

    c->codec_id = codec_id;

    /* Put sample parameters. */
    c->bit_rate = 400000;
    /* Resolution must be a multiple of two. */
    c->width    = 352;
    c->height   = 288;
    /* timebase: This is the fundamental unit of time (in seconds) in terms
     * of which frame timestamps are represented. For fixed-fps content,
     * timebase should be 1/framerate and timestamp increments should be
     * identical to 1. */
    c->time_base.den = 25;
    c->time_base.num = 1;
    c->gop_size      = 12; /* emit one intra frame every twelve frames at most */
    c->pix_fmt       = PIX_FMT_YUV420P;
    if (c->codec_id == AV_CODEC_ID_MPEG2VIDEO) {
        /* just for testing, we also add B frames */
        c->max_b_frames = 2;
    }
    if (c->codec_id == AV_CODEC_ID_MPEG1VIDEO) {
        /* Needed to avoid using macroblocks in which some coeffs overflow.
         * This does not happen with normal video, it just happens here as
         * the motion of the chroma plane does not match the luma plane. */
        c->mb_decision = 2;
    }
    /* Some formats want stream headers to be separate. */
    if (oc->oformat->flags & AVFMT_GLOBALHEADER)
        c->flags |= CODEC_FLAG_GLOBAL_HEADER;

    return st;
}

void FFmpegWrapper::FFmpegMedia::open_video(AVFormatContext *oc, AVCodec *codec, AVStream *st)
{
    int ret;
    AVCodecContext *c = st->codec;

    /* open the codec */
    if (avcodec_open2(c, codec, NULL) < 0) {
		throw gcnew Exception(L"ffmpeg: could not open codec");
        //fprintf(stderr, "Could not open codec\n");
        //exit(1);
    }

    video_outbuf = NULL;
    if (!(oc->oformat->flags & AVFMT_RAWPICTURE)) {
        /* Allocate output buffer. */
        /* XXX: API change will be done. */
        /* Buffers passed into lav* can be allocated any way you prefer,
         * as long as they're aligned enough for the architecture, and
         * they're freed appropriately (such as using av_free for buffers
         * allocated with av_malloc). */
        video_outbuf_size = 200000;
        video_outbuf      = (uint8_t*)av_malloc(video_outbuf_size);
    }

    /* allocate and init a re-usable frame */
  //  frame = avcodec_alloc_frame();
  //  if (!frame) {
		//throw gcnew Exception(L"ffmpeg: could not allocate video frame");
  //      //fprintf(stderr, "Could not allocate video frame\n");
  //      //exit(1);
  //  }

    /* Allocate the encoded raw picture. */
    ret = avpicture_alloc(dst_picture, PIX_FMT_YUV420P, c->width, c->height);
    if (ret < 0) {
		throw gcnew Exception(L"ffmpeg: could not not allocate picture");
        //fprintf(stderr, "Could not allocate picture\n");
        //exit(1);
    }

    /* If the output format is not YUV420P, then a temporary YUV420P
     * picture is needed too. It is then converted to the required
     * output format. */
    if (c->pix_fmt != PIX_FMT_YUV420P) {
        ret = avpicture_alloc(src_picture, PIX_FMT_YUV420P, c->width, c->height);
        if (ret < 0) {
			throw gcnew Exception(L"ffmpeg: could not allocate temporary picture");
            //fprintf(stderr, "Could not allocate temporary picture\n");
            //exit(1);
        }
    }

    /* copy data and linesize picture pointers to frame */
	//cli::pin_ptr<AVPicture *> p_dst_picture=&dst_picture;
    //*((AVPicture *)frame) = p_dst_picture;
}



void  FFmpegWrapper::FFmpegMedia::AVFormatCloseOutput(){
	avio_close( out_format_context->pb );
}
void  FFmpegWrapper::FFmpegMedia::CodecInit(){

	av_register_all();
	avcodec_register_all();
	//video_codec = avcodec_find_decoder(AV_CODEC_ID_H264);

	video_codec = avcodec_find_decoder(AV_CODEC_ID_MPEG4);

	video_codec_context = avcodec_alloc_context();

	//video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;
	//video_codec_context->codec_id = AV_CODEC_ID_H264;
	//video_codec_context->pix_fmt = PIX_FMT_YUV420P;
	//video_codec_context->width=1280;
	//video_codec_context->height=960;

	video_codec_context->codec_type = AVMEDIA_TYPE_VIDEO;
	video_codec_context->codec_id =AV_CODEC_ID_MPEG4;
	video_codec_context->pix_fmt = PIX_FMT_YUV420P;
	video_codec_context->width=640;
	video_codec_context->height=480;


	//video_codec_context->skip_frame         = AVDISCARD_DEFAULT;
	//video_codec_context->error_concealment   = 3;
	//video_codec_context->error_recognition   = 1;
	//video_codec_context->skip_loop_filter      = AVDISCARD_DEFAULT;
	//video_codec_context->workaround_bugs      = 1;
	//video_codec_context->pix_fmt =PIX_FMT_BGR24;
	//video_codec_context->sample_fmt = SAMPLE_FMT_NONE;
	//video_codec_context->skip_loop_filter = AVDISCARD_DEFAULT;
	//video_codec_context->skip_idct = AVDISCARD_DEFAULT;
	//video_codec_context->skip_frame = AVDISCARD_DEFAULT;
	//video_codec_context->color_primaries = AVCOL_PRI_UNSPECIFIED;
	//video_codec_context->color_trc = AVCOL_PRI_UNSPECIFIED;
	//video_codec_context->colorspace = AVCOL_PRI_UNSPECIFIED;
	//video_codec_context->color_range = AVCOL_RANGE_UNSPECIFIED;
	//video_codec_context->chroma_sample_location = AVCHROMA_LOC_LEFT;
	//video_codec_context->lpc_type = AV_LPC_TYPE_DEFAULT;


	int video_result = avcodec_open2(video_codec_context, video_codec, NULL);

	if (video_result < 0) 
		throw gcnew Exception(L"ffmpeg: Unable to open video codec");

}
void FFmpegWrapper::FFmpegMedia::InitVideoFrame(){

	frame = avcodec_alloc_frame();
	frame_rgb = avcodec_alloc_frame();

	uint8_t *buffer;
	int num_bytes = avpicture_get_size(PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);
	buffer=(uint8_t *)av_malloc(num_bytes*sizeof(uint8_t));
	avpicture_fill((AVPicture *)frame_rgb, buffer, PIX_FMT_RGB24, video_codec_context->width, video_codec_context->height);

	//av_free(buffer);
}
void FFmpegWrapper::FFmpegMedia::DeinitVideoFrame(){		

	av_free(frame);
	av_free(frame_rgb);
}




//void  FFmpegWrapper::FFmpegMedia::GetDecodeVideoFrame(IntPtr data,  int size, int type){
//
//	AVPacket packet;
//
//	packet.data=reinterpret_cast<uint8_t*>(data.ToPointer());
//	packet.size=size;
//
//	//av_new_packet(&packet, size);
//	//memcpy(packet.data, reinterpret_cast<uint8_t*>(data.ToPointer()), size);
//
//	packet.pts=AV_NOPTS_VALUE;
//	packet.dts=AV_NOPTS_VALUE;
//
//	packet.stream_index=0;
//	if(type==5)
//		packet.flags=1;
//	else
//		packet.flags=0;
//
//	AVFrame *frame = avcodec_alloc_frame();
//	AVFrame *frame_rgb = avcodec_alloc_frame();
//
//	int width=video_codec_context->width;
//	int height=video_codec_context->height;
//
//	uint8_t *buffer;
//	int num_bytes = avpicture_get_size(PIX_FMT_RGB24, width, height);
//	buffer=(uint8_t *)av_malloc(num_bytes*sizeof(uint8_t));
//	avpicture_fill((AVPicture *)frame_rgb, buffer, PIX_FMT_RGB24, width, height);
//
//	//if (packet.stream_index == video_stream) {
//	while (packet.size > 0) {
//		int got_video_frame;
//		int result =avcodec_decode_video2(video_codec_context, frame, &got_video_frame, &packet);
//
//		if(result<0){
//			break;
//			//throw gcnew Exception(L"Error while decoding");			
//		}
//
//		if (got_video_frame>0) 
//		{
//			sws_scale(img_convert_context, frame->data, frame->linesize, 0, frame->height, frame_rgb->data, frame_rgb->linesize);
//
//			VideoFrameReceived((IntPtr)(frame_rgb->data[0]), frame_rgb->linesize[0], width, height);
//		}
//		//}
//		packet.size-=result;
//		packet.data+=result;
//	}
//	//av_free(buffer);
//	//av_free_packet(&packet);
//
//}






void FFmpegWrapper::FFmpegMedia::GetDecodeVideoFrame(IntPtr data,  int size, bool key, unsigned long pts)
{
	AVPacket packet;
	av_new_packet(&packet, size);
	memcpy(packet.data, reinterpret_cast<uint8_t*>(data.ToPointer()), size);
	packet.pts=pts;
	packet.dts=pts;
	//packet.pts=AV_NOPTS_VALUE;
	//packet.dts=AV_NOPTS_VALUE;

	if(key==true)
		packet.flags=1;
	else
		packet.flags=0;

	int got_video_frame;
	int result =avcodec_decode_video2(video_codec_context, frame, &got_video_frame, &packet);

	if(result>0) {
		/*throw gcnew Exception(L"Error while decoding");*/
		if (got_video_frame>0) {

			sws_scale(img_convert_context, frame->data, frame->linesize, 0, frame->height, frame_rgb->data, frame_rgb->linesize);

			VideoFrameReceived((IntPtr)(frame_rgb->data[0]), frame_rgb->linesize[0], video_codec_context->width, video_codec_context->height);
			//VideoFrameReceived((IntPtr)(frame->data[0]), frame->linesize[0], video_codec_context->width, video_codec_context->height);

			av_interleaved_write_frame(out_format_context, &packet);

		}
	}
	av_free_packet(&packet);
}




void FFmpegWrapper::FFmpegMedia::AVCodecClose(){
	avcodec_close(video_codec_context);
}

void FFmpegWrapper::FFmpegMedia::SwsFreeContext(){
	sws_freeContext(img_convert_context);
}

void FFmpegWrapper::FFmpegMedia::AVFormatNetworkDeinit(){
	avformat_network_deinit();
}
void FFmpegWrapper::FFmpegMedia::AVFormatCloseInput(){

	cli::pin_ptr<AVFormatContext*> p=&format_context;
	avformat_close_input(p);

}
void FFmpegWrapper::FFmpegMedia::SDLQuit(){
	SDL_Quit();
}


void FFmpegWrapper::FFmpegMedia::Close(){
	exit=true;
}

//avformat_network_deinit();
//avformat_close_input(&format_context);
//SDL_Quit();
//if(!String::IsNullOrEmpty(mediaSource))
//	return video_start((char*) ((Marshal::StringToHGlobalAnsi (mediaSource)).ToPointer ()));			
//else
//	throw gcnew Exception(L"Unable to Init Media Source");

//struct SwsContext* img_convert_context;
//int  video_start(char* input_source)
//{
//	int err;
//
//	err = SDL_Init(SDL_INIT_VIDEO);
//	if (err < 0) {
//		fprintf(stderr, "Unable to init SDL: %s\n", SDL_GetError());
//		return -1;
//	}
//
//	av_register_all();
//	avformat_network_init();
//
//
//	AVFormatContext* format_context = NULL;
//
//	err = avformat_open_input(&format_context, input_source, NULL, NULL);
//
//	if (err < 0) {
//		fprintf(stderr, "ffmpeg: Unable to open input file\n");
//
//		return -1;
//	}
//
//
//	err = avformat_find_stream_info(format_context, NULL);
//	if (err < 0) {
//		fprintf(stderr, "ffmpeg: Unable to find stream info\n");
//		return -1;
//	}
//
//	int video_stream;
//	for (video_stream = 0; video_stream < format_context->nb_streams; ++video_stream) {
//		if (format_context->streams[video_stream]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {
//			break;
//		}
//	}
//
//	if (video_stream == format_context->nb_streams) {
//		fprintf(stderr, "ffmpeg: Unable to find video stream\n");
//		//return -1;
//	}
//
//	AVCodecContext* codec_context = format_context->streams[video_stream]->codec;
//	AVCodec* codec = avcodec_find_decoder(codec_context->codec_id);
//	err = avcodec_open2(codec_context, codec, NULL);
//
//	if (err < 0) {
//		fprintf(stderr, "ffmpeg: Unable to open codec\n");
//		return -1;
//	}
//
//	SDL_Surface* screen = SDL_SetVideoMode(codec_context->width, codec_context->height, 0, 0);
//	if (screen == NULL) {
//		fprintf(stderr, "Couldn't set video mode\n");
//		return -1;
//	}
//
//	SDL_Overlay* bmp = SDL_CreateYUVOverlay(codec_context->width, codec_context->height,
//		SDL_YV12_OVERLAY, screen);
//
//	struct SwsContext* img_convert_context;
//	img_convert_context = sws_getCachedContext(NULL,
//		codec_context->width, codec_context->height,
//		codec_context->pix_fmt,
//		codec_context->width, codec_context->height,
//		PIX_FMT_YUV420P, SWS_BICUBIC,
//		NULL, NULL, NULL);
//	if (img_convert_context == NULL) {
//		fprintf(stderr, "Cannot initialize the conversion context\n");
//		return -1;
//	}
//
//
//	AVFrame* frame = avcodec_alloc_frame();
//	AVPacket packet;
//
//
//	while (av_read_frame(format_context, &packet) >= 0) {
//		if (packet.stream_index == video_stream) {
//			// Video stream packet
//			int frame_finished;
//			int result =avcodec_decode_video2(codec_context, frame, &frame_finished, &packet);
//			if(result<0){
//				fprintf(stderr, "Error while decoding\n");
//				exit(1);
//			}
//			if (frame_finished) {
//				SDL_LockYUVOverlay(bmp);
//
//				AVPicture pict;
//				pict.data[0] = bmp->pixels[0];
//				pict.data[1] = bmp->pixels[2];  // it's because YV12
//				pict.data[2] = bmp->pixels[1];
//
//				pict.linesize[0] = bmp->pitches[0];
//				pict.linesize[1] = bmp->pitches[2];
//				pict.linesize[2] = bmp->pitches[1];
//
//				sws_scale(img_convert_context,frame->data, frame->linesize,0,
//					codec_context->height,pict.data, pict.linesize);
//
//				SDL_UnlockYUVOverlay(bmp);
//				SDL_Rect rect;
//				rect.x = 0;
//				rect.y = 0;
//				rect.w = codec_context->width;
//				rect.h = codec_context->height;
//				SDL_DisplayYUVOverlay(bmp, &rect);
//			}
//		}
//		av_free_packet(&packet);
//
//		SDL_Event event;
//		if (SDL_PollEvent(&event)) {
//			if (event.type == SDL_QUIT) {
//				break;
//			}
//		}
//	}
//
//	sws_freeContext(img_convert_context);
//
//	//av_free(frame);
//
//	avcodec_close(codec_context);
//
//	avformat_network_deinit();
//	avformat_close_input(&format_context);
//	SDL_Quit();
//
//	return 0;
//}
//int  CLRWrapper::FFmpegVideo::ShowVideo(String^ inputSource){
//	//String^ inputSource = "rtsp://192.168.10.203/ONVIF/MediaInput?profile=1_def_profile6";	
//	//Console::ReadKey();
//	return video_start((char*) ((Marshal::StringToHGlobalAnsi (inputSource)).ToPointer ()));
//}


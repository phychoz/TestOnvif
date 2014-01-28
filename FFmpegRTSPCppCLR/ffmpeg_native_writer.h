
namespace FFmpegWrapper 
{
	public class ffmpeg_native_writer 
	{
	public:
		void open(char* file, char* codec, int width, int height);
		void close();

		void write_audio_data_to_file(uint8_t* data, int size, unsigned long time, int duration);
		void write_video_data_to_file(uint8_t* data, int size, unsigned long time, int duration);

	private:

		char* filename;

		char* codec;
		int width;
		int height;

		AVFrame *video_frame;
		AVFrame *audio_frame;

		uint8_t *enc_video_buffer;
		uint8_t *enc_audio_buffer;

		//AVDictionary * dictionary;

		AVFormatContext* format_context;

		AVCodec* video_codec;
		AVCodec* audio_codec;

		AVCodecContext* video_codec_context;	
		AVCodecContext* audio_codec_context;	

		AVStream* audio_stream;
		AVStream* video_stream;

		struct SwsContext* yuv_convert_context;

		void init_file();
		void init_video_codec(char* codec, int width, int height);
		void init_audio_codec();
		void alloc_frames();
		void open_file();
	};
}
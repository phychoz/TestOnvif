namespace FFmpegWrapper
{
	const int VIDEO_CLOCKRATE=90000;
	const int AUDIO_CLOCKRATE=8000;

	struct session_counter
	{
		unsigned long recieved_frame_count;
		unsigned long curr_timestamp;
		unsigned long last_timestamp;
		unsigned long time_interval;
	};

	typedef void (__stdcall *VIDEO_FRAME_DECODED)(uint8_t* , int, int, int, unsigned long, unsigned long, int);
	typedef void (__stdcall *AUDIO_FRAME_DECODED)(uint8_t* , int, unsigned long);

	public class ffmpeg_native_reader 
	{
	public:

		void open(char* c, int w, int h);
		void close();

		void video_data_decoding(uint8_t* data, int size, bool key, unsigned long pts);
		void audio_data_decoding(uint8_t* data, int size, bool key, unsigned long pts);

		void register_video_handler(VIDEO_FRAME_DECODED callback);
		void register_audio_handler(AUDIO_FRAME_DECODED callback);

	private:

		char* codec;
		int width;
		int height;

		AVFrame *audio_frame;
		AVFrame *video_frame;
		AVFrame *rgb_frame;

		uint8_t *rgb_buffer;

		AVFormatContext* format_context;

		AVCodec* video_codec;
		AVCodec* audio_codec;

		AVCodecContext* video_codec_context;	
		AVCodecContext* audio_codec_context;	

		struct SwsContext* rgb_convert_context;

		session_counter* video_counter;
		session_counter* audio_counter;

		void init_video_codec();
		void init_audio_codec();
		void alloc_frames();

		VIDEO_FRAME_DECODED video_frame_decoded;
		AUDIO_FRAME_DECODED audio_frame_decoded;

	};
}

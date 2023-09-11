using System;
namespace text_to_speechStudio.Models
{
	public class TextToSpeechModel
	{
		public string text
		{
			get; set;
		}
		public string voiceName
		{
			get; set;
		}
        public string pitch
        {
            get; set;
        }
        public string speed
        {
            get; set;
        }
    }

	public class TextToSpeechResponse
	{
        public string audioContent 
        {
            get; set;
        }
    }
}


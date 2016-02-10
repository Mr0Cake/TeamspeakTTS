using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Threading;
using System.Text.RegularExpressions;

namespace TeamspeakReader
{
    
    public static class ChatReader
    {
        public static bool Reading = false;
        
        

        private static Queue<string> _TalkMessages;

        public static Queue<string> TalkMessages
        {
            get { return _TalkMessages; }
            set { _TalkMessages = value; }
        }

        private static void SanitizeString(string text)
        {
            
            //text = text.Replace("[url]", "").Replace("[/url]", "");
            text = Regex.Replace(text, @"\[url\b[^\]]+\]([^\[]*(?:(?!\[/url)\[[^\[]*)*)\[/url\]", " Link ");

            if (TalkMessages == null)
                TalkMessages = new Queue<string>();
            TalkMessages.Enqueue(text);
            if (!Reading)
            {
                Task.Run(() => startReading());
            }
        }

        public static void AddTextToQueue(string text, string user)
        {
            if(!text.IsNullOrTrimmedEmpty())
                Task.Run(() => SanitizeString(text));
            
        }

        

        public static void startReading()
        {
            Reading = true;
            if(TalkMessages!= null)
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                //synth.SetOutputToAudioStream()
                synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult, 22,
                    new System.Globalization.CultureInfo("nl-BE"));
                while (TalkMessages.Count > 0)
                {
                    string text = TalkMessages.Dequeue();
                    synth.Speak(text);
                    Thread.Sleep(2000);
                }
            }
            Reading = false;
        }
    }
}

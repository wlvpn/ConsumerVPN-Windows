using System;
using System.Collections.Generic;
using System.Linq;
using SpeechLib;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using Microsoft.Win32;

namespace WLVPN.Utils
{
    public class SAPIWrapper
    {
        private ISpeechVoice _voice;
        private string culture = "en-EN";

        public SAPIWrapper()
        {
            if (Registry.ClassesRoot.OpenSubKey(@"CLSID\{96749377-3391-11D2-9EE3-00C04F797396}", false) == null)
            {
                SapiAvailable = false;
                return;
            }

            try
            {
                _voice = new SpVoice();
                culture = Thread.CurrentThread.CurrentUICulture.TextInfo.CultureName;
            }
            catch
            {
                SapiAvailable = false;
            }
        }

        public bool SapiAvailable { get; set; } = true;

        public void Speak(string txt, SpeechVoiceSpeakFlags flags = SpeechVoiceSpeakFlags.SVSFDefault)
        {
            if (Properties.Settings.Default.EnableSpeech && SapiAvailable)
            {
                Task.Run(() =>
                {
                    _voice?.Speak("<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='" + culture + "'>"
                                + txt + "</speak>", SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFIsXML);
                });
            }
        }
    }
}

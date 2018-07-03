using System;
using System.Diagnostics;
using System.IO;

namespace discord_raadio_elmar_bot
{
    class StreamReceiver : IDisposable
    {
        Process ffmpeg;
        ProcessStartInfo info;
        string radio;
        bool active;
                
        public Stream AudioStream { get { return ffmpeg.StandardOutput.BaseStream; } }
        public Process FFMPEG { get { return ffmpeg; } }
        public string RadioURL { get { return radio; } }
        public bool Active { get { return active; } }

        //creates the object basically.
        public StreamReceiver(string radio_url)
        {
            radio = radio_url;
            info = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = "-i \"" + radio + "\" -fflags nobuffer -ac 2 -f s16le -ar 48000 pipe:1", //-t 00:01 -ac
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            active = false;                
        }

        //public method to start receiving data again.
        public void Start()
        {
            if (!active)
            {                
                ffmpeg = Process.Start(info);
            }
            active = true;
        }

        //public method to stop receiving stream, useful to conserve data.
        public void Stop()
        {
            if (active) DeleteStreamer();
            active = false;
        }        
        
        void DeleteStreamer()
        {
            if (ffmpeg != null)
            {
                ffmpeg.Kill();
                ffmpeg.Dispose();
            }
        }

        public void Dispose()
        {
            DeleteStreamer();
        }
    }
}

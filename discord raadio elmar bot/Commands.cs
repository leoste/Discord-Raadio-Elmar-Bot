using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace discord_raadio_elmar_bot
{
    class RadioCommands
    {
        StreamReceiver elmar;
        List<VoiceNextConnection> vncs = new List<VoiceNextConnection>();

        void InitElmar()
        {
            //create stream receiving process, but don't start it yet.
            elmar = new StreamReceiver(Const.radio);
        }

        bool sendingdatas = false;
        bool allowmodify = true;        
        
        public async Task SendDatas(DiscordClient client)
        {            
            sendingdatas = true;
            elmar.Start();
            var ffout = elmar.FFMPEG.StandardOutput.BaseStream;

            var buff = new byte[3840];
            var br = 0;
            while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
            {
                if (br < buff.Length) // not a full sample, mute the rest
                    for (var i = br; i < buff.Length; i++)
                        buff[i] = 0;

                if (vncs.Count == 0) break;

                allowmodify = false;                
                List<Task> sends = new List<Task>();
                for (int i = 0; i < vncs.Count; i++)
                {
                    if (vncs[i].Channel == null)
                    {                        
                        vncs.Remove(vncs[i]);
                        i--;
                    }
                    else
                    {                     
                        sends.Add(vncs[i].SendAsync(buff, 20));
                    }
                }
                await Task.WhenAll(sends);
                allowmodify = true;
            }            
            elmar.Stop();
            sendingdatas = false;
        }

        [Command("mängi")]
        public async Task Play(CommandContext ctx)
        {
            if (elmar == null) InitElmar();

            //try to connect if not connected yet.
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Juba mängin poju.");
                throw new InvalidOperationException("Already connected in this guild.");
            }

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("Sea end enne istuma.");
                throw new InvalidOperationException("You need to be in a voice channel.");
            }

            vnc = await vnext.ConnectAsync(chn);

            //if bot wasn't connected before code will have arrived here, so we need to activate stream for it.
            await ctx.RespondAsync("Mängin.");

            //add vnc to the list of vncs that should receive data.
            SpinWait.SpinUntil(() => allowmodify);
            lock (vncs)
            {
                vncs.Add(vnc);
            }
            await vnc.SendSpeakingAsync(true);

            //we just added new vnc, if we aren't sending data we need to start doing it again.
            if (!sendingdatas) SendDatas(ctx.Client);

            //since it's a radio stream the bot will probably never reach here.
            await vnc.SendSpeakingAsync(false);
            throw new FieldAccessException("lel");
        }

        [Command("mine")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Ammu juba läinud.");
                throw new InvalidOperationException("Not connected in this guild.");
            }
            
            //this vnc shouldn't receive data anymore, so remove it.
            SpinWait.SpinUntil(() => allowmodify);            
            lock (vncs)
            {
                vncs.Remove(vnc);
            }

            await ctx.RespondAsync("proovin minna.");
            SpinWait.SpinUntil(() => vnc.IsPlaying == false);
            
            await vnc.SendSpeakingAsync(false);
            vnc.Disconnect();

            /*bool dontgo = true;
            do
            {
                try
                {
                    await ctx.RespondAsync("proovin minna.");
                    SpinWait.SpinUntil(() => vnc.IsPlaying == false);
                    await vnc.SendSpeakingAsync(false);
                    vnc.Disconnect();                    
                    dontgo = false;
                } catch { await ctx.RespondAsync("ei saanud minna."); }
            } while (dontgo);*/

            await ctx.RespondAsync("Lähen.");
        }
    }
}

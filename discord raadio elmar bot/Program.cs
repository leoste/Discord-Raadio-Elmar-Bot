using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using System.Diagnostics;

namespace discord_raadio_elmar_bot
{
    class Program
    {
        //those are set when program starts but don't really change afterwards.
        static DiscordClient client;
        static VoiceNextClient voice;
        static CommandsNextModule commands;

        //entry point. just redirects to async main method.
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        //actual main LOL pranked.
        static async Task MainAsync(string[] args)
        {
            //create discord client.
            client = new DiscordClient(new DiscordConfiguration
            {
                Token = Const.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });            

            //create voice client.
            voice = client.UseVoiceNext();            

            //attach commands
            commands = client.UseCommandsNext(new CommandsNextConfiguration { StringPrefix = "elmar " });
            commands.RegisterCommands<RadioCommands>();
            
            //log onto discord.
            await client.ConnectAsync();

            //never close, never choose to.
            await Task.Delay(-1);
        }
    }
}

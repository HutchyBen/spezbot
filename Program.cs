using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;

namespace Music
{



    class Bot
    {
        public static DiscordChannel? GetUserVC(DiscordMember member)
        {
            var state = member.VoiceState;
            if (state == null)
            {
                return null;
            }
            var chan = state.Channel;
            if (chan == null)
            {
                return null;
            }
            return chan;
        }
        public static ServiceProvider service = new ServiceCollection()
                .AddSingleton<Dictionary<ulong, ServerInstance>>()
                .BuildServiceProvider();
        public static DiscordClient? Discord;
        public static LavalinkExtension? Lavalink;
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        }


        static async Task MainAsync(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("dtoken");
            Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,

                MinimumLogLevel = LogLevel.Information
            });
            service = new ServiceCollection()
                .AddSingleton<Dictionary<ulong, ServerInstance>>()
                .BuildServiceProvider();
            var commands = Discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                StringPrefixes = new[] { "spez!" },
                EnableMentionPrefix = true,
                Services = service
            });
            commands.RegisterCommands<TestCommands>();
            commands.RegisterCommands<MusicCommands.MusicCommands>();
            Lavalink = Discord.UseLavalink();
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            Discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1),
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore
            });


            await Discord.ConnectAsync();
            await Lavalink.ConnectAsync(new LavalinkConfiguration()
            {
                Password = "hutchypass", // not an actual password i use just like ya know hutchyben password. 
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
            });

            Discord.VoiceStateUpdated += VoiceStateChange;

            await Task.Delay(-1);
        }

        static async Task VoiceStateChange(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            var Servers = service.GetRequiredService<Dictionary<ulong, ServerInstance>>();
            ServerInstance? inst;
            if (Servers.TryGetValue(e.Guild.Id, out inst))
            {
                if (inst == null)
                {
                    return;
                }
                if (e.Before == null)
                {
                    return;
                }
                if (e.After == null)
                {
                    return;
                }
                if (e.After.Channel == null)
                {
                    return;
                }
                if (e.Before.Channel == null)
                {
                    return;
                }
                if (e.Before.Channel.Id == e.After.Channel.Id)
                {
                    return;
                }
                if (e.Before.Channel.Id == inst.channel.Id)
                {

                    await Discord!.SendMessageAsync(inst.msgChannel, "Moved channel");
                    inst.channel = e.After.Channel;
                }

            };
            return;
        }
    }
}

﻿using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;
using SpotifyAPI.Web;

namespace Music
{
    class Bot
    {
        static public ProfanityFilter.ProfanityFilter filter = new ProfanityFilter.ProfanityFilter();

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
            // load bad words from text file
            filter.AddProfanity("flip");
            filter.AddProfanity("frick");
            filter.AddProfanity("fricking");

            var token = Environment.GetEnvironmentVariable("dtoken");
            var spotID = Environment.GetEnvironmentVariable("spotID");
            var spotSecret = Environment.GetEnvironmentVariable("spotSecret");
            Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,

                MinimumLogLevel = LogLevel.Information
            });

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new ClientCredentialsAuthenticator(spotID, spotSecret));

            var spotify = new SpotifyClient(config);


            service = new ServiceCollection()
                .AddSingleton<Dictionary<ulong, ServerInstance>>()
                .AddSingleton<SpotifyClient>(spotify)
                .BuildServiceProvider();
            var commands = Discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                StringPrefixes = new[] { "spez!" },
                EnableMentionPrefix = true,
                Services = service
            });
            commands.RegisterCommands<TestCommands>();
            commands.RegisterCommands<Commands.MusicCommands>();
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
            Discord.MessageCreated += MessageCreate;
            await Task.Delay(-1);
        }

        static async Task MessageCreate(DiscordClient client, MessageCreateEventArgs e)
        {
            // check for swear words
            if (e.Author.IsBot)
            {
                return;
            }
            if (filter.ContainsProfanity(e.Message.Content))
            {
                if (e.Guild.Id == 802660679856160818) 
                    await e.Message.RespondAsync("Please do not use bad words in this server.");
                    
            }
        }

        static async Task VoiceStateChange(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            
            var Servers = service.GetRequiredService<Dictionary<ulong, ServerInstance>>();
            ServerInstance? inst;
            if (Servers.TryGetValue(e.Guild.Id, out inst))
            {
                
                if (e.After.Channel == null && e.User.Id == client.CurrentUser.Id)
                {
                    Servers.Remove(e.Guild.Id);
                    return;
                }

                if (e.After.Channel.Id != inst.channel.Id && e.User.Id == client.CurrentUser.Id )
                {
                    inst.channel = e.After.Channel;
                }

            };
            return;
        }
    }
}

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
using SpotifyAPI.Web;

namespace Music
{
    class Bot
    {
        static Random rand;
        static Dictionary<string, Markov> Markovs;
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
        static void LoadModels()
        {
            Directory.GetFiles("models").ToList().ForEach(file =>
            {
                var id = file.Split('\\').Last().Split('/').Last().Split('.').First();
                var mk = new Markov(id);
                Markovs.Add(id, mk);
            });
        }

        static async Task MainAsync(string[] args)
        {
            rand = new Random();
            Markovs = new Dictionary<string, Markov>();
            LoadModels();
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
                .AddSingleton<Dictionary<string, Markov>>(Markovs)
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
            Discord.GuildMemberAdded += GuildMemberAdd;
            Discord.GuildMemberRemoved += GuildMemberRemove;
            await Task.Delay(-1);
        }

        static async Task GuildMemberAdd(DiscordClient client, GuildMemberAddEventArgs e)
        {
            if (e.Guild.Id == 802660679856160818)
            {
                var mCount = 0;
                foreach (var member in e.Guild.Members)
                {
                    if (!member.Value.IsBot)
                    {
                        mCount++;
                    }
                }
                var embed = new DiscordEmbedBuilder();
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.WithDescription("has joined the server");
                embed.WithColor(DiscordColor.Green);
                embed.AddField("Member Count", mCount.ToString());
                await client.SendMessageAsync(await client.GetChannelAsync(802963819881824256), embed);
            }
        }

        static async Task GuildMemberRemove(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild.Id == 802660679856160818)
            {
                var mCount = 0;
                foreach (var member in e.Guild.Members)
                {
                    if (!member.Value.IsBot)
                    {
                        mCount++;
                    }
                }
                var embed = new DiscordEmbedBuilder();
                embed.WithAuthor($"{e.Member.Username}#{e.Member.Discriminator}", null, e.Member.AvatarUrl);
                embed.WithDescription("has left the server");
                embed.WithColor(DiscordColor.Red);
                embed.AddField("Member Count", mCount.ToString());
                await client.SendMessageAsync(await client.GetChannelAsync(802963819881824256), embed);
            }
        }
        static async Task MessageCreate(DiscordClient client, MessageCreateEventArgs e)
        {
            // what the personal channel
            if (e.Message.ChannelId == 802963819881824256)
            {
                return;
            }
            if (e.Message.Author.Id == Discord.CurrentUser.Id)
            {
                return;
            }

            Markov? mk;
            if (!Markovs.ContainsKey(e.Channel.Guild.Id.ToString()))
                Markovs.Add(e.Channel.Guild.Id.ToString(), new Markov(e.Channel.Guild.Id.ToString()));

            mk = Markovs[e.Channel.Guild.Id.ToString()];
            if (!e.Message.Content.StartsWith("spez!"))
                if (!e.Message.Author.IsBot || e.Message.Author.Id == 974297735559806986 || e.Message.Author.Id == 247283454440374274)
                {
                    if (e.Message.Content.Trim() != "")
                        mk.Add(e.Message.Content.Trim());

                    foreach (var attachment in e.Message.Attachments)
                    {
                        mk.Add(attachment.Url);
                    }
                }
            if (e.Message.Author.Id == 974297735559806986)
            {
                var cnext = client.GetCommandsNext();
                var msg = e.Message;

                var cmdStart = msg.GetStringPrefixLength("spez!");
                if (cmdStart != -1)
                {
                    var prefix = msg.Content.Substring(0, cmdStart);
                    var cmdString = msg.Content.Substring(cmdStart);

                    var command = cnext.FindCommand(cmdString, out var args);
                    if (command != null)
                    {
                        var ctx = cnext.CreateContext(msg, prefix, command, args);
                        Task.Run(async () => await cnext.ExecuteCommandAsync(ctx));
                    }
                }
            }

            if (e.Message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id) || rand.Next(10) <= 1 || e.Message.Content.Contains("spez"))
            {
                var content = mk.Generate();
                if (content.Trim() != "")
                {
                    await e.Message.RespondAsync(content);
                }

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
                if (e.After.Channel == null)
                    return;
                if (e.After.Channel.Id != inst.channel.Id && e.User.Id == client.CurrentUser.Id)
                {
                    inst.channel = e.After.Channel;
                }

            };
            return;
        }
    }
}

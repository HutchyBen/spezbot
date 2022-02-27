using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.Lavalink;
namespace Music.MusicCommands
{
    public partial class MusicCommands : BaseCommandModule
    {
        private async Task<LavalinkLoadResult?> StartPlay(CommandContext ctx, Uri search)
        {
            await Join(ctx);

            var inst = Servers[ctx.Guild.Id];
            if (search == null)
            {
                if (inst.connection.CurrentState.CurrentTrack != null)
                {
                    await ctx.RespondAsync("Resuming...");
                    await inst.connection.ResumeAsync();
                    ctx.Client.Logger.Log(LogLevel.Information, "Resumed");
                    return null;
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = ":warning: You didn't provide a search term",
                        Description = "Try again.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return null;
                }
            }

            var tracks = await inst.connection.Node.Rest.GetTracksAsync(search);

            if (tracks.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                var err = new DiscordEmbedBuilder
                {
                    Title = "::no_entry_sign: Failed to load track",
                    Description = $"{tracks.Exception.Message}",
                    Color = DiscordColor.Red
                };
                await ctx.Channel.SendMessageAsync(embed: err);
                ctx.Client.Logger.Log(LogLevel.Information, "Failed to load track");
                return null;
            }
            return tracks;
        }
        private async Task<LavalinkLoadResult?> StartPlay(CommandContext ctx, string search)
        {
            await Join(ctx);

            var inst = Servers[ctx.Guild.Id];
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel.Id != inst.channel.Id) {
                    var embed = new DiscordEmbedBuilder {
                        Title = ":warning: You are not in the same voice channel as the bot",
                        Description = "Join the voice channel of the bot to skip songs.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return null;
            }
            if (search == null)
            {
                if (inst.connection.CurrentState.CurrentTrack != null)
                {
                    await ctx.RespondAsync("Resuming...");
                    await inst.connection.ResumeAsync();
                    ctx.Client.Logger.Log(LogLevel.Information, "Resumed");
                    return null;
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = ":warning: You didn't provide a search term",
                        Description = "Try again.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return null;
                }
            }

            var tracks = await inst.connection.Node.Rest.GetTracksAsync(search);

            if (tracks.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                var err = new DiscordEmbedBuilder
                {
                    Title = "::no_entry_sign: Failed to load track",
                    Description = $"{tracks.Exception.Message}",
                    Color = DiscordColor.Red
                };
                await ctx.Channel.SendMessageAsync(embed: err);
                ctx.Client.Logger.Log(LogLevel.Information, "Failed to load track");
                return null;
            }
            return tracks;
        }


        public Dictionary<ulong, ServerInstance> Servers { get; set; }
        [Command, Priority(0)]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {

            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddSong(tracks, ctx.Member);

        }

        [Command, Priority(1)]
        public async Task Play(CommandContext ctx, Uri search)
        {
            await Join(ctx);
            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddSong(tracks, ctx.Member);

        }

        [Command("playnext"), Priority(0)]
        public async Task PlayNext(CommandContext ctx, [RemainingText] string search)
        {
            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(tracks, ctx.Member);

        }

        [Command("playnext"), Priority(1)]
        public async Task PlayNext(CommandContext ctx, Uri search)
        {
            await Join(ctx);
            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(tracks, ctx.Member);
        }

        [Command("PlayNow"), Priority(0)]
        public async Task PlayNow(CommandContext ctx, [RemainingText] string search)
        {
            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(tracks, ctx.Member, true);
        }

        [Command("PlayNow"), Priority(1)]
        public async Task PlayNow(CommandContext ctx, Uri search)
        {
            await Join(ctx);
            var tracks = await StartPlay(ctx, search);
            if (tracks == null)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(tracks, ctx.Member, true);
        }
    }
}

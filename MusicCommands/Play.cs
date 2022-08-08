using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.Lavalink;
namespace Music.Commands
{
    public struct SearchResult
    {
        public List<LavalinkTrack> Tracks { get; set; }
        public string PlayListName { get; set; }
    }
    public partial class MusicCommands : BaseCommandModule
    {
        // Makes sure the player is connected to the voice channel and if not, connects to it.
        // Then makes request to lavalink and returns results
        private async Task<SearchResult> StartPlay(CommandContext ctx, string search)
        {
            await Join(ctx);

            var inst = Servers[ctx.Guild.Id];
            if ((ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel.Id != inst.channel.Id) && ctx.Message.Author.Id != 974297735559806986)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = ":warning: You are not in the same voice channel as the bot",
                    Description = "Join the voice channel of the bot to add songs to the queue.",
                    Color = DiscordColor.Yellow
                };
                await ctx.RespondAsync(embed);
                return new SearchResult();
            }
            if (search == null)
            {
                if (inst.connection.CurrentState.CurrentTrack != null)
                {
                    await ctx.RespondAsync("Resuming...");
                    await inst.connection.ResumeAsync();
                    return new SearchResult();
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
                    return new SearchResult();
                }
            }
            SearchResult tracks;
            if (search.ToLower().Contains("open.spotify.com"))
            {
                Uri spotURI = new Uri(search);
                switch (spotURI.Segments[1])
                {
                    case "track/":
                        tracks = await GetSpotifyTrack(inst, spotURI.Segments[2]);

                        break;
                    case "playlist/":
                        tracks = await GetSpotifyPlaylist(inst, spotURI.Segments[2], ctx.Member);
                        break;
                    default:
                        tracks = new SearchResult();
                        break;
                }
            }
            else
            {
                Uri? uri = null;
                try
                {
                    uri = new Uri(search);
                }
                catch { }

                var lavaSearch = uri == null ? await inst.connection.Node.Rest.GetTracksAsync(search) : await inst.connection.Node.Rest.GetTracksAsync(uri);
                if (lavaSearch.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = ":warning: No results found",
                        Description = "Try again.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return new SearchResult();
                }
                else if (lavaSearch.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = ":no_entry_sign: Cannot load that song",
                        Description = "Try another version of the song.",
                        Color = DiscordColor.Red
                    };
                    await ctx.RespondAsync(embed);
                    return new SearchResult();
                }
                
                List<LavalinkTrack> ltrack;
                if (lavaSearch.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                {
                    ltrack = lavaSearch.Tracks.ToList();
                }
                else
                {
                    ltrack = new List<LavalinkTrack> { lavaSearch.Tracks.First() };
                }
                tracks = new SearchResult
                {
                    PlayListName = lavaSearch.PlaylistInfo.Name,
                    Tracks = ltrack
                };

            }

            return tracks;
        }


        public Dictionary<ulong, ServerInstance> Servers = new Dictionary<ulong, ServerInstance>();
        [Command, Aliases("p"), Priority(0)]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {

            var result = await StartPlay(ctx, search);
            if (result.Tracks.Count() == 0)
                return;
            var inst = Servers[ctx.Guild.Id];

            await inst.AddSong(result, ctx.Member!);

        }

        [Command("playnext"), Priority(0)]
        public async Task PlayNext(CommandContext ctx, [RemainingText] string search)
        {

            var result = await StartPlay(ctx, search);
            if (result.Tracks.Count() == 0)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(result, ctx.Member!);

        }

        [Command("PlayNow"), Priority(0)]
        public async Task PlayNow(CommandContext ctx, [RemainingText] string search)
        {
            var result = await StartPlay(ctx, search);
            if (result.Tracks.Count() == 0)
                return;
            var inst = Servers[ctx.Guild.Id];
            await inst.AddNext(result, ctx.Member!, skip: true);
        }
    }
}

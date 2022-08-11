using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System.Text;
using DSharpPlus.Interactivity.Extensions;
namespace Music.Commands
{
    public partial class MusicCommands : BaseCommandModule
    {
        string GetProgress(LavalinkTrack song, LavalinkGuildConnection connection) {
            var prog = connection.CurrentState.PlaybackPosition.TotalSeconds / song.Length.TotalSeconds;
            var emoPos = (int)Math.Round(11 * prog);

            var seconds = TimeSpan.FromSeconds(Math.Round(connection.CurrentState.PlaybackPosition.TotalSeconds));
            var str = new StringBuilder("▬▬▬▬▬▬▬▬▬▬▬");
            str.Remove(emoPos, 1);
            str.Insert(emoPos, "🔘");
            str.Append($"`{seconds:hh\\:mm\\:ss}/{song.Length:hh\\:mm\\:ss}`");
            
            return str.ToString();
        }

        [Command("nowplaying"),  Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null) {
                var song = inst.NowPlaying;
                var member = inst.NowPlaying.member;
                var embed = new DiscordEmbedBuilder {
                    Title = $"{song.track.Title}",
                    Author = new DiscordEmbedBuilder.EmbedAuthor {
                        Name = $"{member.Username}",
                        IconUrl = member.AvatarUrl
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter {
                        Text = $"Uploader: {song.track.Author}",
                    },
                    Description = GetProgress(song.track, inst.connection),
                    Color = DiscordColor.Green,
                    Url = song.track.Uri.ToString()
                };
                var msg = await ctx.RespondAsync(embed: embed);
                var res = msg.WaitForReactionAsync(ctx.Member, DiscordEmoji.FromUnicode("⏹️"), TimeSpan.FromMinutes(2));
                while (!res.IsCompleted)
                {
                    embed.Description = GetProgress(song.track, inst.connection);
                    await msg.ModifyAsync(embed: embed.Build());
                    await Task.Delay(2000);
                }
            }
        }
    }
}

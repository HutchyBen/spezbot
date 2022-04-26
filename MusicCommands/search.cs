using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
namespace Music.MusicCommands
{
    public partial class MusicCommands : BaseCommandModule
    {
        [Command("search")]
        public async Task Search(CommandContext ctx, [RemainingText] string search)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null) {
                var result = await StartPlay(ctx, search);
                var tracks = result?.Tracks.Take(5).ToArray();
                
                if (tracks == null || tracks.Length == 0)
                {
                    await ctx.RespondAsync("No results found.");
                    return;
                }

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Search Results",
                    Color = new DiscordColor(0x55FF00),
                    Description = "Enter track number to play."
                };
                for (int i = 0; i < tracks.Length; i++)
                {
                    embed.AddField($"{i + 1}: {tracks[i].Title}", tracks[i].Uri.ToString());
                }
                await ctx.RespondAsync(embed: embed);
                var resp = await ctx.Message.GetNextMessageAsync();
                if (resp.Result.Content == "cancel")
                {
                    await ctx.RespondAsync("Cancelled.");
                    return;
                }

                var num = int.Parse(resp.Result.Content);
                if (num > 0 && num <= tracks.Length)
                {
                    await inst.AddSong(tracks[num - 1], ctx.Member!);
                }
                else
                {
                    await ctx.RespondAsync("Invalid track number.");
                }
            }
        }
    }
}

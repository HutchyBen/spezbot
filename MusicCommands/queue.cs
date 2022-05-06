using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Music.Commands
{
    public partial class MusicCommands : BaseCommandModule
    {
        private async Task HandleReactions(CommandContext ctx, DiscordMessage msg, List<string> pages, int page) {
            
            var res = msg.WaitForReactionAsync(ctx.Member, TimeSpan.FromSeconds(30));
            if (res.Result.TimedOut) {
                return;
            }
            var react = res.Result.Result;
            if (react.Emoji.Name == "⏪") {
                if (page == 0) {
                    page = pages.Count - 1;
                } else {
                    page--;
                }
            } else if (react.Emoji.Name == "⏩") {
                if (page == pages.Count - 1) {
                    page = 0;
                } else {
                    page++;
                }
            } else if (react.Emoji.Name == "⏹") {
                return;
            }
            await msg.DeleteReactionAsync(react.Emoji, ctx.Member);
            await msg.ModifyAsync(embed: new DiscordEmbedBuilder
                {
                    Title = ":musical_note: Queue",
                    Color = DiscordColor.Yellow,
                    Description = pages[page]
                }.Build());
            await HandleReactions(ctx, msg, pages, page);
            
        }


        [Command("queue")]
        public async Task Queue(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null)
            {
                
                List<string> tings = new List<string>();
                var interact = ctx.Client.GetInteractivity();
                int index = inst.currentListIndex;
                var currItem = 1;
                List<UserSongList> clone = inst.userSongs.Select(x =>
                {
                    var list = new UserSongList(x.member);
                    list.queue = x.queue.ToList();
                    return list;
                }).ToList(); 
                while (true)
                {
                    if (clone.Count == 0)
                    {
                        break;
                    }
                    tings.Add($"{currItem}. {clone[index].queue[0].Title} `{clone[index].queue[0].Length.ToString()}` {clone[index].member.Mention}");
                    clone[index].queue.RemoveAt(0);
                    if (clone[index].queue.Count == 0)
                    {
                        clone.RemoveAt(index);
                    }
                    if (clone.Count == 0)
                    {
                        break;
                    }

                    if (index >= clone.Count-1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }

                    currItem++;
                    
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = ":musical_note: Queue",
                    Color = DiscordColor.Yellow
                };
                
                var pages = new List<string>();
                // put every 10 items of time combined into pages
                for (int i = 0; i < tings.Count; i += 10)
                {
                   var line = String.Join("\n", tings.Skip(i).Take(10));
                    pages.Add(line);
                }
                if (!pages.Any())
                {
                    var err = new DiscordEmbedBuilder
                    {
                        Title = "No tracks are queued",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(err);
                    return;
                }
                embed.Description = pages[0];
                var msg = await ctx.RespondAsync(embed: embed);
                await msg.CreateReactionAsync(DiscordEmoji.FromUnicode("⏪"));
                await msg.CreateReactionAsync(DiscordEmoji.FromUnicode("⏹"));
                await msg.CreateReactionAsync(DiscordEmoji.FromUnicode("⏩"));
                await HandleReactions(ctx, msg, pages, 0);
                
            }

        }
    }
}
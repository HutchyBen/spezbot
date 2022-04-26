using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Music.MusicCommands
{
    public partial class MusicCommands : BaseCommandModule
    {
        [Command("shuffle")]
        public async Task Shuffle(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null) {
                var queue = inst.userSongs.Find(x => x.member == ctx.Member);
                if (queue != null) {
                    queue.Shuffle();
                    await ctx.RespondAsync($"Shuffled {queue.member.Nickname}'s queue");
                }
            }
        }
    }
}

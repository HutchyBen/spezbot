using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

namespace Music.MusicCommands
{
    public partial class MusicCommands : BaseCommandModule
    {
        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null)
            {
                await inst.connection.PauseAsync();
            }
            
        }
    }
}

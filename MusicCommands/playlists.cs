using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Music.Commands
{
    public partial class MusicCommands : BaseCommandModule
    {
        [Command("james")]
        public async Task james(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            var songPlaying = inst.connection.CurrentState.CurrentTrack != null;
            if (songPlaying)
            {
                await Play(ctx, "https://www.youtube.com/playlist?list=PLvS4-OftztDoKXvc7fve7WepFSSBha3KX");
                await Shuffle(ctx);
            }
            else
            {
                await Play(ctx, "ksi erie");
                await Play(ctx, "https://www.youtube.com/playlist?list=PLvS4-OftztDoKXvc7fve7WepFSSBha3KX");
                await Shuffle(ctx);
                await Skip(ctx);
            }
        }

    }
}

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
                await Play(ctx, new Uri("https://www.youtube.com/playlist?list=PLlOLjpoe2WPOQVfy55X137m6InmChvfUm"));
                await Shuffle(ctx);
            }
            else
            {
                await Play(ctx, "ksi erie");
                await Play(ctx, new Uri("https://www.youtube.com/playlist?list=PLlOLjpoe2WPOQVfy55X137m6InmChvfUm"));
                await Shuffle(ctx);
                await Skip(ctx);
            }
        }

        [Command("ben")]
        public async Task ben(CommandContext ctx)
        {
            
            var inst = Servers[ctx.Guild.Id];
            var songPlaying = inst.connection.CurrentState.CurrentTrack != null;
            if (songPlaying)
            {
                await Play(ctx, new Uri("https://www.youtube.com/playlist?list=PLyQlRlihVp28epHKjN4kGjSM8AbIBoR1F"));
                await Shuffle(ctx);
            }
            else
            {
                await Play(ctx, "ksi erie");
                await Play(ctx, new Uri("https://www.youtube.com/playlist?list=PLyQlRlihVp28epHKjN4kGjSM8AbIBoR1F"));
                await Shuffle(ctx);
                await Skip(ctx);
            }
        }
    }
}

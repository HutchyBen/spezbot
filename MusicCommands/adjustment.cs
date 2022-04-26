using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using System.Reflection;
using DSharpPlus.Lavalink.Entities;
namespace Music.MusicCommands
{
    public partial class MusicCommands : BaseCommandModule
    {
        [Command("volume")]
        public async Task Volume(CommandContext ctx, int volume)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null)
            {
                if (volume < 0)
                {
                    volume = 0;
                }

                Type t = typeof(LavalinkNodeConnection);
                MethodInfo[] methods = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                var jsonstr = "{\"op\":\"volume\",\"volume\":" + volume.ToString() + ",\"guildId\":\"" + inst.connection.Guild.Id.ToString() + "\"}";
                methods.Where(m => m.Name == "WsSendAsync").First().Invoke(inst.connection.Node, new object[] { jsonstr });
                await ctx.RespondAsync($"Set volume to {volume}%");
            }
        }
        [Command("bigdirtystinkingbass")]
        public async Task TurnTheBassUp(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null)
            {
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(0, 1));
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(1, 1));
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(2, 1));
            }
        }
        [Command("smallcleanaromaticbass")]
        public async Task TurnTheBassDown(CommandContext ctx)
        {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null)
            {
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(0, 0));
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(1, 0));
                await inst.connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(2, 0));
            }
        }
    }
}

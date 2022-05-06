using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;


namespace Music.Commands
{
    public partial class MusicCommands : BaseCommandModule
    {

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            if (!Servers.ContainsKey(ctx.Guild.Id))
            {
                var chan = Music.Bot.GetUserVC(ctx.Member!);
                if (chan == null)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = ":warning: You are not in a voice channel",
                        Description = "Hop in a VC.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return;
                }

                Servers.Add(ctx.Guild.Id, new ServerInstance(chan, ctx.Client, ctx.Channel));
            }
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            ServerInstance? inst; 
            var exists = Servers.TryGetValue(ctx.Guild.Id, out inst);
            if (exists)
            {
                ctx.Client.Logger.Log(LogLevel.Information, "Left");
                await inst!.connection.DisconnectAsync();
                Servers.Remove(ctx.Guild.Id);
            }
        }
    }
}


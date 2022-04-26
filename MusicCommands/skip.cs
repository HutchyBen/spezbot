using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
namespace Music.MusicCommands {
    public partial class MusicCommands : BaseCommandModule {



        [Command("skip")]
        public async Task Skip(CommandContext ctx) {
            var inst = Servers[ctx.Guild.Id];
            if (inst != null) {
                // check if user is in bots voice channel

                if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel.Id != inst.channel.Id) {
                    var embed = new DiscordEmbedBuilder {
                        Title = ":warning: You are not in the same voice channel as the bot",
                        Description = "Join the voice channel of the bot to skip songs.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed);
                    return;
                }

                if (inst.NowPlaying.member != ctx.Member) {
                    var intr = ctx.Client.GetInteractivity();
                    var embed = new DiscordEmbedBuilder {
                        Title = ":warning: You are not the current Music Player",
                        Description = "Type skip to ruin your friendship by skipping.",
                        Color = DiscordColor.Yellow
                    };
                    await ctx.RespondAsync(embed: embed);
                    var poo = await ctx.Message.GetNextMessageAsync(m => m.Content.ToLower() == "skip");
                    if (!poo.TimedOut) {
                        await inst.Skip();
                    }
                    return;
                }
                await inst.Skip();
            }
            
        }
    }
}
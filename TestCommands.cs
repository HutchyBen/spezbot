using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
namespace Music
{
    public class TestCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task PingCommand(CommandContext ctx)
        {
            Console.WriteLine("ping");
            await ctx.RespondAsync("Pong...");
        }
        [Command("fuckeveryone")]
        public async Task FuckEveryone(CommandContext ctx, string name) {
            foreach (DiscordMember member in ctx.Guild.Members.Values) {
                await member.ModifyAsync(new Action<DSharpPlus.Net.Models.MemberEditModel>((model) => {
                    model.Nickname = name;
                }));
            }
        }
    }
}

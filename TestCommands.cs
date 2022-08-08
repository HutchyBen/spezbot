using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
namespace Music
{
    public class TestCommands : BaseCommandModule
    {
        public Dictionary<string, Markov> Markovs { get; set; }
        [Command("ping")]
        public async Task PingCommand(CommandContext ctx)
        {
            Console.WriteLine("ping");
            await ctx.RespondAsync("Pong...");
        }
        // this is cause ll0l 
        [Command("gen"), Aliases("g")]
        public async Task Generate(CommandContext ctx, [RemainingText] string text) {
            Markov mk;
            
            if (!Markovs.ContainsKey(ctx.Guild.Id.ToString())) {
                await ctx.RespondAsync("No data exists for this server");
                return;
            } else {
                mk = Markovs[ctx.Guild.Id.ToString()];
                await ctx.RespondAsync(mk.Generate(text));
            }
        }
    }
}

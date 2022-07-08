using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.IO;
namespace Music.Commands {
    public partial class MusicCommands : BaseCommandModule {
        [Command("update")]
        public async void Update(CommandContext ctx, [RemainingText] String url) {
            if (ctx.Message.Author.Id != 386912562937331725) {
                await ctx.RespondAsync("This is a ben only command sorry xoxoxoxoxoxoox.");
                return;
            }

            if (url == null) {
                await ctx.RespondAsync("Please provide a url to update the bot with.");
                return;
            }
            
            var msg = await ctx.RespondAsync("Updating... 0%");
            using (HttpClient client = new HttpClient()) {
                var res = await client.GetAsync(url);
                using (var fs = new FileStream("Update", FileMode.Create)) {
                    await res.Content.CopyToAsync(fs);
                }
            }
        }
    }
}
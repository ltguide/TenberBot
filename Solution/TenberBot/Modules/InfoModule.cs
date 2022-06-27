using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenberBot.Modules;
public class InfoModule : ModuleBase<SocketCommandContext>
{
	// ~say hello world -> hello world
	[Command("say")]
	[Summary("Echoes a message.")]
	public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
		=> ReplyAsync(echo);
}

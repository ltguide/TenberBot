using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TenberBot;

internal class Program
{
    private readonly DiscordSocketClient client;
    private readonly CommandService commandService;
    private readonly IServiceProvider serviceProvider;

    private Program()
    {
        client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,

            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            MessageCacheSize = 50,
        });

        commandService = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,

            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        });

        // Subscribe the logging handler to both the client and the CommandService.
        client.Log += Log;
        commandService.Log += Log;

        // Setup your DI container.
        serviceProvider = ConfigureServices();
    }

    // If any services require the client, or the CommandService, or something else you keep on hand,
    // pass them as parameters into this method as needed.
    // If this method is getting pretty long, you can seperate it out into another file using partials.
    private static IServiceProvider ConfigureServices()
    {
        var map = new ServiceCollection();
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            //.AddSingleton(new SomeServiceClass());

        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return map.BuildServiceProvider();
    }

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        // Centralize the logic for commands into a separate method.
        await InitCommands();

        var token = Environment.GetEnvironmentVariable("TenberToken");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(Timeout.Infinite);
    }

    private Task Log(LogMessage message)
    {
        //synchronous on the gateway thread, advised to use Task.Run

        //Console.WriteLine(msg.ToString());

        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();


        return Task.CompletedTask;
    }

    private async Task InitCommands()
    {
        // Either search the program and add all Module classes that can be found.
        // Module classes MUST be marked 'public' or they will be ignored.
        // You also need to pass your 'IServiceProvider' instance now,
        // so make sure that's done before you get here.
        await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        // Or add Modules manually if you prefer to be a little more explicit:
        //await commandService.AddModuleAsync<SomeModule>(serviceProvider);
        // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

        // Subscribe a handler to see if a message invokes a command.
        client.MessageReceived += HandleCommandAsync;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == client.CurrentUser.Id || msg.Author.IsBot) return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Replace the '!' with whatever character
        // you want to prefix your commands with.
        // Uncomment the second half if you also want
        // commands to be invoked by mentioning the bot instead.
        if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            // Create a Command Context.
            var context = new SocketCommandContext(client, msg);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully).
            var result = await commandService.ExecuteAsync(context, pos, serviceProvider);

            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}

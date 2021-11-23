using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DexterSlash.Events
{
    public class InteractionEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionEvent> _logger;

        public InteractionEvent(ILogger<InteractionEvent> logger, DiscordShardedClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = logger;
        }

        public void Initialize()
        {
            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;

            _client.ShardReady += async (_) =>
            {
                _logger.LogInformation("Initializing guild commands.");
                
                try
                {
                    await _commands.RegisterCommandsToGuildAsync(613441321751019550);
                    _logger.LogInformation("Sucessfully initialized commands!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to initialize guild commands!\n{ex}");
                }
            };
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new ShardedInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext interactionContext, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}

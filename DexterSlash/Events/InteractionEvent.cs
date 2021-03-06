using DexterSlash.Attributes;
using DexterSlash.Databases.Models.GuildConfiguration;
using DexterSlash.Databases.Repositories;
using DexterSlash.Enums;
using DexterSlash.Exceptions;
using DexterSlash.Extensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace DexterSlash.Events
{
    public class InteractionEvent : Event
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionEvent> _logger;
        private readonly Dictionary<ModuleAttribute, ModuleInfo> _modules;

        public InteractionEvent(ILogger<InteractionEvent> logger, DiscordShardedClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = logger;

            _modules = new();
        }

        public override void Initialize()
        {
            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results
            _commands.SlashCommandExecuted += SlashCommandExecuted;

            _client.ShardReady += GenerateCommands;

            _client.JoinedGuild += GenerateGuild;

            _client.Log += Log;

            _commands.Log += Log;

            _commands.Modules.ToList().ForEach(module => {
                var attribute = module.Preconditions
                    .Where(a => a.GetType() == typeof(ModuleAttribute))
                    .FirstOrDefault();

                if (attribute != null)
                    if (attribute is ModuleAttribute modA)
                        _modules.Add(modA, module);
            });
        }

        private async Task Log(LogMessage logMessage) {
            LogLevel logLevel = logMessage.Severity switch
            {
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Debug => LogLevel.Debug,
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Warning => LogLevel.Warning,
                _ => throw new NotImplementedException()
            };

            _logger.Log(logLevel, logMessage.Exception, logMessage.Message);
        }

        private async Task GenerateCommands(DiscordSocketClient shard)
        {
            _logger.LogInformation($"Initializing guild commands for shard {shard.ShardId}.");

            try
            {
                foreach (var guild in shard.Guilds)
                    await GenerateGuild(guild);

                _logger.LogInformation($"Sucessfully initialized commands for shard {shard.ShardId}!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize guild commands for shard {shard.ShardId}!\n{ex}");
            }
        }


        private async Task GenerateGuild(SocketGuild guild)
        {
            List<ModuleInfo> gModules = new();

            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var scopedRepo = new ConfigRepository(scope.ServiceProvider);

            foreach (var module in _modules)
                if (await scopedRepo.GetGuildConfig(module.Key.Module, guild.Id) != null)
                    gModules.Add(module.Value);

            await _commands.AddModulesToGuildAsync(
                guild,
                true,
                gModules.ToArray()
            );
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
                _logger.LogError(ex, $"Unable to execute {arg.Type} in channel {arg.Channel}");

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext interactionContext, Discord.Interactions.IResult result)
        {
            if (result.IsSuccess)
                return;

            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    if (result.ErrorReason.Length <= 0)
                        return;

                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Halt! Don't go there-")
                        .WithDescription(result.ErrorReason)
                        .SendEmbed(interactionContext.Interaction);

                    break;
                case InteractionCommandError.UnknownCommand:
                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Unknown Command.")
                        .WithDescription($"Oopsies! It seems as if the command **{commandInfo.Name}** doesn't exist!")
                        .SendEmbed(interactionContext.Interaction);

                    break;
                case InteractionCommandError.BadArgs:
                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Unable to parse command!")
                        .WithDescription($"Invalid amount of command arguments.")
                        .SendEmbed(interactionContext.Interaction);

                    break;
                case InteractionCommandError.Exception:
                    if (result.ErrorReason.Contains("ObjectNotFound"))
                    {
                        await CreateEmbed(EmojiEnum.Annoyed)
                            .WithTitle(result.ErrorReason)
                            .WithDescription($"If you believe this was an error, please do contact a developer!\n" +
                                $"If the {result.ErrorReason.Split(' ')[0].ToLower()} does exist, it may be due to caching. If so, please wait a few minutes.")
                            .SendEmbed(interactionContext.Interaction);

                        return;
                    }

                    if (result is ExecuteResult executeResult)
                    {
                        if (executeResult.Exception is MusicException music)
                        {
                            await CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle($"Unable to {music.Name}!")
                                .WithDescription(music.Description)
                                .SendEmbed(interactionContext.Interaction);

                            return;
                        }
                    }

                    // If the error is not an ObjectNotFound error, we log the message to the console with the appropriate data.
                    _logger.LogWarning($"Unknown statement reached!\nCommand: {commandInfo.Name}\nresult: {result}");

                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle(Regex.Replace(result.Error.GetType().Name, @"(?<!^)(?=[A-Z])", " "))
                        .WithDescription(result.ErrorReason)
                        .SendEmbed(interactionContext.Interaction);

                    break;
                case InteractionCommandError.Unsuccessful:
                    await CreateEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Command Unsuccessful!")
                        .WithDescription($"I was unable to run this command, please contact a developer!")
                        .SendEmbed(interactionContext.Interaction);

                    break;
                default:
                    break;
            }

        }

    }
}

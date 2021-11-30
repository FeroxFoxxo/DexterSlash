﻿using DexterSlash.Enums;
using DexterSlash.Attributes;
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

        private bool _hasInitialized;

        public InteractionEvent(ILogger<InteractionEvent> logger, DiscordShardedClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = logger;

            _hasInitialized = false;
        }

        public override void Initialize()
        {
            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results
            _commands.SlashCommandExecuted += SlashCommandExecuted;

            _client.ShardReady += GenerateCommands;
        }

        private async Task GenerateCommands(DiscordSocketClient shard) {

            if (!_hasInitialized)
            {
                _hasInitialized = true;

                _logger.LogInformation("Initializing global commands.");

                try
                {
                    await _commands.AddCommandsGloballyAsync(
                        true,
                        _commands.SlashCommands.Where(x => x.Attributes.Where(x => x is GlobalAttribute).Any()).ToArray()
                    );

                    _logger.LogInformation("Sucessfully initialized global commands!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to initialize guild commands!\n{ex}");
                }
            }

            _logger.LogInformation($"Initializing guild commands for shard {shard.ShardId}.");

            try
            {
                foreach (var guild in shard.Guilds)
                    await _commands.AddCommandsToGuildAsync(
                        guild,
                        true,
                        _commands.SlashCommands.Where(x => !x.Attributes.Where(x => x is GlobalAttribute).Any()).ToArray()
                    );

                _logger.LogInformation($"Sucessfully initialized commands for shard {shard.ShardId}!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize guild commands for shard {shard.ShardId}!\n{ex}");
            }
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

                    await interactionContext.Interaction.RespondAsync(
                        embed: 
                            CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Halt! Don't go there-")
                                .WithDescription(result.ErrorReason)
                                .Build(),
                        ephemeral: true
                        );
                    break;
                case InteractionCommandError.UnknownCommand:
                    await interactionContext.Interaction.RespondAsync(
                        embed:
                            CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Unknown Command.")
                                .WithDescription($"Oopsies! It seems as if the command **{commandInfo.Name}** doesn't exist!")
                                .Build(),
                        ephemeral: true
                        );
                    break;
                case InteractionCommandError.BadArgs:
                    await interactionContext.Interaction.RespondAsync(
                        embed:
                            CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Unable to parse command!")
                                .WithDescription($"Invalid amount of command arguments.")
                                .Build(),
                        ephemeral: true
                        );
                    break;
                case InteractionCommandError.Exception:
                    if (result.ToString().Contains("ObjectNotFound"))
                    {
                        await interactionContext.Interaction.RespondAsync(
                            embed:
                                CreateEmbed(EmojiEnum.Annoyed)
                                    .WithTitle(result.ErrorReason)
                                    .WithDescription($"If you believe this was an error, please do contact a developer!\n" +
                                        $"If the {result.ErrorReason.Split(' ')[0].ToLower()} does exist, it may be due to caching. If so, please wait a few minutes.")
                                    .Build()
                            );
                        return;
                    }

                    // If the error is not an ObjectNotFound error, we log the message to the console with the appropriate data.
                    _logger.LogWarning($"Unknown statement reached!\nCommand: {commandInfo.Name}\nresult: {result}");

                    await interactionContext.Interaction.RespondAsync(
                        embed:
                            CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle(Regex.Replace(result.Error.GetType().Name, @"(?<!^)(?=[A-Z])", " "))
                                .WithDescription(result.ErrorReason)
                                .Build()
                        );

                    break;
                case InteractionCommandError.Unsuccessful:
                    await interactionContext.Interaction.RespondAsync(
                        embed:
                            CreateEmbed(EmojiEnum.Annoyed)
                                .WithTitle("Command Unsuccessful!")
                                .WithDescription($"I was unable to run this command, please contact a developer!")
                                .Build()
                        );
                    break;
                default:
                    break;
            }

        }

    }
}
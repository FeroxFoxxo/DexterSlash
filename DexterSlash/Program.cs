using DexterSlash.Databases.Context;
using DexterSlash.Events;
using DexterSlash.Logging;
using DexterSlash.Workers;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Figgle;
using Genbox.WolframAlpha;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Lyrics;
using Lavalink4NET.MemoryCache;
using Lavalink4NET.Tracking;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using System.Reflection;

// PRE LOGIN

var builder = WebApplication.CreateBuilder(args);

string name;
int shards;

using (var restClient = new DiscordRestClient())
{
    await restClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));

    shards = await restClient.GetRecommendedShardCountAsync();

    name = restClient.CurrentUser.Username;
}

var foreground = Console.ForegroundColor;

Console.ForegroundColor = ConsoleColor.Cyan;

Console.WriteLine(FiggleFonts.Standard.Render(name));

Console.Title = $"{name} (Discord.Net v{DiscordConfig.Version})";

Console.ForegroundColor = foreground;

// DATABASE

string connectionString =
            $"Server={   Environment.GetEnvironmentVariable("MYSQL_HOST")};" +
            $"Port={     Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
            $"Database={ Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
            $"Uid={      Environment.GetEnvironmentVariable("MYSQL_USER")};" +
            $"Pwd={      Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

builder.Services.AddDbContext<DatabaseContext>(x => x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// DISCORD

builder.Services

    .AddSingleton(provider =>
    {
        var client = new DiscordShardedClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 50,
            TotalShards = shards,
            LogLevel = LogSeverity.Debug
        });

        return client;
    })

    .AddSingleton(new InteractionServiceConfig
    {
        DefaultRunMode = RunMode.Async,
        LogLevel = LogSeverity.Debug,
        UseCompiledLambda = true
    })

    .AddSingleton<InteractionService>()

    .AddSingleton(provider =>
    {
        var client = provider.GetRequiredService<DiscordShardedClient>();
        return new InteractiveService(client, TimeSpan.FromMinutes(5));
    })

    .AddSingleton<ILavalinkCache, LavalinkCache>()

    .AddSingleton<IAudioService, LavalinkNode>()

    .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()

    .AddSingleton(new LavalinkNodeOptions
    {
        RestUri = "http://localhost:2333/",
        WebSocketUri = "ws://localhost:2333/",
        Password = "youshallnotpass"
    })

    .AddSingleton<LyricsOptions>()

    .AddSingleton<LyricsService>()

    .AddSingleton<InactivityTrackingOptions>()

    .AddSingleton<InactivityTrackingService>()

    .AddHostedService<DiscordWorker>()

    .AddSingleton(new WolframAlphaClient(Environment.GetEnvironmentVariable("WOLFRAM_ALPHA")))

    .AddSingleton(new ClientCredentialsRequest(Environment.GetEnvironmentVariable("SPOTIFY_ID"), Environment.GetEnvironmentVariable("SPOTIFY_SECRET")));

// EVENTS

var events = Assembly.GetAssembly(typeof(Event)).GetTypes()
    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Event)))
    .ToList();

events.ForEach(type => builder.Services.AddSingleton(type));

// WEB

builder.Services

    .AddDatabaseDeveloperPageExceptionFilter()

    .AddRazorPages();

// LOGGING

builder.Logging
    
    .ClearProviders()
    
    .AddProvider(new LoggerProvider());

// BUILD

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app
        .UseExceptionHandler("/Error")
        
        .UseHsts();
}
else
{
    app
        .UseDeveloperExceptionPage()
        
        .UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<DatabaseContext>();

    context.Database.Migrate();

    context.Database.EnsureCreated();
}

events.ForEach(type => (app.Services.GetService(type) as Event).Initialize());

app
    .UseHttpsRedirection()
    
    .UseStaticFiles()
    
    .UseRouting()
    
    .UseAuthorization();

app.MapRazorPages();

app.Run();
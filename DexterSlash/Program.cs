using DexterSlash.Databases.Context;
using DexterSlash.Workers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Genbox.WolframAlpha;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Lyrics;
using Lavalink4NET.MemoryCache;
using Lavalink4NET.Tracking;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;

var builder = WebApplication.CreateBuilder(args);

// DISCORD

builder.Services
    .AddSingleton(provider =>
    {
        var client = new DiscordShardedClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 50,
            TotalShards = Configuration.GetValue<int>("shards"),
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

// WEB

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();

var app = builder.Build();

// BUILD

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<DatabaseContext>();

    context.Database.Migrate();

    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
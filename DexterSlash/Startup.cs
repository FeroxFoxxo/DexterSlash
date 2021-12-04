using DexterSlash.Databases.Context;
using DexterSlash.Events;
using DexterSlash.Identity;
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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using System.Reflection;

namespace DexterSlash
{
	public class Startup
    {

        private readonly IConfiguration Configuration;

        private readonly IWebHostEnvironment CurrentEnvironment;

        private static readonly string connectionString =
            $"Server={   Environment.GetEnvironmentVariable("MYSQL_HOST")};" +
            $"Port={     Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
            $"Database={ Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
            $"Uid={      Environment.GetEnvironmentVariable("MYSQL_USER")};" +
            $"Pwd={      Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // BOT

            services
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

            GetEvents()
                .ForEach(type => services.AddSingleton(type));

            // WEB

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/api/login";
                    options.LogoutPath = "/api/logout";
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
                    options.Cookie.MaxAge = new TimeSpan(7, 0, 0, 0);
                    options.Cookie.Name = "Dexter.USF.AccessToken";
                    options.Cookie.HttpOnly = false;
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.Headers["Location"] = context.RedirectUri;
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                })
                .AddDiscord(options =>
                {
                    options.ClientId = Environment.GetEnvironmentVariable("DISCORD_OAUTH_CLIENT_ID");
                    options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_OAUTH_CLIENT_SECRET");
                    options.Scope.Add("guilds");
                    options.Scope.Add("identify");
                    options.SaveTokens = true;
                    options.Prompt = "none";
                    options.AccessDeniedPath = "/oauthfailed";
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    options.CorrelationCookie.HttpOnly = false;
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder("Cookies")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddSingleton<OAuthManager>();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.Name = "Dexter.USF.AccessToken";

                // You might want to only set the application cookies over a secure connection:
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;

                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            services.AddRazorPages();

            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();
            }

            if (!CurrentEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
        }

        public static List<Type> GetEvents() => Assembly.GetAssembly(typeof(Event)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Event))).ToList();

    }
}
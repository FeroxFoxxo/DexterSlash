using AspNetCoreRateLimit;
using DexterSlash.Databases.Context;
using DexterSlash.Events;
using DexterSlash.Identity;
using DexterSlash.Middlewares;
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
using Lavalink4NET.Player;
using Lavalink4NET.Tracking;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using System.Reflection;

namespace DexterSlash
{
    public class Startup
    {

        private readonly IConfiguration Configuration;

        private readonly IWebHostEnvironment CurrentEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = $"Server={   Environment.GetEnvironmentVariable("MYSQL_HOST")};" +
                                      $"Port={     Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
                                      $"Database={ Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
                                      $"Uid={      Environment.GetEnvironmentVariable("MYSQL_USER")};" +
                                      $"Pwd={      Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};";

            services

                .AddSingleton<DiscordShardedClient>()

                .AddSingleton(new InteractionServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Debug,
                    UseCompiledLambda = true
                })

                .AddSingleton<InteractionService>()

                .AddSingleton<ILavalinkCache, LavalinkCache>()

                .AddSingleton<LyricsOptions>()

                .AddSingleton<LyricsService>()

                .AddSingleton<InactivityTrackingOptions>()

                .AddSingleton<InactivityTrackingService>()

                .AddSingleton<IAudioService, LavalinkNode>()
                
                .AddSingleton<VoteLavalinkPlayer>()

                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()

                .AddSingleton(new LavalinkNodeOptions {
                    RestUri = "http://localhost:2333/",
                    WebSocketUri = "ws://localhost:2333/",
                    Password = "youshallnotpass"
                })

                .AddSingleton(provider =>
                {
                    var client = provider.GetRequiredService<DiscordShardedClient>();
                    return new InteractiveService(client, TimeSpan.FromMinutes(5));
                })
                
                .AddHostedService<DiscordWorker>()

                .AddDbContext<DatabaseContext>(x => x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)))

                .AddSingleton<OAuthManager>()

                .AddSingleton(new WolframAlphaClient(Environment.GetEnvironmentVariable("WOLFRAM_ALPHA")))
                
                .AddSingleton(new ClientCredentialsRequest(Environment.GetEnvironmentVariable("SPOTIFY_ID"), Environment.GetEnvironmentVariable("SPOTIFY_SECRET")));

            GetEvents()
                .ForEach(type => services.AddSingleton(type));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/api/login";
                    options.LogoutPath = "/api/logout";
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
                    options.Cookie.MaxAge = new TimeSpan(7, 0, 0, 0);
                    options.Cookie.Name = "dex_access_token";
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

            if (CurrentEnvironment.IsDevelopment()) {
                services.AddCors(o => o.AddPolicy("AngularDevCors", builder =>
                {
                    builder.WithOrigins("http://127.0.0.1:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }));
            }

            // Stores rate limit counters and ip rules.
            services
                .AddMemoryCache()

                .AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>()

            // Loads general configuration from appsettings.json
                .Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"))

            // Load IP rules from appsettings.json.
                .Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"))

            // Injects counter and rules stores.
                .AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>()

                .AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>()

                .AddMvc();

            services.AddEndpointsApiExplorer()
                .AddSwaggerGen()

            // The IHttpContextAccessor service is not registered by default, though the clientId/clientIp resolvers use it.
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()

            // Configuration (resolvers, counter key builders).
                .AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (CurrentEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseCors("AngularDevCors");
            }

            app.UseMiddleware<APIExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseIpRateLimiting();

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static List<Type> GetEvents() => Assembly.GetAssembly(typeof(Event)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Event))).ToList();

    }
}
using AspNetCoreRateLimit;
using DexterSlash.Databases.Context;
using DexterSlash.Events;
using DexterSlash.Identity;
using DexterSlash.Logging;
using DexterSlash.Middlewares;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Genbox.WolframAlpha;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

            services.AddDbContext<DatabaseContext>(x => x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            services.AddSingleton<OAuthManager>();

            services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>(), new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug,
                UseCompiledLambda = true
            }));

            services.AddSingleton(new WolframAlphaClient(Environment.GetEnvironmentVariable("WOLFRAM_ALPHA")));

            services.Scan(scan => scan
                .FromAssemblyOf<IEvent>()
                .AddClasses(classes => classes.InNamespaces("DexterSlash.Events"))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/api/v1/login";
                    options.LogoutPath = "/api/v1/logout";
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
            services.AddMemoryCache();
            
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            // Loads general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            // Load IP rules from appsettings.json.
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // Injects counter and rules stores.
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.AddMvc();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // The IHttpContextAccessor service is not registered by default, though the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configuration (resolvers, counter key builders).
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new LoggerProvider());

            if (CurrentEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseCors("AngularDevCors");
            }

            app.UseMiddleware<APIExceptionHandlingMiddleware>();
            app.UseMiddleware<HeaderMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseIpRateLimiting();

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();

                scope.ServiceProvider.GetRequiredService<InteractionService>()
                    .AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);
            }

            app.ApplicationServices.GetServices<IEvent>().ToList().ForEach(eventI => eventI.Initialize());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
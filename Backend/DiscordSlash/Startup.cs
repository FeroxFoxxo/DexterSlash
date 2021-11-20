using AspNetCoreRateLimit;
using DiscordSlash.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DiscordSlash
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

            services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddHostedService<DiscordBot>();

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

            _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("Tokens", x =>
                {
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"))),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder("Cookies", "Tokens")
                    .RequireAuthenticatedUser()
                    .Build();
            });

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

            // The IHttpContextAccessor service is not registered by default, though the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configuration (resolvers, counter key builders).
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
    }
}
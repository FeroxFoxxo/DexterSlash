namespace DiscordSlash.Middlewares
{
    public class HeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public HeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.Headers["Host"] = Environment.GetEnvironmentVariable("SERVICE_DOMAIN");
            await _next(context);
        }
    }
}

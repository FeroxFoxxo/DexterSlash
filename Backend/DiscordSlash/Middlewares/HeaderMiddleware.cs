namespace DiscordSlash.Middlewares
{
    public class HeaderMiddleware
    {
        private readonly RequestDelegate Next;

        public HeaderMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.Headers["Host"] = Environment.GetEnvironmentVariable("SERVICE_DOMAIN");
            await Next(context);
        }
    }
}

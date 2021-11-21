using DiscordSlash.Exceptions;
using Newtonsoft.Json;

namespace DiscordSlash.Middlewares
{
    public class APIExceptionHandlingMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly ILogger Logger;

        public APIExceptionHandlingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            Next = next;
            Logger = loggerFactory.CreateLogger<APIExceptionHandlingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await Next(context);
            }
            catch (BaseAPIException ex)
            {
                string message = ex.Message;
                context.Response.StatusCode = 400;

                if (ex is UnauthorizedException)
                {
                    context.Response.StatusCode = 401;
                }

                if (ex is ResourceNotFoundException)
                {
                    context.Response.StatusCode = 404;
                }

                Logger.LogWarning($"Encountered API error type {ex.Error}, message: " + message);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { customError = ex.Error, message = message }));
            }
        }
    }
}

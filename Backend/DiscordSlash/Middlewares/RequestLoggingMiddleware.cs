namespace DiscordSlash.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly ILogger Logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            Next = next;
            Logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
        }

        private string GetIP(HttpContext context)
        {
            try
            {
                string ip = context.Request.Headers["X-Forwarded-For"];

                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.Request.Headers["REMOTE_ADDR"];
                }
                else
                {
                    ip = ip.Split(',').Last().Trim();
                }

                if (string.IsNullOrEmpty(ip))
                {
                    return context.Connection.RemoteIpAddress.ToString();
                }

                return ip;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting IP");
                return "";
            }
        }

        public async Task Invoke(HttpContext context)
        {
            string method = context.Request?.Method;

            switch (method)
            {
                case "DELETE":
                    method = "DEL";
                    break;
                case "OPTIONS":
                    method = "OPT";
                    break;
                case "PATCH":
                    method = "PAT";
                    break;
                case "TRACE":
                    method = "TRC";
                    break;
                case "CONNECT":
                    method = "CON";
                    break;
                case "HEAD":
                    method = "HED";
                    break;
                case "POST":
                    method = "POS";
                    break;
            }

            try
            {
                Logger.LogInformation($"INC {method} {context.Request?.Path.Value}{context.Request?.QueryString} | {GetIP(context)}");
                await Next(context);
                Logger.LogInformation($"{context.Response?.StatusCode} {method} {context.Request?.Path.Value}");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"500 {method} {context.Request?.Path.Value}");
                throw;
            }
        }
    }
}

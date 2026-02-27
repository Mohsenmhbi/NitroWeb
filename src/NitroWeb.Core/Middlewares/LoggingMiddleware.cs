using NitroWeb.Core.Context;
using NitroWeb.Core.Delegates;

namespace NitroWeb.Core.Middlewares;

public sealed class LoggingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var start = DateTimeOffset.UtcNow;
        Console.WriteLine($"--> {ctx.Request.Method} {ctx.Request.RawTarget}");

        await next(ctx);

        var ms = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
        Console.WriteLine($"<-- {ctx.Response.StatusCode} ({ms:0.0} ms)");
    }
}
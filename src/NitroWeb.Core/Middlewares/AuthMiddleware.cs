using NitroWeb.Core.Context;
using NitroWeb.Core.Delegates;
using NitroWeb.Core.Principal;

namespace NitroWeb.Core.Middlewares;

public sealed class AuthOptions
{
    public string RequireAuthPathsPrefix { get; init; } = "/secure";
    public HashSet<string> ValidBearerTokens { get; init; } = new(StringComparer.Ordinal);
}

public sealed class AuthMiddleware(AuthOptions options) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        // فقط مسیرهایی که با /secure شروع میشن نیاز به auth دارن
        if (!ctx.Request.Path.StartsWith(options.RequireAuthPathsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(ctx);
            return;
        }

        var auth = ctx.Request.GetHeader("Authorization");
        if (auth is null || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Response.StatusCode = 401;
            ctx.Response.SetHeader("WWW-Authenticate", "Bearer");
            await ctx.Response.WriteTextAsync("Unauthorized");
            return;
        }

        var token = auth["Bearer ".Length..].Trim();
        if (!options.ValidBearerTokens.Contains(token))
        {
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteTextAsync("Forbidden");
            return;
        }

        // یوزر ساده
        ctx.User = new UserPrincipal { IdentityName = "dev-user" };
        await next(ctx);
    }
}
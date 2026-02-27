using NitroWeb.Core.Delegates;
using NitroWeb.Core.Middlewares;

namespace NitroWeb.Core.Builders;

public sealed class AppBuilder
{
    private readonly List<IMiddleware> _middlewares = new();

    public AppBuilder Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public RequestDelegate Build()
    {
        RequestDelegate app = static ctx =>
        {
            ctx.Response.StatusCode = 404;
            return ctx.Response.WriteTextAsync("Not Found");
        };

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var current = _middlewares[i];
            var next = app;
            app = (ctx) => current.InvokeAsync(ctx, next);
        }

        return app;
    }
}

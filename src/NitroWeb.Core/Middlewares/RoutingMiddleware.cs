using NitroWeb.Core.Context;
using NitroWeb.Core.Controller;
using NitroWeb.Core.Delegates;
using NitroWeb.Core.RoutingAttributes.BaseAttributeRouter;

namespace NitroWeb.Core.Middlewares;

public sealed class RoutingMiddleware(AttributeRouter router) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var match = router.Match(ctx.Request.Method, ctx.Request.Path);
        if (match is null)
        {
            await next(ctx);
            return;
        }
        var controller = (ControllerBase)Activator.CreateInstance(match.ControllerType, ctx)!;

        // پارامترها
        var parameters = match.Action.GetParameters();
        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (match.RouteValues.TryGetValue(p.Name!, out var v))
                args[i] = ConvertSimple(v, p.ParameterType);
            else
                args[i] = p.HasDefaultValue ? p.DefaultValue : GetDefault(p.ParameterType);
        }

        var resultObj = match.Action.Invoke(controller, args);
        if (resultObj is IActionResult result)
        {
            await result.ExecuteAsync(ctx);
            return;
        }

        // اگر action چیزی برگردوند (مثلاً string یا object)
        if (resultObj is string s)
        {
            ctx.Response.StatusCode = 200;
            await ctx.Response.WriteTextAsync(s);
            return;
        }

        if (resultObj is not null)
        {
            ctx.Response.StatusCode = 200;
            await ctx.Response.WriteJsonAsync(resultObj);
            return;
        }

        ctx.Response.StatusCode = 204;
        await ctx.Response.WriteBytesAsync(Array.Empty<byte>(), "text/plain; charset=utf-8");
    }

    private static void SetControllerContext(ControllerBase controller, HttpContext ctx)
    {
        // init-only property را با reflection set می‌کنیم (ساده، برای demo)
        var prop = typeof(ControllerBase).GetProperty(nameof(ControllerBase.Context))!;
        prop.SetValue(controller, ctx);
    }

    private static object? ConvertSimple(string v, Type t)
    {
        if (t == typeof(string)) return v;
        if (t == typeof(int)) return int.Parse(v);
        if (t == typeof(long)) return long.Parse(v);
        if (t == typeof(Guid)) return Guid.Parse(v);
        if (t == typeof(bool)) return bool.Parse(v);
        return Convert.ChangeType(v, t);
    }

    private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
}

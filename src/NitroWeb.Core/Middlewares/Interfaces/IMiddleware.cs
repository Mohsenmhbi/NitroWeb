using NitroWeb.Core.Context;
using NitroWeb.Core.Delegates;

namespace NitroWeb.Core.Middlewares;

public interface IMiddleware
{
    Task InvokeAsync(HttpContext ctx, RequestDelegate next);
}
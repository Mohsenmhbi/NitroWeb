using NitroWeb.Core.Context;

namespace NitroWeb.Core.Controller;

// -------------------- ControllerBase & Action Results --------------------
public abstract class ControllerBase
{
    protected ControllerBase(HttpContext context) => Context = context;

    public HttpContext Context { get; }

    protected IActionResult Ok<T>(T obj) => new JsonResult(200, obj);
    protected IActionResult Text(string s, int status = 200) => new TextResult(status, s);
    protected IActionResult Unauthorized(string? msg = null) => new TextResult(401, msg ?? "Unauthorized");
}

public interface IActionResult
{
    Task ExecuteAsync(HttpContext ctx);
}

public sealed class TextResult(int status, string text) : IActionResult
{
    public Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteTextAsync(text);
    }
}

public sealed class JsonResult(int status, object value) : IActionResult
{
    public Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteJsonAsync(value);
    }
}

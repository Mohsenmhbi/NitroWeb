using NitroWeb.Core.Context;
using NitroWeb.Core.Controller;
using NitroWeb.Core.RoutingAttributes;

[Route("/api")]
public class ConsoleController : ControllerBase
{
    public ConsoleController(HttpContext ctx) : base(ctx) { }

    [HttpGet("/ping")]
    public IActionResult Ping()
    {
        Console.WriteLine($"[Controller] Ping hit: {Context.Request.RawTarget}");
        return Text("pong");
    }

    [HttpGet("/time")]
    public IActionResult Time()
    {
        Console.WriteLine("[Controller] Time hit");
        return Ok(new { utc = DateTimeOffset.UtcNow, local = DateTimeOffset.Now });
    }

    [HttpGet("/echo/{name}")]
    public IActionResult Echo(string name)
    {
        Console.WriteLine($"[Controller] Echo name={name}");
        return Ok(new { name });
    }

    // مسیر امن (AuthMiddleware چک می‌کند)
    [HttpGet("/secure/me")]
    public IActionResult Me()
    {
        Console.WriteLine("[Controller] Secure Me hit");
        return Ok(new { user = Context.User?.IdentityName ?? "unknown" });
    }
}
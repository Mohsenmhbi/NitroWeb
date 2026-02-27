using System.Reflection;
using NitroWeb.Core.Builders;
using NitroWeb.Core.Middlewares;
using NitroWeb.Core.RoutingAttributes.BaseAttributeRouter;
using NitroWeb.Core.TCPConfig;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var router = new AttributeRouter()
    .RegisterControllers(Assembly.GetExecutingAssembly());
    
var app = new AppBuilder()
    .Use(new LoggingMiddleware())
    .Use(new AuthMiddleware(new AuthOptions
    {
        RequireAuthPathsPrefix = "/secure",
        ValidBearerTokens = ["dev-token-123"]
    }))
    .Use(new RoutingMiddleware(router))
    .Build();
    
var server = new TcpHttpServer(app);
Console.WriteLine("Server started: http://127.0.0.1:8080");
Console.WriteLine("Press Ctrl+C to stop.");

await server.StartAsync("127.0.0.1", 8080, cts.Token);
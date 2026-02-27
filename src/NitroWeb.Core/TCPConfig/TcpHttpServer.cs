using System.Net;
using System.Net.Sockets;
using NitroWeb.Core.Context;
using NitroWeb.Core.Delegates;
using NitroWeb.Core.HttpParsers;

namespace NitroWeb.Core.TCPConfig;


// -------------------- TCP HTTP Server (TcpListener/TcpClient) --------------------
public sealed class TcpHttpServer
{
    private readonly RequestDelegate _app;
    public TcpHttpServer(RequestDelegate app) => _app = app;

    public async Task StartAsync(string ip, int port, CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Parse(ip), port);
        listener.Start();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(client, ct); // fire-and-forget per connection
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        
        using var stream = client.GetStream();

        try
        {
            var req = await HttpParser.ReadRequestAsync(stream, ct);
            if (req is null) return;

            var ctx = new HttpContext
            {
                Stream = stream,
                Request = req,
                Response = new HttpResponse(stream)
            };

            await _app(ctx);
        }
        catch (Exception ex)
        {
            // اگر وسط کار خراب شد، یک 500 ساده برگردون
            try
            {
                var resp = new HttpResponse(stream) { StatusCode = 500 };
                await resp.WriteTextAsync("Internal Server Error\n" + ex.Message);
            }
            catch { /* ignore */ }
        }
    }
}
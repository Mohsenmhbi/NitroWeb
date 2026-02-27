using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NitroWeb.Core.Principal;

namespace NitroWeb.Core.Context;
// -------------------- HttpContext / Request / Response --------------------
public sealed class HttpContext
{
    public required HttpRequest Request { get; init; }
    public required HttpResponse Response { get; init; }
    public required NetworkStream Stream { get; init; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    public UserPrincipal? User { get; set; }
}

public sealed class HttpRequest
{
    public required string Method { get; init; }
    public required string Path { get; init; }
    public required string RawTarget { get; init; } // path + query
    public required string HttpVersion { get; init; }

    public IReadOnlyDictionary<string, string> Headers => _headers;
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    public byte[] BodyBytes { get; internal set; } = Array.Empty<byte>();
    public string BodyText => Encoding.UTF8.GetString(BodyBytes);

    public string? GetHeader(string name) => _headers.TryGetValue(name, out var v) ? v : null;

    internal void AddHeader(string k, string v) => _headers[k] = v;
}

public sealed class HttpResponse
{
    private readonly NetworkStream _stream;
    public int StatusCode { get; set; } = 200;

    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Server"] = "MiniTcpWeb/0.1",
        ["Connection"] = "close"
    };

    public HttpResponse(NetworkStream stream) => _stream = stream;

    public void SetHeader(string key, string value) => _headers[key] = value;

    public async Task WriteBytesAsync(byte[] body, string contentType = "application/octet-stream")
    {
        _headers["Content-Type"] = contentType;
        _headers["Content-Length"] = body.Length.ToString();

        var reason = ReasonPhrase(StatusCode);
        var sb = new StringBuilder();
        sb.Append($"HTTP/1.1 {StatusCode} {reason}\r\n");
        foreach (var h in _headers) sb.Append($"{h.Key}: {h.Value}\r\n");
        sb.Append("\r\n");

        var head = Encoding.ASCII.GetBytes(sb.ToString());
        await _stream.WriteAsync(head);
        await _stream.WriteAsync(body);
    }

    public Task WriteTextAsync(string text, string contentType = "text/plain; charset=utf-8")
        => WriteBytesAsync(Encoding.UTF8.GetBytes(text), contentType);

    public Task WriteJsonAsync<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        return WriteTextAsync(json, "application/json; charset=utf-8");
    }

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        500 => "Internal Server Error",
        _ => "OK"
    };
}
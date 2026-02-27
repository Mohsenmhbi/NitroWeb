using System.Net.Sockets;
using System.Text;
using NitroWeb.Core.Context;

namespace NitroWeb.Core.HttpParsers;

// -------------------- HTTP Parser (حداقلی) --------------------
public static class HttpParser
{
    // این پارسر ساده است: فقط Content-Length را می‌خواند، chunked را پشتیبانی نمی‌کند.
    public static async Task<HttpRequest?> ReadRequestAsync(NetworkStream stream, CancellationToken ct)
    {
        // خواندن هدرها تا \r\n\r\n
        var headerBytes = await ReadUntilAsync(stream, Encoding.ASCII.GetBytes("\r\n\r\n"), 64 * 1024, ct);
        if (headerBytes.Length == 0) return null;

        var headerText = Encoding.ASCII.GetString(headerBytes);
        var lines = headerText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return null;

        var first = lines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (first.Length < 3) return null;

        var method = first[0].Trim();
        var target = first[1].Trim();
        var version = first[2].Trim();

        var path = target;
        var qIdx = target.IndexOf('?');
        if (qIdx >= 0) path = target[..qIdx];

        var req = new HttpRequest
        {
            Method = method,
            RawTarget = target,
            Path = path,
            HttpVersion = version
        };

        for (int i = 1; i < lines.Length; i++)
        {
            var idx = lines[i].IndexOf(':');
            if (idx <= 0) continue;
            var k = lines[i][..idx].Trim();
            var v = lines[i][(idx + 1)..].Trim();
            req.AddHeader(k, v);
        }

        var cl = req.GetHeader("Content-Length");
        if (cl is not null && int.TryParse(cl, out var len) && len > 0)
        {
            req.BodyBytes = await ReadExactAsync(stream, len, ct);
        }

        return req;
    }

    private static async Task<byte[]> ReadUntilAsync(NetworkStream stream, byte[] delimiter, int maxBytes, CancellationToken ct)
    {
        var buffer = new List<byte>(4096);
        var temp = new byte[1024];

        while (buffer.Count < maxBytes)
        {
            var n = await stream.ReadAsync(temp, ct);
            if (n <= 0) break;
            buffer.AddRange(temp.AsSpan(0, n).ToArray());

            if (EndsWith(buffer, delimiter))
                return buffer.ToArray();
        }

        return buffer.ToArray();
    }

    private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int len, CancellationToken ct)
    {
        var buf = new byte[len];
        var read = 0;
        while (read < len)
        {
            var n = await stream.ReadAsync(buf.AsMemory(read, len - read), ct);
            if (n <= 0) throw new IOException("Connection closed while reading body.");
            read += n;
        }
        return buf;
    }

    private static bool EndsWith(List<byte> data, byte[] suffix)
    {
        if (data.Count < suffix.Length) return false;
        for (int i = 0; i < suffix.Length; i++)
        {
            if (data[data.Count - suffix.Length + i] != suffix[i]) return false;
        }
        return true;
    }
}
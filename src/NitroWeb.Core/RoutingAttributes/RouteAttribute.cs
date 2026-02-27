namespace NitroWeb.Core.RoutingAttributes;

// -------------------- Routing Attributes --------------------
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RouteAttribute(string prefix) : Attribute
{
    public string Prefix { get; } = Normalize(prefix);
    private static string Normalize(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return "";
        if (!p.StartsWith('/')) p = "/" + p;
        return p.TrimEnd('/');
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HttpGetAttribute(string template) : Attribute
{
    public string Template { get; } = template;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HttpPostAttribute(string template) : Attribute
{
    public string Template { get; } = template;
}
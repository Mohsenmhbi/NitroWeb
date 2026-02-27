using System.Reflection;
using System.Text.RegularExpressions;
using NitroWeb.Core.Controller;

namespace NitroWeb.Core.RoutingAttributes.BaseAttributeRouter;
public sealed record RouteMatch(Type ControllerType, MethodInfo Action, IReadOnlyDictionary<string, string> RouteValues);

public sealed class AttributeRouter
{
    private readonly List<RouteEntry> _routes = new();

    public AttributeRouter RegisterControllers(Assembly asm)
    {
        var controllerTypes = asm.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t));

        foreach (var t in controllerTypes)
        {
            var prefixAttr = t.GetCustomAttribute<RouteAttribute>();
            var prefix = prefixAttr?.Prefix ?? "";

            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (var a in m.GetCustomAttributes<HttpGetAttribute>())
                    _routes.Add(RouteEntry.From("GET", prefix, a.Template, t, m));

                foreach (var a in m.GetCustomAttributes<HttpPostAttribute>())
                    _routes.Add(RouteEntry.From("POST", prefix, a.Template, t, m));
            }
        }

        return this;
    }

    public RouteMatch? Match(string method, string path)
    {
        foreach (var r in _routes)
        {
            if (!string.Equals(r.Method, method, StringComparison.OrdinalIgnoreCase))
                continue;

            var match = r.Regex.Match(path);
            if (!match.Success) continue;

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in r.ParamNames)
                values[name] = match.Groups[name].Value;

            return new RouteMatch(r.ControllerType, r.Action, values);
        }

        return null;
    }

    private sealed record RouteEntry(string Method, Regex Regex, string[] ParamNames, Type ControllerType, MethodInfo Action)
    {
        public static RouteEntry From(string method, string prefix, string template, Type controllerType, MethodInfo action)
        {
            var full = (prefix + NormalizeTemplate(template)).TrimEnd('/');
            if (full == "") full = "/";

            // /echo/{name} => ^/echo/(?<name>[^/]+)$
            var paramNames = new List<string>();
            var pattern = Regex.Replace(full, "{([a-zA-Z_][a-zA-Z0-9_]*)}", m =>
            {
                var n = m.Groups[1].Value;
                paramNames.Add(n);
                return $"(?<{n}>[^/]+)";
            });

            pattern = "^" + pattern + "$";
            return new RouteEntry(method, new Regex(pattern, RegexOptions.Compiled), paramNames.ToArray(), controllerType, action);
        }

        private static string NormalizeTemplate(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "";
            if (!t.StartsWith('/')) t = "/" + t;
            return t.TrimEnd('/');
        }
    }
}
using NitroWeb.Core.Context;

namespace NitroWeb.Core.Delegates;

public delegate Task RequestDelegate(HttpContext ctx);

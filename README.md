MiniTcpWeb (مینی فریم‌ورک HTTP روی TCP برای .NET)

MiniTcpWeb یک مینی فریم‌ورک آموزشی برای ساخت سرویس‌های HTTP بدون استفاده از ASP.NET Core است. این پروژه با TcpListener/TcpClient یک سرور TCP راه‌اندازی می‌کند، درخواست‌های HTTP را parse می‌کند، یک HttpContext می‌سازد، و سپس با یک Pipeline (Middleware) و Attribute Routing درخواست را به Controller مناسب هدایت می‌کند.

این پروژه برای یادگیری مفاهیم زیر طراحی شده:

تفاوت TCP خام با HTTP

پیاده‌سازی ساده‌ی HttpContext/Request/Response

طراحی Pipeline شبیه middlewareهای ASP.NET Core

Routing مبتنی بر Attribute

Auth و Logging در سطح middleware

ویژگی‌ها
✅ ۱) TCP-based HTTP Server

سرور با TcpListener روی IP/Port مشخص Listen می‌کند.

هر اتصال ورودی با TcpClient دریافت و پردازش می‌شود.

✅ ۲) HTTP Parsing حداقلی

Request line را می‌خواند: METHOD /path HTTP/1.1

هدرها را استخراج می‌کند.

بدنه را فقط با Content-Length می‌خواند.

نکته: chunked در این نسخه پشتیبانی نمی‌شود.

✅ ۳) HttpContext ساده

برای هر درخواست ساخته می‌شود و شامل این‌هاست:

Request (Method, Path, Headers, Body)

Response (StatusCode, Headers, WriteText/WriteJson)

Items برای اشتراک داده بین middlewareها

User برای احراز هویت (نمونه)

✅ ۴) Pipeline (Middleware)

مثل ASP.NET Core، زنجیره‌ای از middlewareها اجرا می‌شود:

نمونه ترتیب:

Logging

Auth

Routing

✅ ۵) Attribute Routing + ControllerBase

کنترلرها با [Route("/prefix")]

اکشن‌ها با [HttpGet("/path")] و [HttpPost("/path")]

پشتیبانی از route parameter مثل: /echo/{name}

✅ ۶) Auth Middleware (نمونه)

مسیرهایی که با یک prefix شروع می‌شوند (مثلاً /secure) نیاز به Authorization دارند.

نمونه‌ی ساده Bearer Token:

Authorization: Bearer dev-token-123

✅ ۷) Logging Middleware

هر درخواست و زمان پاسخ را در کنسول چاپ می‌کند.

معماری کلی (Flow)

۱) TcpHttpServer اتصال را می‌گیرد
۲) HttpParser هدرها و بدنه را parse می‌کند
۳) یک HttpContext ساخته می‌شود
۴) درخواست وارد Pipeline می‌شود
۵) RoutingMiddleware مسیر را match می‌کند و Controller را اجرا می‌کند
۶) Controller یک IActionResult برمی‌گرداند و Response نوشته می‌شود

ساختار پروژه (پیشنهادی)

Program.cs
تنظیم pipeline، router و start کردن سرور

MiniTcpWeb.cs
هسته فریم‌ورک:

TcpHttpServer

HttpParser

HttpContext/Request/Response

Pipeline و Middlewareها

AttributeRouter

ControllerBase و ActionResults

Controllers.cs
کنترلرهای نمونه (API)

اجرا
پیش‌نیاز

.NET 7 یا .NET 8

اجرا
dotnet run

سرور روی آدرس زیر بالا می‌آید (طبق تنظیم Program.cs):

http://127.0.0.1:8080

نمونه Routeها
Ping

GET /api/ping

curl http://127.0.0.1:8080/api/ping
Echo با route param

GET /api/echo/{name}

curl http://127.0.0.1:8080/api/echo/ali
مسیر امن (نیازمند Bearer Token)

GET /api/secure/me

بدون توکن:

curl http://127.0.0.1:8080/api/secure/me

با توکن:

curl -H "Authorization: Bearer dev-token-123" http://127.0.0.1:8080/api/secure/me
کانفیگ (Config)
۱) تعیین IP و Port

در Program.cs:

await server.StartAsync("127.0.0.1", 8080, cts.Token);

برای دسترسی از شبکه:

await server.StartAsync("0.0.0.0", 8080, cts.Token);
۲) ترتیب middlewareها

ترتیب مهم است (مثل ASP.NET):

var app = new AppBuilder()
    .Use(new LoggingMiddleware())
    .Use(new AuthMiddleware(options))
    .Use(new RoutingMiddleware(router))
    .Build();
۳) تنظیم Auth
new AuthOptions
{
    RequireAuthPathsPrefix = "/secure",
    ValidBearerTokens = ["dev-token-123"]
}

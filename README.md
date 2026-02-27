NitroWeb (A Minimal HTTP-over-TCP Framework for .NET)

NitroWeb is an educational mini-framework for building HTTP services without using ASP.NET Core. It starts a TCP server using TcpListener/TcpClient, parses incoming HTTP requests, constructs an HttpContext, and then uses a Pipeline (Middleware) plus Attribute Routing to dispatch the request to the appropriate Controller.

This project is designed to help you learn:

The difference between raw TCP and HTTP

A simple implementation of HttpContext / HttpRequest / HttpResponse

Designing a pipeline similar to ASP.NET Core middleware

Attribute-based routing

Authentication and logging at the middleware level

Features
✅ 1) TCP-based HTTP Server

The server listens on a specified IP/Port using TcpListener.

Each incoming connection is accepted and handled via TcpClient.

✅ 2) Minimal HTTP Parsing

Reads the request line: METHOD /path HTTP/1.1

Extracts headers

Reads the body only using Content-Length

Note: Transfer-Encoding: chunked is not supported in this version.

✅ 3) Simple HttpContext

A new context is created per request and contains:

Request (Method, Path, Headers, Body)

Response (StatusCode, Headers, WriteText/WriteJson)

Items for sharing data across middlewares

User for authentication (sample)

✅ 4) Pipeline (Middleware)

Like ASP.NET Core, the request passes through a chain of middlewares.

Example order:

Logging

Auth

Routing

✅ 5) Attribute Routing + ControllerBase

Controllers are marked with [Route("/prefix")]

Actions are marked with [HttpGet("/path")] and [HttpPost("/path")]

Supports route parameters like: /echo/{name}

✅ 6) Auth Middleware (Sample)

Paths that start with a prefix (e.g. /secure) require Authorization.

Simple Bearer Token example:

Authorization: Bearer dev-token-123

✅ 7) Logging Middleware

Prints each request and the response duration to the console.

High-Level Architecture (Flow)

TcpHttpServer accepts the connection

HttpParser parses headers and body

An HttpContext is created

The request enters the Pipeline

RoutingMiddleware matches the route and invokes the Controller

The Controller returns an IActionResult, and the Response is written back

Project Structure (Suggested)
Program.cs

Configures the pipeline, router, and starts the server.

MiniTcpWeb.cs (framework core)

Contains:

TcpHttpServer

HttpParser

HttpContext/Request/Response

Pipeline & Middlewares

AttributeRouter

ControllerBase & ActionResults

Controllers.cs

Sample API controllers.

Run
Prerequisites

.NET 7 or .NET 8

Start
dotnet run

The server will run at (based on your Program.cs config):

http://127.0.0.1:8080

Example Routes
Ping

GET /api/ping

curl http://127.0.0.1:8080/api/ping
Echo with route parameter

GET /api/echo/{name}

curl http://127.0.0.1:8080/api/echo/ali
Secure route (requires Bearer Token)

GET /api/secure/me

Without token:

curl http://127.0.0.1:8080/api/secure/me

With token:

curl -H "Authorization: Bearer dev-token-123" http://127.0.0.1:8080/api/secure/me
Configuration (Config)
1) Set IP and Port

In Program.cs:

await server.StartAsync("127.0.0.1", 8080, cts.Token);

To allow access from other machines on the network:

await server.StartAsync("0.0.0.0", 8080, cts.Token);
2) Middleware order

Order matters (similar to ASP.NET):

var app = new AppBuilder()
    .Use(new LoggingMiddleware())
    .Use(new AuthMiddleware(options))
    .Use(new RoutingMiddleware(router))
    .Build();
3) Auth settings
new AuthOptions
{
    RequireAuthPathsPrefix = "/secure",
    ValidBearerTokens = ["dev-token-123"]
}

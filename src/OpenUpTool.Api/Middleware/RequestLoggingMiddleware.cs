using System.Diagnostics;

namespace OpenUpTool.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            sw.Stop();

            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var time = sw.ElapsedMilliseconds;

            // Definir color basado en status code
            var colorCode = statusCode switch
            {
                >= 200 and < 300 => "\u001b[32m", // Verde
                >= 300 and < 400 => "\u001b[36m", // Cyan
                >= 400 and < 500 => "\u001b[33m", // Amarillo
                >= 500 => "\u001b[31m", // Rojo
                _ => "\u001b[37m" // Blanco
            };
            var resetCode = "\u001b[0m";

            // Log simple y limpio
            Console.WriteLine($"{colorCode}{method.PadRight(6)} {path.ToString().PadRight(40)} {statusCode} ({time}ms){resetCode}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            var method = context.Request.Method;
            var path = context.Request.Path;
            var time = sw.ElapsedMilliseconds;
            var resetCode = "\u001b[0m";

            // Rojo para errores
            Console.WriteLine($"\u001b[31m{method.PadRight(6)} {path.ToString().PadRight(40)} ERROR ({time}ms){resetCode}");
            _logger.LogError(ex, "Error procesando request {Method} {Path}", method, path);
            throw;
        }
    }
}

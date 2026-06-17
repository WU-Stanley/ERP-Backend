using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace WUIAM.Middleware
{
    /// <summary>
    /// Middleware that adds a correlation ID to each request for distributed tracing.
    /// The correlation ID is passed via 'X-Correlation-ID' header if present, otherwise generated.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private const string CorrelationIdFeatureKey = "__CorrelationId__";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Gets the correlation ID from the current HTTP context.
        /// </summary>
        public static string? GetCorrelationId(HttpContext context)
        {
            return context.Items[CorrelationIdFeatureKey] as string;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }

            context.Items[CorrelationIdFeatureKey] = correlationId;

            var sw = Stopwatch.StartNew();

            await _next(context);

            sw.Stop();
            var duration = sw.Elapsed;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation(
                "[CorrelationId: {CorrelationId}] {Method} {Path} completed in {DurationMs:F2}ms with status {StatusCode}",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                duration.TotalMilliseconds,
                statusCode);

            if (duration.TotalMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "[CorrelationId: {CorrelationId}] SLOW REQUEST: {Method} {Path} took {DurationMs:F2}ms (threshold: 1000ms)",
                    correlationId,
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds);
            }

            if (duration.TotalMilliseconds > 5000)
            {
                _logger.LogError(
                    "[CorrelationId: {CorrelationId}] CRITICAL SLOW REQUEST: {Method} {Path} took {DurationMs:F2}ms (threshold: 5000ms)",
                    correlationId,
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Extension methods for the CorrelationIdMiddleware.
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}

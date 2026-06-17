using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace WUIAM.Middleware
{
    /// <summary>
    /// Middleware that tracks performance metrics for all HTTP requests.
    /// Provides per-endpoint timing statistics (min, max, avg, count).
    /// </summary>
    public class PerformanceMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMetricsMiddleware> _logger;

        // In-memory metrics store (consider Redis/Datadog in production)
        private static readonly ConcurrentDictionary<string, EndpointMetrics> _metrics = new();
        private static readonly object _lock = new();

        public PerformanceMetricsMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current performance metrics for all endpoints.
        /// </summary>
        public static IDictionary<string, EndpointMetrics> GetMetrics()
        {
            return _metrics.ToDictionary(
                kvp => kvp.Key,
                kvp => new EndpointMetrics
                {
                    Count = kvp.Value.Count,
                    TotalMilliseconds = kvp.Value.TotalMilliseconds,
                    MinMilliseconds = kvp.Value.MinMilliseconds,
                    MaxMilliseconds = kvp.Value.MaxMilliseconds,
                    AverageMilliseconds = kvp.Value.Count > 0
                        ? kvp.Value.TotalMilliseconds / kvp.Value.Count
                        : 0
                });
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();

                var endpointKey = $"{context.Request.Method} {context.Request.Path}";
                var durationMs = sw.Elapsed.TotalMilliseconds;

                UpdateMetrics(endpointKey, durationMs);

                if (!context.Response.HasStarted)
                {
                    context.Response.Headers["X-Response-Time"] = $"{durationMs:F2}ms";
                }

                var level = durationMs > 1000
                    ? LogLevel.Warning
                    : durationMs > 5000
                        ? LogLevel.Error
                        : LogLevel.Information;

                _logger.Log(
                    level,
                    "[Perf] {Method} {Path} -> {StatusCode} in {DurationMs:F2}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    durationMs);
            }
        }

        private void UpdateMetrics(string endpointKey, double durationMs)
        {
            var metrics = _metrics.GetOrAdd(endpointKey, _ => new EndpointMetrics());

            lock (_lock)
            {
                metrics.Count++;
                metrics.TotalMilliseconds += durationMs;

                if (durationMs < metrics.MinMilliseconds || metrics.MinMilliseconds == 0)
                    metrics.MinMilliseconds = durationMs;

                if (durationMs > metrics.MaxMilliseconds)
                    metrics.MaxMilliseconds = durationMs;
            }
        }

        /// <summary>
        /// Resets all performance metrics (useful for testing).
        /// </summary>
        public static void ResetMetrics()
        {
            lock (_lock)
                _metrics.Clear();
        }
    }

    /// <summary>
    /// Performance metrics for a single endpoint.
    /// </summary>
    public class EndpointMetrics
    {
        public long Count { get; set; }
        public double TotalMilliseconds { get; set; }
        public double MinMilliseconds { get; set; }
        public double MaxMilliseconds { get; set; }
        public double AverageMilliseconds { get; set; }
    }

    /// <summary>
    /// Extension methods for the PerformanceMetricsMiddleware.
    /// </summary>
    public static class PerformanceMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMetrics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMetricsMiddleware>();
        }
    }
}

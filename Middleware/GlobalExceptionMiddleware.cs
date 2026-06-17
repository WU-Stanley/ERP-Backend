using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WUIAM.DTOs;

namespace WUIAM.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errors) = GetErrorDetails(exception);

            _logger.LogError(
                "Unhandled exception: {ExceptionType}: {ExceptionMessage} | Response: {Message} | Path: {Path} | Client IP: {ClientIp}",
                exception.GetType().Name,
                exception.GetBaseException().Message,
                message,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        private (HttpStatusCode StatusCode, string Message, string[] Errors) GetErrorDetails(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access.", Array.Empty<string>()),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message, new[] { ex.Message }),
                InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation.", new[] { ex.Message }),
                DbUpdateException => (HttpStatusCode.Conflict, "A database constraint violation occurred.", new[] { ex.InnerException?.Message ?? ex.Message }),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.", Array.Empty<string>())
            };
        }
    }
}

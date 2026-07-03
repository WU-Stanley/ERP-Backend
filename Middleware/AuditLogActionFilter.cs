using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using WUIAM.Interfaces;

namespace WUIAM.Middleware
{
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;
        private readonly string[] _skipControllers;

        public AuditLogActionFilter(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
            _skipControllers = new[] { "Auth", "Health", "Swagger", "Hangfire", "AuditLogs" };
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.ActionDescriptor.RouteValues["controller"];

            // Skip health/status controllers
            if (_skipControllers.Any(c => controllerName?.Contains(c, StringComparison.OrdinalIgnoreCase) == true))
            {
                await next();
                return;
            }

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userIdGuid = Guid.TryParse(userId, out var uid) ? uid : null;
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            // Determine action type from HTTP method
            var httpMethod = context.HttpContext.Request.Method;
            var actionType = httpMethod switch
            {
                "POST" => "Create",
                "PUT" => "Update",
                "PATCH" => "Update",
                "DELETE" => "Delete",
                "GET" => "Read",
                _ => "Other"
            };

            var entityName = controllerName;
            var entityId = ExtractEntityId(context);

            // Log before execution for Create/Update/Delete
            if (actionType is "Create" or "Update" or "Delete")
            {
                await _auditLogService.LogAsync(
                    actionType: actionType,
                    userId: userIdGuid,
                    entityName: entityName,
                    entityId: entityId,
                    description: $"{actionType} action on {entityName} via {context.ActionDescriptor.RouteValues["action"]}",
                    oldValues: null,
                    newValues: httpMethod == "DELETE" ? null : SerializeActionArguments(context.ActionArguments),
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }

            var result = await next();

            // Log after execution for success/failure
            if (result.Exception != null)
            {
                var exceptionSummary = $"{result.Exception.GetType().Name}: {result.Exception.Message}";
                await _auditLogService.LogAsync(
                    actionType: $"{actionType}Failed",
                    userId: userIdGuid,
                    entityName: entityName,
                    entityId: entityId,
                    description: $"{actionType} action on {entityName} failed: {result.Exception.Message}",
                    oldValues: null,
                    newValues: exceptionSummary,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }

        private static Guid? ExtractEntityId(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("id", out var id) && id is Guid g)
                return g;

            if (context.ActionArguments.TryGetValue("Id", out var Id) && Id is Guid G)
                return G;

            return null;
        }

        private static string SerializeActionArguments(System.Collections.Generic.IDictionary<string, object?> args)
        {
            try
            {
                var sensitiveKeys = new[] { "password", "token", "refreshToken", "authorization", "secret", "apiKey", "otp", "mfa" };
                var filtered = args.ToDictionary(
                    kvp => kvp.Key,
                    kvp => sensitiveKeys.Any(key => kvp.Key.Contains(key, StringComparison.OrdinalIgnoreCase))
                        ? "[redacted]"
                        : NormalizeAuditValue(kvp.Value, sensitiveKeys)
                );
                return System.Text.Json.JsonSerializer.Serialize(filtered);
            }
            catch
            {
                return "(serialization failed)";
            }
        }

        private static object? NormalizeAuditValue(object? value, string[] sensitiveKeys)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string stringValue)
            {
                return TryRedactJsonString(stringValue, sensitiveKeys, out var redacted)
                    ? redacted
                    : stringValue;
            }

            if (value is JsonElement jsonElement)
            {
                return RedactElement(jsonElement, sensitiveKeys);
            }

            try
            {
                var json = JsonSerializer.Serialize(value);
                using var document = JsonDocument.Parse(json);
                return RedactElement(document.RootElement, sensitiveKeys);
            }
            catch
            {
                return value.ToString();
            }
        }

        private static bool TryRedactJsonString(string value, string[] sensitiveKeys, out object? redacted)
        {
            try
            {
                using var document = JsonDocument.Parse(value);
                redacted = RedactElement(document.RootElement, sensitiveKeys);
                return true;
            }
            catch
            {
                redacted = null;
                return false;
            }
        }

        private static string RedactSensitiveJson(string value, string[] sensitiveKeys)
        {
            try
            {
                using var document = JsonDocument.Parse(value);
                var redacted = RedactElement(document.RootElement, sensitiveKeys);
                return JsonSerializer.Serialize(redacted);
            }
            catch
            {
                return value;
            }
        }

        private static object? RedactElement(JsonElement element, string[] sensitiveKeys)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => sensitiveKeys.Any(key => property.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
                        ? "[redacted]"
                        : RedactElement(property.Value, sensitiveKeys)),
                JsonValueKind.Array => element.EnumerateArray().Select(item => RedactElement(item, sensitiveKeys)).ToList(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
    }
}

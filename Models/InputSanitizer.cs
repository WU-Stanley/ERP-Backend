using System.Text;
using System.Text.RegularExpressions;

namespace WUIAM.Models
{
    /// <summary>
    /// Provides sanitization utilities for user input to prevent XSS and injection attacks.
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitizes a string by removing HTML tags and encoding special characters.
        /// Returns the original string if null or empty.
        /// </summary>
        public static string SanitizeHtml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // First strip HTML tags
            var stripped = Regex.Replace(input, "<[^>]*>", string.Empty);
            // Then encode
            return System.Net.WebUtility.HtmlEncode(stripped.Trim());
        }

        /// <summary>
        /// Sanitizes a string for SQL injection prevention.
        /// Note: Prefer parameterized queries over this method.
        /// </summary>
        public static string SanitizeSql(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove common SQL injection patterns
            var sanitized = input
                .Replace("--", "")
                .Replace(";", "")
                .Replace("/*", "")
                .Replace("*/", "")
                .Replace("'\"", "");

            return sanitized.Trim();
        }

        /// <summary>
        /// Validates that a string matches a specific pattern (e.g., email, phone).
        /// </summary>
        public static bool IsValidPattern(string input, string pattern)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Validates a phone number format (international format).
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(phone, @"^\+?[1-9]\d{6,14}$");
        }

        /// <summary>
        /// Trims and limits the length of a string.
        /// Returns the original string if null or empty.
        /// </summary>
        public static string TrimAndLimit(string? input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var trimmed = input.Trim();
            if (trimmed.Length > maxLength)
                return trimmed.Substring(0, maxLength);

            return trimmed;
        }

        /// <summary>
        /// Validates that a string is a valid GUID.
        /// </summary>
        public static bool IsValidGuid(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Guid.TryParse(input, out _);
        }

        /// <summary>
        /// Sanitizes a file name by removing special characters.
        /// </summary>
        public static string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Remove path traversal and special characters
            var sanitized = Regex.Replace(fileName, @"[^a-zA-Z0-9\._\-]", "_");
            return sanitized.Trim();
        }

        /// <summary>
        /// Validates that a URL is well-formed and safe.
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}

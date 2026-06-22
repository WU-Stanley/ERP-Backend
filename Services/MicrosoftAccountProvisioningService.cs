using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Services
{
    public class MicrosoftAccountProvisioningService : IMicrosoftAccountProvisioningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly WUIAMDbContext _context;
        private readonly ILogger<MicrosoftAccountProvisioningService> _logger;

        public MicrosoftAccountProvisioningService(
            HttpClient httpClient,
            IConfiguration configuration,
            WUIAMDbContext context,
            ILogger<MicrosoftAccountProvisioningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<MicrosoftAccountResult> CreateAccountAsync(
            string employeeName,
            string? jobTitle,
            CancellationToken cancellationToken = default)
        {
            var tenantId = RequiredSetting("MicrosoftIdentity:TenantId");
            var clientId = RequiredSetting("MicrosoftIdentity:ClientId");
            var clientSecret = RequiredSetting("MicrosoftIdentity:ClientSecret");
            var domain = (_configuration["IctOnboarding:MicrosoftAccount:Domain"] ?? "wigweuniversity.edu.ng")
                .Trim().TrimStart('@').ToLowerInvariant();
            var format = _configuration["IctOnboarding:MicrosoftAccount:EmailFormat"] ?? "{first}.{last}";
            var maximumAttempts = Math.Clamp(
                _configuration.GetValue<int?>("IctOnboarding:MicrosoftAccount:MaximumUniqueAttempts") ?? 100,
                2,
                1000);

            var (firstName, lastName) = SplitName(employeeName);
            var baseAlias = BuildAlias(format, firstName, lastName);
            var accessToken = await GetAccessTokenAsync(tenantId, clientId, clientSecret, cancellationToken);

            for (var attempt = 1; attempt <= maximumAttempts; attempt++)
            {
                var alias = attempt == 1 ? baseAlias : $"{baseAlias}{attempt}";
                var userPrincipalName = $"{alias}@{domain}";

                if (await ExistsLocallyAsync(userPrincipalName, cancellationToken) ||
                    await ExistsInMicrosoftAsync(userPrincipalName, accessToken, cancellationToken))
                {
                    continue;
                }

                var temporaryPassword = GenerateTemporaryPassword();
                var body = new
                {
                    accountEnabled = true,
                    displayName = employeeName.Trim(),
                    givenName = firstName,
                    surname = lastName,
                    jobTitle,
                    mailNickname = alias,
                    userPrincipalName,
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = true,
                        password = temporaryPassword
                    }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/users")
                {
                    Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                    var id = document.RootElement.GetProperty("id").GetString()
                        ?? throw new InvalidOperationException("Microsoft created the account without returning an object ID.");
                    var actualUpn = document.RootElement.TryGetProperty("userPrincipalName", out var upn)
                        ? upn.GetString() ?? userPrincipalName
                        : userPrincipalName;
                    return new MicrosoftAccountResult(id, actualUpn, temporaryPassword);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    continue;
                }

                var error = await ReadGraphErrorAsync(response, cancellationToken);
                _logger.LogError("Microsoft account provisioning failed with status {Status}: {Error}", response.StatusCode, error);
                throw new InvalidOperationException($"Microsoft account provisioning failed: {error}");
            }

            throw new InvalidOperationException($"Could not generate a unique Microsoft email after {maximumAttempts} attempts.");
        }

        private async Task<bool> ExistsLocallyAsync(string email, CancellationToken cancellationToken)
        {
            var normalized = email.ToLower();
            return await _context.Users.AnyAsync(user => user.UserEmail.ToLower() == normalized, cancellationToken) ||
                   await _context.EmployeeDetails.AnyAsync(employee => employee.Email.ToLower() == normalized, cancellationToken) ||
                   await _context.JobApplications.AnyAsync(application =>
                       application.MicrosoftUserPrincipalName != null &&
                       application.MicrosoftUserPrincipalName.ToLower() == normalized, cancellationToken);
        }

        private async Task<bool> ExistsInMicrosoftAsync(string userPrincipalName, string accessToken, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(userPrincipalName)}?$select=id");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode == HttpStatusCode.NotFound) return false;

            var error = await ReadGraphErrorAsync(response, cancellationToken);
            throw new InvalidOperationException($"Unable to check Microsoft email availability: {error}");
        }

        private async Task<string> GetAccessTokenAsync(
            string tenantId,
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = "https://graph.microsoft.com/.default"
            });
            using var response = await _httpClient.PostAsync(
                $"https://login.microsoftonline.com/{Uri.EscapeDataString(tenantId)}/oauth2/v2.0/token",
                content,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Unable to authenticate the Microsoft provisioning service.");

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return document.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Microsoft authentication returned no access token.");
        }

        private string RequiredSetting(string key) =>
            _configuration[key] ?? throw new InvalidOperationException($"Configuration setting '{key}' is required.");

        private static (string FirstName, string LastName) SplitName(string employeeName)
        {
            var names = employeeName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (names.Length < 2)
                throw new InvalidOperationException("The hired employee must have both a first name and last name before provisioning.");
            return (names[0], names[^1]);
        }

        private static string BuildAlias(string format, string firstName, string lastName)
        {
            var first = NormalizeName(firstName);
            var last = NormalizeName(lastName);
            var alias = format.ToLowerInvariant()
                .Replace("{first}", first)
                .Replace("{last}", last)
                .Replace("{firstinitial}", first[..1])
                .Replace("{lastinitial}", last[..1]);
            alias = Regex.Replace(alias, "[^a-z0-9._-]", string.Empty).Trim('.', '-', '_');
            if (string.IsNullOrWhiteSpace(alias) || alias.Contains('{') || alias.Length > 58)
                throw new InvalidOperationException("The configured Microsoft email format is invalid.");
            return alias;
        }

        private static string NormalizeName(string value)
        {
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var letters = decomposed.Where(character =>
                CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark);
            return Regex.Replace(new string(letters.ToArray()).Normalize(NormalizationForm.FormC).ToLowerInvariant(), "[^a-z0-9]", string.Empty);
        }

        private static string GenerateTemporaryPassword()
        {
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string symbols = "!@$%*-_";
            var characters = new List<char>
            {
                lower[RandomNumberGenerator.GetInt32(lower.Length)],
                upper[RandomNumberGenerator.GetInt32(upper.Length)],
                digits[RandomNumberGenerator.GetInt32(digits.Length)],
                symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
            };
            const string all = lower + upper + digits + symbols;
            while (characters.Count < 16)
                characters.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
            return new string(characters.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }

        private static async Task<string> ReadGraphErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                using var document = JsonDocument.Parse(content);
                return document.RootElement.GetProperty("error").GetProperty("message").GetString()
                    ?? $"Microsoft Graph returned {(int)response.StatusCode}.";
            }
            catch
            {
                return $"Microsoft Graph returned {(int)response.StatusCode}.";
            }
        }
    }
}

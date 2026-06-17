using WUIAM.Interfaces;

namespace WUIAM.Services
{
    public class TeamsMeetingService : ITeamsMeetingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TeamsMeetingService> _logger;

        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public TeamsMeetingService(HttpClient httpClient, IConfiguration configuration, ILogger<TeamsMeetingService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _tenantId = configuration["MicrosoftIdentity:TenantId"] ?? "";
            _clientId = configuration["MicrosoftIdentity:ClientId"] ?? "";
            _clientSecret = configuration["MicrosoftIdentity:ClientSecret"] ?? "";
        }

        public async Task<string?> CreateTeamsMeetingAsync(string title, DateTime startTime, DateTime endTime, string organizerEmail)
        {
            try
            {
                var accessToken = await GetGraphAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Failed to obtain Microsoft Graph access token. Using placeholder meeting link.");
                    return GeneratePlaceholderMeetingLink();
                }

                var requestUrl = "https://graph.microsoft.com/v1.0/users/" + Uri.EscapeDataString(organizerEmail) + "/events";

                var meetingBody = new
                {
                    subject = title,
                    body = new { contentType = "HTML", content = $"<p>You are invited to the meeting: {title}</p>" },
                    start = new { dateTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                    end = new { dateTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                    location = new { displayName = "Microsoft Teams Meeting" },
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness",
                    allowNewTimeProposals = false,
                    isCancellation = false,
                    responseRequested = true
                };

                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(meetingBody),
                    System.Text.Encoding.UTF8,
                    "application/json");

                jsonContent.Headers.Remove("Authorization");
                jsonContent.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(requestUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);

                    if (jsonElement.TryGetProperty("onlineMeetingUrl", out var meetingUrlProp))
                    {
                        var meetingUrl = meetingUrlProp.GetString();
                        _logger.LogInformation("Teams meeting created successfully: {MeetingUrl}", meetingUrl);
                        return meetingUrl;
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to create Teams meeting. Status: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Teams meeting.");
            }

            return GeneratePlaceholderMeetingLink();
        }

        public async Task<bool> CancelMeetingAsync(string teamsMeetingId)
        {
            try
            {
                var accessToken = await GetGraphAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken)) return false;

                var requestUrl = $"https://graph.microsoft.com/v1.0/events/{teamsMeetingId}";
                var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Teams meeting.");
                return false;
            }
        }

        public async Task<string?> GetMeetingLinkAsync(string teamsMeetingId)
        {
            try
            {
                var accessToken = await GetGraphAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken)) return null;

                var requestUrl = $"https://graph.microsoft.com/v1.0/events/{teamsMeetingId}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);

                    if (jsonElement.TryGetProperty("onlineMeetingUrl", out var meetingUrlProp))
                        return meetingUrlProp.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Teams meeting link.");
            }

            return null;
        }

        private async Task<string?> GetGraphAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(_tenantId) || string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
                return null;

            var tokenUrl = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default")
            });

            try
            {
                var response = await _httpClient.PostAsync(tokenUrl, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonResponse);
                return jsonElement.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
            }
            catch
            {
                return null;
            }
        }

        private string GeneratePlaceholderMeetingLink()
        {
            var meetingId = Guid.NewGuid().ToString("N")[..16];
            return $"https://teams.microsoft.com/l/meetup-join/19%3ameeting_{meetingId}";
        }
    }
}

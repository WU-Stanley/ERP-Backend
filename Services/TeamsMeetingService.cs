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

        public async Task<string?> CreateTeamsMeetingAsync(string title, DateTime startTime, DateTime endTime, string organizerEmail, List<string>? attendeeEmails = null)
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

                var attendees = attendeeEmails?.Select(email => new
                {
                    emailAddress = new { address = email },
                    type = "required"
                }).ToList();

                var meetingBody = new
                {
                    subject = title,
                    body = new { contentType = "HTML", content = $"<p>You are invited to the meeting: {title}</p>" },
                    start = new { dateTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                    end = new { dateTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                    location = new { displayName = "Microsoft Teams Meeting" },
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness",
                    attendees = attendees
                };

                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(meetingBody),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = jsonContent
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string? errorCode = null;
                    string? errorMessage = null;
                    string? graphRequestId = null;

                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(errorContent);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("error", out var errorProp))
                        {
                            if (errorProp.TryGetProperty("code", out var codeProp))
                                errorCode = codeProp.GetString();
                            if (errorProp.TryGetProperty("message", out var msgProp))
                                errorMessage = msgProp.GetString();
                            if (errorProp.TryGetProperty("innerError", out var innerProp))
                            {
                                if (innerProp.TryGetProperty("request-id", out var reqIdProp))
                                    graphRequestId = reqIdProp.GetString();
                            }
                        }
                    }
                    catch
                    {
                        // Fallback
                    }

                    if (string.IsNullOrEmpty(graphRequestId) && response.Headers.TryGetValues("request-id", out var headerValues))
                    {
                        graphRequestId = headerValues.FirstOrDefault();
                    }

                    _logger.LogWarning(
                        "Failed to create Teams meeting. Status: {Status}. Graph Error Code: {ErrorCode}, Message: {ErrorMessage}, Graph Request ID: {GraphRequestId}. Raw Response: {Response}",
                        response.StatusCode,
                        errorCode ?? "N/A",
                        errorMessage ?? "N/A",
                        graphRequestId ?? "N/A",
                        errorContent);
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
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    response.Headers.TryGetValues("request-id", out var headerValues);
                    var graphRequestId = headerValues?.FirstOrDefault() ?? "N/A";
                    _logger.LogWarning("Failed to cancel Teams meeting. Status: {Status}. Graph Request ID: {GraphRequestId}. Response: {Response}", response.StatusCode, graphRequestId, errorContent);
                    return false;
                }
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
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);

                    if (jsonElement.TryGetProperty("onlineMeetingUrl", out var meetingUrlProp))
                        return meetingUrlProp.GetString();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    response.Headers.TryGetValues("request-id", out var headerValues);
                    var graphRequestId = headerValues?.FirstOrDefault() ?? "N/A";
                    _logger.LogWarning("Failed to get Teams meeting link. Status: {Status}. Graph Request ID: {GraphRequestId}. Response: {Response}", response.StatusCode, graphRequestId, errorContent);
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
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Token request failed with status {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                }
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonResponse);
                return jsonElement.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during Graph access token request.");
                return null;
            }
        }

        private string GeneratePlaceholderMeetingLink()
        {
            var meetingId = Guid.NewGuid().ToString("N")[..16];
            return $"https://meet.jit.si/WU-Interview-{meetingId}";
        }
    }
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ProjectManagement.DTOs.Jira;
using ProjectManagement.Enums;
using ProjectManagement.Exceptions;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services.Interfaces;
using TaskStatus = ProjectManagement.Enums.TaskStatus;

namespace ProjectManagement.Services;

public class JiraService : IJiraService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJiraSettingsRepository _settingsRepository;
    private readonly ILogger<JiraService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public JiraService(
        IHttpClientFactory httpClientFactory,
        IJiraSettingsRepository settingsRepository,
        ILogger<JiraService> logger)
    {
        _httpClientFactory  = httpClientFactory;
        _settingsRepository = settingsRepository;
        _logger             = logger;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<(HttpClient client, string baseUrl)> CreateClientAsync()
    {
        var settings = await _settingsRepository.GetAsync();
        if (settings is null
            || string.IsNullOrWhiteSpace(settings.BaseUrl)
            || string.IsNullOrWhiteSpace(settings.Email)
            || string.IsNullOrWhiteSpace(settings.ApiToken))
        {
            throw new BadRequestException("Jira is not configured. Please configure Jira settings first.");
        }

        var client = _httpClientFactory.CreateClient("Jira");
        var token  = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.Email}:{settings.ApiToken}"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", token);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return (client, settings.BaseUrl);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string context)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound       => new NotFoundException($"Jira: {context} not found."),
            System.Net.HttpStatusCode.Unauthorized   => new BadRequestException("Jira credentials are invalid. Please check your email and API token."),
            System.Net.HttpStatusCode.BadRequest     => new BadRequestException($"Jira rejected the request: {body}"),
            _                                        => new BadRequestException($"Jira API error ({(int)response.StatusCode}): {body}")
        };
    }

    // ── Public methods ────────────────────────────────────────────────────────

    public async Task<string> TestConnectionAsync()
    {
        var (client, baseUrl) = await CreateClientAsync();
        var response = await client.GetAsync($"{baseUrl}/rest/api/3/myself");
        await EnsureSuccessAsync(response, "user");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var displayName = doc.RootElement.GetProperty("displayName").GetString() ?? "Unknown";
        return displayName;
    }

    public async Task<JiraIssueDto> GetIssueAsync(string issueKey)
    {
        var (client, baseUrl) = await CreateClientAsync();
        var response = await client.GetAsync(
            $"{baseUrl}/rest/api/3/issue/{issueKey}?fields=summary,status,assignee,issuetype");
        await EnsureSuccessAsync(response, $"issue {issueKey}");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return ParseIssue(doc.RootElement, baseUrl);
    }

    public async Task<JiraSearchResultDto> SearchIssuesAsync(
    string jql,
    string? nextPageToken = null,
    int maxResults = 50)
{
    var (client, baseUrl) = await CreateClientAsync();

    var payload = new Dictionary<string, object?>
    {
        ["jql"] = jql,
        ["maxResults"] = maxResults,
        ["fields"] = new[] { "summary", "status", "assignee", "issuetype" }
    };

    if (!string.IsNullOrEmpty(nextPageToken))
    {
        payload["nextPageToken"] = nextPageToken;
    }

    var body = JsonSerializer.Serialize(payload);

    var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"{baseUrl}/rest/api/3/search/jql")
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    var response = await client.SendAsync(request);
    await EnsureSuccessAsync(response, "issue search");

    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    var issues = root.GetProperty("issues")
        .EnumerateArray()
        .Select(el => ParseIssue(el, baseUrl))
        .ToList();

    return new JiraSearchResultDto
    {
        Issues = issues,
        NextPageToken = root.TryGetProperty("nextPageToken", out var tokenEl)
            ? tokenEl.GetString()
            : null,
        IsLast = root.TryGetProperty("isLast", out var isLastEl)
            && isLastEl.GetBoolean()
    };
}
    public async Task<string> CreateIssueAsync(
        string projectKey, string summary, string issueType, string? description)
    {
        var (client, baseUrl) = await CreateClientAsync();

        object descriptionContent = string.IsNullOrWhiteSpace(description)
            ? new { type = "doc", version = 1, content = Array.Empty<object>() }
            : new
            {
                type    = "doc",
                version = 1,
                content = new[]
                {
                    new
                    {
                        type    = "paragraph",
                        content = new[]
                        {
                            new { type = "text", text = description }
                        }
                    }
                }
            };

        var body = JsonSerializer.Serialize(new
        {
            fields = new
            {
                project     = new { key = projectKey },
                summary,
                issuetype   = new { name = issueType },
                description = descriptionContent
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rest/api/3/issue?notifyUsers=false")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        await EnsureSuccessAsync(response, "create issue");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("key").GetString()!;
    }

    // ── Parsing ───────────────────────────────────────────────────────────────

    private static JiraIssueDto ParseIssue(JsonElement el, string baseUrl)
    {
        var key    = el.GetProperty("key").GetString() ?? string.Empty;
        var fields = el.GetProperty("fields");

        var summary   = fields.GetProperty("summary").GetString() ?? string.Empty;
        var status    = fields.GetProperty("status").GetProperty("name").GetString() ?? string.Empty;
        var issueType = fields.TryGetProperty("issuetype", out var it)
            ? it.GetProperty("name").GetString()
            : null;
        var assignee = fields.TryGetProperty("assignee", out var a) && a.ValueKind != JsonValueKind.Null
            ? a.GetProperty("displayName").GetString()
            : null;

        return new JiraIssueDto
        {
            Key       = key,
            Summary   = summary,
            Status    = status,
            IssueType = issueType,
            Assignee  = assignee,
            BrowseUrl = $"{baseUrl}/browse/{key}"
        };
    }
}

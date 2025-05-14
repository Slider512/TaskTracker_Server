using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Server.Services
{
    public class JiraService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _authHeader;

        public JiraService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Jira:BaseUrl"];
            var email = configuration["Jira:Email"];
            var apiKey = configuration["Jira:ApiKey"];
            _authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{apiKey}"));
        }

        public async Task<string> GetIssuesAsync(string projectKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/rest/api/3/search?jql=project={projectKey}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task CreateIssueAsync(string projectKey, string summary, string description)
        {
            var payload = new
            {
                fields = new
                {
                    project = new { key = projectKey },
                    summary,
                    description,
                    issuetype = new { name = "Task" }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/rest/api/3/issue")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}

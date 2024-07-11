using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Models.FinTechApi.Responses;
using Newtonsoft.Json;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private string _token;
    private DateTime _tokenExpiry;

    public TokenService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiry)
        {
            await FetchTokenAsync();
        }

        return _token;
    }

    private async Task FetchTokenAsync()
    {
        var tokenUrl = _configuration["ThirdPartyApi:TokenUrl"];
        var username = _configuration["ThirdPartyApi:Username"];
        var password = _configuration["ThirdPartyApi:Password"];

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "app-cli"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        ]);

        request.Content = content;

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<GetTokenResponse>(responseContent);

        _token = tokenResponse.access_token;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
    }
}

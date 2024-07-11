using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Models.Responses;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Magnise_Test_Task.Services
{
    public class FintaTechService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;

        public FintaTechService(IConfiguration configuration, HttpClient httpClient, ITokenService tokenService, ILogger<RealTimePricesService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<string> GetAssetsAsync(
          string provider = null,
          string symbol = null,
          int? page = null,
          int? size = null,
          string kind = null)
        {
            var token = await _tokenService.GetTokenAsync();
            var baseUrl = _configuration["ThirdPartyApi:BaseUrl"];

            var queryParams = new List<string>
            {
                provider != null ? $"provider={provider}" : null,
                symbol != null ? $"symbol={symbol}" : null,
                page.HasValue ? $"page={page.Value}" : null,
                size.HasValue ? $"size={size.Value}" : null,
                kind != null ? $"kind={kind}" : null
            }.Where(param => param != null);

            var queryString = string.Join("&", queryParams);
            var url = $"{baseUrl}/api/instruments/v1/instruments{(string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}")}";

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<PriceResponse> GetPriceAsync(Guid assetId, string provider, int barsCount = 1, int interval = 1, string periodicity = "minute", DateTime? date = null)
        {
            var token = await _tokenService.GetTokenAsync();
            var baseUrl = _configuration["ThirdPartyApi:BaseUrl"];
            var requestUrl = $"{baseUrl}/api/bars/v1/bars/count-back";

            // Create query parameters
            var queryParams = new List<string>
            {
                $"instrumentId={assetId}",
                $"barsCount={barsCount}",
                $"interval={interval}",
                $"periodicity={periodicity}",
                $"provider={provider}"
            };

            if (date.HasValue)
            {
                queryParams.Add($"date={date.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}");
            }

            requestUrl += "?" + string.Join("&", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PriceResponse>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Attempt to get last price info failed. Details: {ex.Message}");
            }

            return new PriceResponse();
        }
    }
}

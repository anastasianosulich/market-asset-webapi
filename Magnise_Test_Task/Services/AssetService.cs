using AutoMapper;
using Magnise_Test_Task.Data;
using Magnise_Test_Task.Data.Entities;
using Magnise_Test_Task.Interfaces;
using Magnise_Test_Task.Models.FinTechApi.Responses;
using Magnise_Test_Task.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Magnise_Test_Task.Services
{
   
    public class AssetService : IAssetService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AssetService> _logger;
        private readonly FintaTechService _fintaTechService;


        public AssetService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, ILogger<AssetService> logger, FintaTechService fintaTechService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _fintaTechService = fintaTechService;
            _logger = logger;
        }

        public async Task UpdateAssetsAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                int currentPage = 1;
                int totalPages;

                do
                {
                    var response = await _fintaTechService.GetAssetsAsync(page: currentPage);
                    var apiResponse = JsonConvert.DeserializeObject<AssetApiResponse>(response);
                    totalPages = apiResponse.Paging.Pages;
                    var assets = apiResponse.Data;
                    await StoreAssetsAsync(assets);

                    currentPage++;
                } while (currentPage <= totalPages);

                _logger.LogInformation("All assets fetched and stored successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching and storing assets.");
            }
        }

        private async Task StoreAssetsAsync(List<Models.FinTechApi.Responses.Asset> assets)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                foreach (var assetResponse in assets)
                {
                    var assetId = Guid.Parse(assetResponse.Id);
                    var existingAsset = await dbContext.Assets
                        .Include(a => a.Mappings)
                        .FirstOrDefaultAsync(a => a.Id == assetId);

                    if (existingAsset != null)
                    {
                        // Update existing asset
                        mapper.Map(assetResponse, existingAsset);

                        // Update mappings
                        foreach (var mapping in assetResponse.Mappings)
                        {
                            var provider = mapping.Key;

                            var existingMapping = existingAsset.Mappings
                                .FirstOrDefault(m => m.Provider == provider);

                            if (existingMapping != null)
                            {
                                // Update existing mapping
                                mapper.Map(mapping.Value, existingMapping, opts => opts.Items["Provider"] = provider);
                            }
                            else
                            {
                                // Add new mapping
                                var assetMapping = mapper.Map<AssetProviderMapping>(mapping.Value, opts => opts.Items["Provider"] = provider);
                                assetMapping.AssetId = existingAsset.Id;
                                existingAsset.Mappings.Add(assetMapping);
                            }
                        }
                    }
                    else
                    {
                        // Add new asset
                        var newAsset = mapper.Map<Data.Entities.Asset>(assetResponse);
                        newAsset.Id = assetId;

                        foreach (var mapping in assetResponse.Mappings)
                        {
                            var provider = mapping.Key;
                            var assetMapping = mapper.Map<AssetProviderMapping>(mapping.Value, opts => opts.Items["Provider"] = provider);
                            assetMapping.AssetId = newAsset.Id;
                            newAsset.Mappings.Add(assetMapping);
                        }

                        dbContext.Assets.Add(newAsset);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<List<PriceResponse>> GetAssetPriceAsync(List<Guid> assetIds, string? provider = null)
        {
            var priceResponses = new List<PriceResponse>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MarketDbContext>();

                var prices = await dbContext.Prices
                    .Include(p => p.AssetProviderMapping)
                    .Where(p => assetIds.Contains(p.AssetProviderMapping.AssetId) && (string.IsNullOrEmpty(provider) || p.AssetProviderMapping.Provider == provider))
                    .GroupBy(p => new { p.AssetProviderMapping.AssetId, p.AssetProviderMapping.Provider })
                    .Select(g => g.OrderByDescending(p => p.Timestamp).FirstOrDefault())
                    .ToListAsync();

                foreach (var price in prices)
                {
                    priceResponses.Add(new PriceResponse
                    {
                        Value = price.Value,
                        Timestamp = price.Timestamp,
                        Provider = price.AssetProviderMapping.Provider,
                        AssetId = price.AssetProviderMapping.AssetId
                    });
                }

                var missingAssets = assetIds.Except(prices.Select(p => p.AssetProviderMapping.AssetId)).ToList();
                foreach (var assetId in missingAssets)
                {
                    var assetMappings = await dbContext.AssetProviderMappings.Where(m => m.AssetId == assetId && (string.IsNullOrEmpty(provider) || m.Provider == provider)).ToListAsync();

                    foreach (var mapping in assetMappings)
                    {
                        try
                        {
                            var priceResponse = await _fintaTechService.GetPriceAsync(assetId, mapping.Provider);

                            if (priceResponse != null)
                            {
                                var price = new Price
                                {
                                    AssetProviderMappingId = mapping.Id,
                                    Value = priceResponse.Value,
                                    Timestamp = priceResponse.Timestamp
                                };

                                dbContext.Prices.Add(price);
                                await dbContext.SaveChangesAsync();

                                priceResponses.Add(new PriceResponse
                                {
                                    Value = priceResponse.Value,
                                    Timestamp = priceResponse.Timestamp,
                                    Provider = mapping.Provider,
                                    AssetId = assetId
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error while trying to get last price info from 3rd party API. \nDetails: {ex.Message}");
                        }

                    }
                }

                return priceResponses;
            }
        }
    }

}


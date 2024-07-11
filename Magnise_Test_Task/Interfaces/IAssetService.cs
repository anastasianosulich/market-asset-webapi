using Magnise_Test_Task.Models.Responses;

namespace Magnise_Test_Task.Interfaces
{
    public interface IAssetService
    {
        Task<List<PriceResponse>> GetAssetPriceAsync(List<Guid> assetIds, string? provider = null);
        Task UpdateAssetsAsync();
    }
}
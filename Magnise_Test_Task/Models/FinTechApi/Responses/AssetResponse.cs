using Magnise_Test_Task.Data.Entities;

namespace Magnise_Test_Task.Models.FinTechApi.Responses
{
    public class AssetApiResponse
    {
        public PagingInfo Paging { get; set; }
        public List<Asset> Data { get; set; }
    }
    public class AssetPriceResponse
    {
        public PagingInfo PagingInfo { get; set; }
        public List<Asset> Assets { get; set; }
    }

    public class PagingInfo
    {
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Items { get; set; }
    }

    public class Asset
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string Kind { get; set; }
        public string Exchange { get; set; }
        public string Description { get; set; }
        public decimal TickSize { get; set; }
        public string Currency { get; set; }
        public string BaseCurrency { get; set; }
        public Dictionary<string, AssetProviderMappingResponse> Mappings { get; set; }
    }

    public class AssetProviderMappingResponse
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public int DefaultOrderSize { get; set; }
    }
}

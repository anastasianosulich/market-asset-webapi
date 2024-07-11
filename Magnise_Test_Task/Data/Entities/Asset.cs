namespace Magnise_Test_Task.Data.Entities
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public string Kind { get; set; }
        public string? Exchange { get; set; }
        public string Description { get; set; }
        public decimal TickSize { get; set; }
        public string Currency { get; set; }
        public string? BaseCurrency { get; set; }
        public List<AssetProviderMapping> Mappings { get; set; } = new List<AssetProviderMapping>();
    }
}

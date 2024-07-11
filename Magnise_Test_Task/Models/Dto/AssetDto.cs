namespace Magnise_Test_Task.Models.Dto
{
    public class AssetDTO
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public string Kind { get; set; }
        public string Exchange { get; set; }
        public string Description { get; set; }
        public decimal TickSize { get; set; }
        public string Currency { get; set; }
        public string BaseCurrency { get; set; }
        public List<AssetProviderMappingDto> Mappings { get; set; }
    }
}

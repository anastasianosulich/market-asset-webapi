namespace Magnise_Test_Task.Data.Entities
{
    public class Price
    {
        public int Id { get; set; }
        public int AssetProviderMappingId { get; set; }
        public AssetProviderMapping AssetProviderMapping { get; set; }
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

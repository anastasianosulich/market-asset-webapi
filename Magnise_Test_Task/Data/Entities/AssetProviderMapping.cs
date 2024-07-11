namespace Magnise_Test_Task.Data.Entities
{
    public class AssetProviderMapping
    {
        public int Id { get; set; }
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; }
        public string Provider { get; set; }
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public int DefaultOrderSize { get; set; }
        public Price Price { get; set; }
    }
}

namespace Magnise_Test_Task.Models.Responses
{
    public class PriceResponse
    {
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string Provider { get; set; }
        public Guid AssetId { get; set; }

    }
}

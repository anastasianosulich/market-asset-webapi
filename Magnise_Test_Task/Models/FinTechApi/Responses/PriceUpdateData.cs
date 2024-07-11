namespace Magnise_Test_Task.Models.FinTechApi.Responses
{
    public class PriceUpdateData
    {
        public string Type { get; set; }
        public string Symbol { get; set; }
        public string Provider { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
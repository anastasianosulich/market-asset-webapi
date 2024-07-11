namespace Magnise_Test_Task.Models.FinTechApi.Responses
{
    public class Last
    {
        public DateTime timestamp { get; set; }
        public decimal price { get; set; }
        public decimal volume { get; set; }
        public decimal change { get; set; }
        public decimal changePct { get; set; }
    }
}

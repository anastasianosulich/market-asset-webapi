namespace Magnise_Test_Task.Models.FinTechApi.Responses
{
    public class L1UpdateMessage
    {
        public string Type { get; set; }
        public string InstrumentId { get; set; }
        public string Provider { get; set; }
        public Last Last { get; set; }
    }
}

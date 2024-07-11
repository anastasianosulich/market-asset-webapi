namespace Magnise_Test_Task.Models.FinTechApi.Responses
{
    public class PriceInfo
    {
        public DateTime t { get; set; }
        public double o { get; set; }
        public double h { get; set; }
        public double l { get; set; }
        public double c { get; set; }
        public int v { get; set; }
    }

    public class PriceApiResponse
    {
        public List<PriceInfo> PriceInfos { get; set; }
    }
}

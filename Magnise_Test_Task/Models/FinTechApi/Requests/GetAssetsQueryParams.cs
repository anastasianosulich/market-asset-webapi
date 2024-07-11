namespace Magnise_Test_Task.Models.FinTechApi.Requests
{
    public class GetAssetsQueryParams
    {
        public string? provider { get; set; }
        public string? kind { get; set; }
        public string? symbol { get; set; }
        public int? page { get; set; }
        public int? size { get; set; }
    }
}

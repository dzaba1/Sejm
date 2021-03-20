namespace Dzaba.Sejm.DataHarvest
{
    public sealed class SejmCrawlerOptions
    {
        public bool SearchDeputies { get; set; } = true;
        public bool SearchTermOfServices { get; set; } = true;
    }
}

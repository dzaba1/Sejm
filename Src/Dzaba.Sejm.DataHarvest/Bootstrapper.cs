using Dzaba.Sejm.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.Sejm.DataHarvest
{
    public static class Bootstrapper
    {
        public static void RegisterSejmDataHarvest(this IServiceCollection services)
        {
            Require.NotNull(services, nameof(services));

            services.AddTransient<ISejmCrawler, SejmCrawler>();
            services.AddTransient<IPageRequesterWrap, PageRequesterWrap>();
            services.AddTransient<IArchiwumCrawler, ArchiwumCrawler>();
            services.AddTransient<IOrkaPoliticiansCrawler, OrkaPoliticiansCrawler>();
            services.AddTransient<IPoliticiansCrawlerManager, PoliticiansCrawlerManager>();
        }
    }
}

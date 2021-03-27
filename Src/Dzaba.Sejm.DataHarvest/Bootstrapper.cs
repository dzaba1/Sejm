using Dzaba.Sejm.DataHarvest.Deputies;
using Dzaba.Sejm.DataHarvest.Orka;
using Dzaba.Sejm.DataHarvest.Xsf;
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
            services.AddTransient<IArchiwumCrawler, ArchiwumCrawler>();
            services.AddTransient<IDeputiesCrawler, OrkaDeputiesCrawler>();
            services.AddTransient<IDeputiesCrawlerManager, DeputiesCrawlerManager>();
            services.AddTransient<IDeputyCrawler, OrkaDeputyCrawler>();
            services.AddTransient<IDeputiesCrawler, XsfDeputiesCrawler>();
            services.AddTransient<IDeputyCrawler, XsfDeputyCrawler>();
        }
    }
}

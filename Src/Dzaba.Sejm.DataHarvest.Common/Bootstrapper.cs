using Dzaba.Sejm.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.Sejm.DataHarvest.Common
{
    public static class Bootstrapper
    {
        public static void RegisterSejmDataHarvestCommon(this IServiceCollection services)
        {
            Require.NotNull(services, nameof(services));

            services.AddTransient<IPageRequesterWrap, PageRequesterWrap>();
            services.AddSingleton<IPageRequesterManager, PageRequesterManager>();
        }
    }
}

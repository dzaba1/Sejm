using Dzaba.Sejm.DataHarvest;
using Dzaba.Sejm.DataHarvest.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using System;
using System.IO;

namespace Dzaba.Sejm.IntegrationTests
{
    public class IntegrationTestFixture
    {
        private ServiceProvider container;

        protected IServiceProvider Container => container;

        [SetUp]
        public void SetupContainer()
        {
            var services = new ServiceCollection();
            services.RegisterSejmDataHarvest();
            services.RegisterSejmDataHarvestCommon();

            RegisterLogging(services);

            services.AddSingleton<IPageRequestSettings>(new PageRequestSettings
            {
                Host = "www.sejm.gov.pl",
                DelayBetweenCalls = TimeSpan.FromSeconds(3),
                Retires = 3,
                RetryWaitTime = TimeSpan.FromSeconds(2)
            });
            services.AddSingleton<IPageRequestSettings>(new PageRequestSettings
            {
                Host = "orka.sejm.gov.pl",
                DelayBetweenCalls = TimeSpan.FromSeconds(3),
                Retires = 3,
                RetryWaitTime = TimeSpan.FromSeconds(2)
            });

            container = services.BuildServiceProvider();
        }

        private void RegisterLogging(IServiceCollection services)
        {
            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IntegrationTests.log");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(filename)
                .CreateLogger();

            services.AddLogging(b => b.AddSerilog(logger, true));
        }

        [TearDown]
        public void CleanupContainer()
        {
            container?.Dispose();
        }
    }
}

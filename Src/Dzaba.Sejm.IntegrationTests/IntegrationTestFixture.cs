using Dzaba.Sejm.DataHarvest;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using System;

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

            RegisterLogging(services);

            container = services.BuildServiceProvider();
        }

        private void RegisterLogging(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
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

using Dzaba.Sejm.DataHarvest;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
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

            container = services.BuildServiceProvider();
        }

        [TearDown]
        public void CleanupContainer()
        {
            container?.Dispose();
        }
    }
}

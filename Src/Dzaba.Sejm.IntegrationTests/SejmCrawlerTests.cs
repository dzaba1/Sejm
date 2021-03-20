using Dzaba.Sejm.DataHarvest;
using Dzaba.Sejm.DataHarvest.Model;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dzaba.Sejm.IntegrationTests
{
    [TestFixture]
    public class SejmCrawlerTests : IntegrationTestFixture
    {
        private ISejmCrawler CreateSut()
        {
            return Container.GetRequiredService<ISejmCrawler>();
        }

        [Test]
        public async Task CrawlAsync_WhenInvoked_ItFindsTermsOfOffice()
        {
            var terms = new List<TermOfOffice>();

            var sut = CreateSut();
            sut.TermOfOfficeFound += t => terms.Add(t);

            var options = new SejmCrawlerOptions
            {
                SearchDeputies = false
            };

            await sut.CrawlAsync(new Uri("https://www.sejm.gov.pl/"), options)
                .ConfigureAwait(false);

            terms.Should().HaveCountGreaterOrEqualTo(10);
        }

        [Test]
        public async Task CrawlAsync_WhenInvoked_ItFindsDeputies()
        {
            var deputies = new List<Deputy>();

            var sut = CreateSut();
            sut.DeputyFound += t => deputies.Add(t);

            var options = new SejmCrawlerOptions
            {
                SearchTermOfServices = false
            };

            await sut.CrawlAsync(new Uri("https://www.sejm.gov.pl/"), options)
                .ConfigureAwait(false);

            deputies.Should().HaveCountGreaterOrEqualTo(10);
        }
    }
}

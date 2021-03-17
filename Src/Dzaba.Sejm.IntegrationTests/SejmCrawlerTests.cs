using Dzaba.Sejm.DataHarvest;
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

            await sut.CrawlAsync(new Uri("https://www.sejm.gov.pl/")).ConfigureAwait(false);

            terms.Should().HaveCountGreaterOrEqualTo(10);
        }
    }
}

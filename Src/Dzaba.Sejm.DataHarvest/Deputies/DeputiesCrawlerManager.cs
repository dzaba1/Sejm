using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Deputies
{
    internal interface IDeputiesCrawlerManager
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class DeputiesCrawlerManager : IDeputiesCrawlerManager
    {
        private readonly IDeputiesCrawler[] deputiesCrawlers;
        private readonly ILogger<DeputiesCrawlerManager> logger;

        public DeputiesCrawlerManager(IEnumerable<IDeputiesCrawler> deputiesCrawlers,
            ILogger<DeputiesCrawlerManager> logger)
        {
            Require.NotNull(deputiesCrawlers, nameof(deputiesCrawlers));
            Require.NotNull(logger, nameof(logger));

            this.deputiesCrawlers = deputiesCrawlers.ToArray();
            this.logger = logger;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            var crawler = deputiesCrawlers.FirstOrDefault(d => d.IsMatch(url));
            if (crawler == null)
            {
                logger.LogWarning("Couldn't match correct crawler for url {Url}", url);
            }
            else
            {
                await crawler.CrawlAsync(url, termOfOffice, data)
                    .ConfigureAwait(false);
            }
        }
    }
}

using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Deputies
{
    internal interface IDeputyCrawlerManager
    {
        Task<Deputy> CrawlAsync(Uri url, TermOfOffice termOfOffice);
    }

    internal sealed class DeputyCrawlerManager : IDeputyCrawlerManager
    {
        private readonly IDeputyCrawler[] deputyCrawlers;
        private readonly ILogger<DeputyCrawlerManager> logger;

        public DeputyCrawlerManager(IEnumerable<IDeputyCrawler> deputyCrawlers,
            ILogger<DeputyCrawlerManager> logger)
        {
            Require.NotNull(deputyCrawlers, nameof(deputyCrawlers));
            Require.NotNull(logger, nameof(logger));

            this.deputyCrawlers = deputyCrawlers.ToArray();
            this.logger = logger;
        }

        public async Task<Deputy> CrawlAsync(Uri url, TermOfOffice termOfOffice)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));

            var crawler = deputyCrawlers.FirstOrDefault(d => d.IsMatch(url));
            if (crawler == null)
            {
                logger.LogWarning("Couldn't match correct crawler for url {Url}", url);
                return null;
            }

            return await crawler.CrawlAsync(url, termOfOffice)
                    .ConfigureAwait(false);
        }
    }
}

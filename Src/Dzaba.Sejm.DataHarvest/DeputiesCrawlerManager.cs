using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    internal interface IDeputiesCrawlerManager
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class DeputiesCrawlerManager : IDeputiesCrawlerManager
    {
        private readonly IOrkaDeputiesCrawler orkaPoliticiansCrawler;

        public DeputiesCrawlerManager(IOrkaDeputiesCrawler orkaPoliticiansCrawler)
        {
            Require.NotNull(orkaPoliticiansCrawler, nameof(orkaPoliticiansCrawler));

            this.orkaPoliticiansCrawler = orkaPoliticiansCrawler;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            if (url.Host == "orka.sejm.gov.pl")
            {
                await orkaPoliticiansCrawler.CrawlAsync(url, termOfOffice, data)
                    .ConfigureAwait(false);
            }
        }
    }
}

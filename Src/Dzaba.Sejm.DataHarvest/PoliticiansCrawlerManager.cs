using Dzaba.Sejm.Utils;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    internal interface IPoliticiansCrawlerManager
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class PoliticiansCrawlerManager : IPoliticiansCrawlerManager
    {
        private readonly IOrkaPoliticiansCrawler orkaPoliticiansCrawler;

        public PoliticiansCrawlerManager(IOrkaPoliticiansCrawler orkaPoliticiansCrawler)
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

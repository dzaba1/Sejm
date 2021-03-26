using Dzaba.Sejm.DataHarvest.Common;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Xsf
{
    internal interface IXsfDeputyCrawler
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class XsfDeputyCrawler : IXsfDeputyCrawler
    {
        private readonly ILogger<XsfDeputyCrawler> logger;
        private readonly IPageRequesterWrap pageRequester;

        public XsfDeputyCrawler(ILogger<XsfDeputyCrawler> logger,
            IPageRequesterWrap pageRequester)
        {
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(pageRequester, nameof(pageRequester));

            this.logger = logger;
            this.pageRequester = pageRequester;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            logger.LogInformation("Start xsf deputy {Url}.", url);
            var perfWatch = Stopwatch.StartNew();

            var page = await pageRequester.MakeRequestAsync(url)
                .ConfigureAwait(false);
            var document = page.AngleSharpHtmlDocument;

            logger.LogInformation("Crawling Orka deputy {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }
    }
}

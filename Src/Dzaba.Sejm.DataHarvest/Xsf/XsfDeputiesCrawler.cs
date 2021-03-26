using AngleSharp.Html.Dom;
using Dzaba.Sejm.DataHarvest.Common;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Xsf
{
    internal sealed class XsfDeputiesCrawler : IDeputiesCrawler
    {
        private static readonly Regex DeputiesXsfRegex = new Regex(@"\/?sejm\d+\.nsf\/poslowie\.xsp");

        private readonly ILogger<XsfDeputiesCrawler> logger;
        private readonly IPageRequesterWrap pageRequester;
        private readonly IXsfDeputyCrawler deputyCrawler;

        public XsfDeputiesCrawler(ILogger<XsfDeputiesCrawler> logger,
            IPageRequesterWrap pageRequester,
            IXsfDeputyCrawler deputyCrawler)
        {
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(deputyCrawler, nameof(deputyCrawler));

            this.logger = logger;
            this.pageRequester = pageRequester;
            this.deputyCrawler = deputyCrawler;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            logger.LogInformation("Start xsf deputies {Url}.", url);
            var perfWatch = Stopwatch.StartNew();

            var urls = await GetDeputiesUrlsAsync(url)
                .ConfigureAwait(false);

            await ProcessListAsync(urls, termOfOffice, data)
                .ConfigureAwait(false);

            logger.LogInformation("Crawling xsf deputies {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }

        private async Task ProcessListAsync(IEnumerable<Uri> urls, TermOfOffice termOfOffice, CrawlData data)
        {
            foreach (var deputyUrl in urls)
            {
                await deputyCrawler.CrawlAsync(deputyUrl, termOfOffice, data)
                    .ConfigureAwait(false);
            }
        }

        private async Task<IReadOnlyList<Uri>> GetDeputiesUrlsAsync(Uri url)
        {
            var page = await pageRequester.MakeRequestAsync(url)
                .ConfigureAwait(false);
            var document = page.AngleSharpHtmlDocument;

            var divs = document.QuerySelectorAll("div.deputyName");
            var hostUrl = url.GetHostUri();
            var hrefs = divs
                .Select(d => (IHtmlAnchorElement)d.ParentElement)
                .Select(a => new Uri(a.Href).ToLocalRelativePath())
                .Select(u => new Uri(hostUrl, u));

            return hrefs.ToArray();
        }

        public bool IsMatch(Uri url)
        {
            Require.NotNull(url, nameof(url));

            return DeputiesXsfRegex.IsMatch(url.AbsolutePath);
        }
    }
}

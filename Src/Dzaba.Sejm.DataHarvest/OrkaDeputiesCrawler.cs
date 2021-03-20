using AngleSharp.Html.Dom;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    internal interface IOrkaDeputiesCrawler
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class OrkaDeputiesCrawler : IOrkaDeputiesCrawler
    {
        private readonly IPageRequesterWrap pageRequester;
        private readonly ILogger<OrkaDeputiesCrawler> logger;
        private readonly IOrkaDeputyCrawler deputyCrawler;

        public OrkaDeputiesCrawler(IPageRequesterWrap pageRequester,
            ILogger<OrkaDeputiesCrawler> logger,
            IOrkaDeputyCrawler deputyCrawler)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(deputyCrawler, nameof(deputyCrawler));

            this.pageRequester = pageRequester;
            this.logger = logger;
            this.deputyCrawler = deputyCrawler;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            logger.LogInformation("Start Orka deputies {Url}.", url);
            var perfWatch = Stopwatch.StartNew();

            var listUrl = await GetListUrlAsync(url)
                .ConfigureAwait(false);
            
            await ProcessListAsync(listUrl, termOfOffice, data)
                .ConfigureAwait(false);

            logger.LogInformation("Crawling Orka deputies {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }

        private async Task ProcessListAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            var urls = await GetDeputiesUrlsAsync(url)
                .ConfigureAwait(false);

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
            var table = document.All
                .OfType<IHtmlTableElement>()
                .First();
            var tableBody = table.Bodies.First();
            var cells = tableBody.Rows
                .Select(r => r.Cells.First());

            var hostUrl = url.GetHostUri();
            var uris = cells
                .SelectMany(c => c.Children)
                .OfType<IHtmlAnchorElement>()
                .Select(a => new Uri(a.Href));
            
            return uris
                .Select(a => new Uri(hostUrl, a.ToLocalRelativePath()))
                .ToArray();
        }

        private async Task<Uri> GetListUrlAsync(Uri url)
        {
            var currentUrl = url;
            var page = await pageRequester.MakeRequestAsync(currentUrl)
                .ConfigureAwait(false);
            var document = page.AngleSharpHtmlDocument;

            var frames = document.All
                .Where(e => e.LocalName == "frame")
                .Select(e => new { Element = e, Source = e.GetAttribute("src") });

            var frame = frames
                .First(e => e.Source.Contains("PoslowieKad"));

            var hostUri = url.GetHostUri();
            currentUrl = new Uri(hostUri, frame.Source);
            page = await pageRequester.MakeRequestAsync(currentUrl)
                .ConfigureAwait(false);
            document = page.AngleSharpHtmlDocument;

            var iframe = document.All
                .OfType<IHtmlInlineFrameElement>()
                .First();
            var source = new Uri(iframe.Source);

            var split = currentUrl.SplitAbsolutePath();
            var baseUri = new Uri(hostUri, split.First() + "/");
            var localRelative = source.ToLocalRelativePath();

            return new Uri(baseUri, localRelative);
        }
    }
}

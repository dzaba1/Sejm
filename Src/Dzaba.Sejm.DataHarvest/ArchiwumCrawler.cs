using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    internal interface IArchiwumCrawler
    {
        Task CrawlAsync(Uri url, CrawlData data);
    }

    internal sealed class ArchiwumCrawler : IArchiwumCrawler
    {
        private static readonly Regex TermOfServiceNameRegex = new Regex(@"(?<Name>\w+\s\w+)\s(?<From>\d{4})-(?<To>\d{4})", RegexOptions.IgnoreCase);

        private readonly IPageRequesterWrap pageRequester;
        private readonly ILogger<ArchiwumCrawler> logger;

        public ArchiwumCrawler(IPageRequesterWrap pageRequester,
            ILogger<ArchiwumCrawler> logger)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(logger, nameof(logger));

            this.pageRequester = pageRequester;
            this.logger = logger;
        }

        public async Task CrawlAsync(Uri url, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(data, nameof(data));

            logger.LogInformation("Start crawling Archiwum {Url}.", url);
            var perfWatch = Stopwatch.StartNew();

            var archPage = await pageRequester.MakeRequestAsync(url)
                .ConfigureAwait(false);
            var document = archPage.AngleSharpHtmlDocument;

            var listRoot = document.All
                .OfType<IHtmlUnorderedListElement>()
                .FirstOrDefault(e => e.LocalName == "ul" && e.ClassName == "komisje-sledcze-bold");
            var toProcess = listRoot.Children
                .Where(IsTermOfServiceItem);

            var tasks = new List<Task>();
            foreach (var element in toProcess)
            {
                var task = ProcessArchiwumTermOfOffice(url, element, data);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            logger.LogInformation("Crawling Archiwum {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }

        private async Task ProcessArchiwumTermOfOffice(Uri archUrl, IElement element, CrawlData data)
        {
            HarvestTermOfOffice(archUrl, element, data);
        }

        private void HarvestTermOfOffice(Uri archUrl, IElement element, CrawlData data)
        {
            var strong = element.QuerySelector("strong");
            var termOfService = ParseArchiwumTermOfService(strong.TextContent);
            var anchorParent = strong.ParentElement as IHtmlAnchorElement;
            if (anchorParent != null)
            {
                termOfService.Url = new Uri(anchorParent.Href);
            }
            else
            {
                termOfService.Url = archUrl;
            }

            data.DataNotifier.NewTermOfOfficeFound(termOfService);
        }

        private TermOfOffice ParseArchiwumTermOfService(string name)
        {
            var matches = TermOfServiceNameRegex.Match(name);
            return new TermOfOffice
            {
                Name = matches.Groups["Name"].Value,
                From = short.Parse(matches.Groups["From"].Value),
                To = short.Parse(matches.Groups["To"].Value)
            };
        }

        private bool IsTermOfServiceItem(IElement item)
        {
            var strongs = item.QuerySelectorAll("strong");
            return strongs.Any(s => s.InnerHtml.Contains("kadencja"));
        }
    }
}

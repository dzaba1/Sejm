using AngleSharp.Html.Dom;
using Dzaba.Sejm.DataHarvest.Common;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    public interface ISejmCrawler
    {
        Task CrawlAsync(Uri root, SejmCrawlerOptions options);

        event Action<TermOfOffice> TermOfOfficeFound;
        event Action<Deputy> DeputyFound;
    }

    internal sealed class SejmCrawler : ISejmCrawler, IDataNotifier
    {
        private readonly IPageRequesterWrap pageRequester;
        private readonly ILogger<SejmCrawler> logger;
        private readonly IArchiwumCrawler archiwumCrawler;

        public SejmCrawler(IPageRequesterWrap pageRequester,
            ILogger<SejmCrawler> logger,
            IArchiwumCrawler archiwumCrawler)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(archiwumCrawler, nameof(archiwumCrawler));

            this.pageRequester = pageRequester;
            this.logger = logger;
            this.archiwumCrawler = archiwumCrawler;
        }

        public event Action<TermOfOffice> TermOfOfficeFound;
        public event Action<Deputy> DeputyFound;

        public async Task CrawlAsync(Uri root, SejmCrawlerOptions options)
        {
            Require.NotNull(root, nameof(root));
            Require.NotNull(options, nameof(options));

            logger.LogInformation("Start crawling {Url}.", root);
            var perfWatch = Stopwatch.StartNew();

            var mainPage = await pageRequester.MakeRequestAsync(root)
                .ConfigureAwait(false);

            var document = mainPage.AngleSharpHtmlDocument;

            await CrawlMainPageAsync(root, document, options)
                .ConfigureAwait(false);
            logger.LogInformation("Crawling {Url} finished. Took {Elapsed}", root, perfWatch.Elapsed);
        }

        public void NewDeputyFound(Deputy deputy)
        {
            Require.NotNull(deputy, nameof(deputy));

            DeputyFound?.Invoke(deputy);
        }

        public void NewTermOfOfficeFound(TermOfOffice termOfOffice)
        {
            Require.NotNull(termOfOffice, nameof(termOfOffice));

            TermOfOfficeFound?.Invoke(termOfOffice);
        }

        private async Task CrawlMainPageAsync(Uri root, IHtmlDocument document, SejmCrawlerOptions options)
        {
            var currentTermOfOffice = new TermOfOffice
            {
                Name = GetNameFromPage(document),
                Url = root
            };

            if (options.SearchTermOfServices)
            {
                NewTermOfOfficeFound(currentTermOfOffice);
            }

            var crawlData = new CrawlData(this, root, options);

            var archUrl = GetArchiwumUrl(root, document);
            var archTask = archiwumCrawler.CrawlAsync(archUrl, crawlData);

            await archTask.ConfigureAwait(false);
        }

        private Uri GetArchiwumUrl(Uri root, IHtmlDocument document)
        {
            var arch = document.All
                .OfType<IHtmlAnchorElement>()
                .First(x => x.LocalName == "a" && x.InnerHtml == "Archiwum");

            return new Uri(root, arch.PathName);
        }

        private string GetNameFromPage(IHtmlDocument document)
        {
            var el = document.All
                .FirstOrDefault(e => e.LocalName == "span" && e.ClassName == "kadencja");

            return el.InnerHtml;
        }
    }
}

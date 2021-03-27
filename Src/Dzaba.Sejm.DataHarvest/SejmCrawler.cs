using AngleSharp.Html.Dom;
using Dzaba.Sejm.DataHarvest.Common;
using Dzaba.Sejm.DataHarvest.Deputies;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static readonly Regex SejmIdRegex = new Regex(@"Sejm(?<Id>\d+)\.nsf", RegexOptions.IgnoreCase);

        private readonly IPageRequesterWrap pageRequester;
        private readonly ILogger<SejmCrawler> logger;
        private readonly IArchiwumCrawler archiwumCrawler;
        private readonly IDeputiesCrawlerManager deputiesCrawlerManager;

        public SejmCrawler(IPageRequesterWrap pageRequester,
            ILogger<SejmCrawler> logger,
            IArchiwumCrawler archiwumCrawler,
            IDeputiesCrawlerManager deputiesCrawlerManager)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(archiwumCrawler, nameof(archiwumCrawler));
            Require.NotNull(deputiesCrawlerManager, nameof(deputiesCrawlerManager));

            this.pageRequester = pageRequester;
            this.logger = logger;
            this.archiwumCrawler = archiwumCrawler;
            this.deputiesCrawlerManager = deputiesCrawlerManager;
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
                Name = GetNameFromPage(root, document),
                Url = root
            };

            if (options.SearchTermOfServices)
            {
                NewTermOfOfficeFound(currentTermOfOffice);
            }

            var crawlData = new CrawlData(this, root, options);

            if (!options.SkipArchiwum)
            {
                await ProcessArchiwumAsync(root, document, crawlData)
                    .ConfigureAwait(false);
            }
            else
            {
                logger.LogInformation("Skipping archiwum.");
            }

            if (options.SearchDeputies)
            {
                await ProcessDeputies(document, currentTermOfOffice, crawlData)
                    .ConfigureAwait(false);
            }
            else
            {
                logger.LogInformation("Skipping current deputies.");
            }
        }

        private int GetSejmId(IHtmlDocument document)
        {
            var a = document.All
                .OfType<IHtmlAnchorElement>()
                .First(x => x.LocalName == "a" && x.TextContent == "Strona główna");

            var match = SejmIdRegex.Match(a.Href);
            if (!match.Success)
            {
                throw new InvalidOperationException("Can't find sejm id.");
            }

            return int.Parse(match.Groups["Id"].Value);
        }

        private async Task ProcessDeputies(IHtmlDocument document, TermOfOffice termOfOffice, CrawlData data)
        {
            var id = GetSejmId(document);
            var url = new Uri(data.RootUrl.GetHostUri(), $"https://www.sejm.gov.pl/Sejm{id}.nsf/poslowie.xsp?type=A");

            await deputiesCrawlerManager.CrawlAsync(url, termOfOffice, data)
                .ConfigureAwait(false);
        }

        private async Task ProcessArchiwumAsync(Uri root, IHtmlDocument document, CrawlData crawlData)
        {
            try
            {
                var archUrl = GetArchiwumUrl(root, document);
                var archTask = archiwumCrawler.CrawlAsync(archUrl, crawlData);

                await archTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error crawling Archiwum.");
            }
        }

        private Uri GetArchiwumUrl(Uri root, IHtmlDocument document)
        {
            var arch = document.All
                .OfType<IHtmlAnchorElement>()
                .First(x => x.LocalName == "a" && x.TextContent == "Archiwum");

            return new Uri(root, arch.PathName);
        }

        private string GetNameFromPage(Uri url, IHtmlDocument document)
        {
            try
            {
                var el = document.All
                .FirstOrDefault(e => e.LocalName == "span" && e.ClassName == "kadencja");

                return el.TextContent;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error getting name for a term of service. Url: {Url}", url);
                return null;
            }
        }
    }
}

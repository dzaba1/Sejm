using AngleSharp.Dom;
using Dzaba.Sejm.DataHarvest.Common;
using Dzaba.Sejm.DataHarvest.Model;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Orka
{
    internal interface IOrkaDeputyCrawler
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
    }

    internal sealed class OrkaDeputyCrawler : IOrkaDeputyCrawler
    {
        private static readonly Regex BirthRegex = new Regex(@"^(?<Date>((\d{2})|(-1))-{1}((\d{2})|(-1))-{1}((\d{4})|(-1))),\s(?<City>.+)$");
        private static readonly Regex BirthRegex2 = new Regex(@"^(?<Date>((\d{2})|(-1))-{1}((\d{2})|(-1))-{1}((\d{4})|(-1)))");

        private readonly IPageRequesterWrap pageRequester;
        private readonly ILogger<OrkaDeputiesCrawler> logger;

        public OrkaDeputyCrawler(IPageRequesterWrap pageRequester,
            ILogger<OrkaDeputiesCrawler> logger)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));
            Require.NotNull(logger, nameof(logger));

            this.pageRequester = pageRequester;
            this.logger = logger;
        }

        public async Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data)
        {
            Require.NotNull(url, nameof(url));
            Require.NotNull(termOfOffice, nameof(termOfOffice));
            Require.NotNull(data, nameof(data));

            logger.LogInformation("Start Orka deputy {Url}.", url);
            var perfWatch = Stopwatch.StartNew();

            var page = await pageRequester.MakeRequestAsync(url)
                .ConfigureAwait(false);
            var document = page.AngleSharpHtmlDocument;
            var list = document.QuerySelector("ul.dane1, ul.dane2");

            var deputy = new Deputy
            {
                Name = GetName(list),
                TermOfOffice = termOfOffice,
                Url = url
            };
            SetBirths(list, deputy);
            data.DataNotifier.NewDeputyFound(deputy);

            logger.LogInformation("Crawling Orka deputy {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }

        private string GetName(IElement list)
        {
            var element = list.QuerySelector("p.posel");
            return element.TextContent;
        }

        private bool TryFindBirthElement(IElement list, out Match match)
        {
            var paragraphs = list.QuerySelectorAll("p.right");
            foreach (var p in paragraphs)
            {
                var text = p.TextContent.Trim();
                match = BirthRegex.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = BirthRegex2.Match(text);
                if (match.Success)
                {
                    return true;
                }
            }

            match = null;
            return false;
        }

        private void SetBirths(IElement list, Deputy deputy)
        {
            if (TryFindBirthElement(list, out var match))
            {
                var birthDateText = match.Groups["Date"].Value;
                if (DateTime.TryParseExact(birthDateText, "dd-MM-yyyy", null, DateTimeStyles.None, out var birthDate))
                {
                    deputy.DateOfBirth = birthDate;
                }
                else
                {
                    logger.LogWarning("Couldn't parse date {BirthDate}.", birthDateText);
                }

                if (match.Groups.Count > 1)
                {
                    deputy.PlaceOfBirth = match.Groups["City"].Value;
                }
                else
                {
                    logger.LogWarning("City of birth not found. Url: {Url}", deputy.Url);
                }
            }
            else
            {
                logger.LogWarning("Couldn't match birth. Url: {Url}", deputy.Url);
            }
        }
    }
}

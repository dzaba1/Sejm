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
            var div = document.QuerySelector("#title_content");

            var deputy = new Deputy
            {
                Name = GetName(div),
                TermOfOffice = termOfOffice,
                Url = url
            };
            SetBirths(div, deputy);
            data.DataNotifier.NewDeputyFound(deputy);

            logger.LogInformation("Crawling Orka deputy {Url} finished. Took {Elapsed}", url, perfWatch.Elapsed);
        }

        private void SetBirths(IElement div, Deputy deputy)
        {
            if (TryFindBirthElement(div, out var match))
            {
                var birthDateText = match.Groups["Date"].Value;
                if (DateTime.TryParseExact(birthDateText, "dd-MM-yyyy", null, DateTimeStyles.None, out var birthDate))
                {
                    deputy.DateOfBirth = birthDate;
                }
                else
                {
                    logger.LogWarning("Couldn't parse date {BirthDate}. Url: {Url}", birthDateText, deputy.Url);
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

        private bool TryFindBirthElement(IElement div, out Match match)
        {
            var ur = div.QuerySelector("#urodzony");
            if (ur != null)
            {
                var text = ur.TextContent.Trim();
                match = Consts.DeputyBirthRegex.Match(text);
                if (match.Success)
                {
                    return true;
                }

                match = Consts.DeputyBirthRegex2.Match(text);
                if (match.Success)
                {
                    return true;
                }
            }

            match = null;
            return false;
        }

        private string GetName(IElement div)
        {
            var h = div.QuerySelector("h1");
            return h.TextContent;
        }
    }
}

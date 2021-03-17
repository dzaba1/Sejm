using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dzaba.Sejm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    public interface ISejmCrawler
    {
        Task CrawlAsync(Uri root);

        event Action<TermOfOffice> TermOfOfficeFound;
    }

    internal sealed class SejmCrawler : ISejmCrawler
    {
        private static readonly Regex TermOfServiceNameRegex = new Regex(@"(?<Name>\w+\s\w+)\s(?<From>\d{4})-(?<To>\d{4})", RegexOptions.IgnoreCase);

        private readonly IPageRequesterWrap pageRequester;

        public SejmCrawler(IPageRequesterWrap pageRequester)
        {
            Require.NotNull(pageRequester, nameof(pageRequester));

            this.pageRequester = pageRequester;
        }

        public event Action<TermOfOffice> TermOfOfficeFound;

        public async Task CrawlAsync(Uri root)
        {
            Require.NotNull(root, nameof(root));

            var mainPage = await pageRequester.MakeRequestAsync(root)
                .ConfigureAwait(false);

            var document = mainPage.AngleSharpHtmlDocument;

            await CrawlMainPageAsync(root, document).ConfigureAwait(false);
        }

        private async Task CrawlMainPageAsync(Uri root, IHtmlDocument document)
        {
            var currentTermOfOffice = new TermOfOffice
            {
                Name = GetNameFromPage(document),
                Url = root
            };
            TermOfOfficeFound?.Invoke(currentTermOfOffice);

            var archUrl = GetArchiwumUrl(root, document);
            var archTask = ProcessArchiwum(root, archUrl);

            await archTask.ConfigureAwait(false);
        }

        private async Task ProcessArchiwum(Uri root, Uri archUrl)
        {
            var archPage = await pageRequester.MakeRequestAsync(archUrl)
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
                var task = ProcessArchiwumTermOfOffice(root, archUrl, element);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task ProcessArchiwumTermOfOffice(Uri root, Uri archUrl, IElement element)
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
            TermOfOfficeFound?.Invoke(termOfService);
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

        private Uri GetArchiwumUrl(Uri root, IHtmlDocument document)
        {
            var arch = document.All
                .OfType<IHtmlAnchorElement>()
                .First(x => x.LocalName == "a" && x.InnerHtml == "Archiwum");

            return new Uri(root, arch.PathName);
        }

        public string GetNameFromPage(IHtmlDocument document)
        {
            var el = document.All
                .FirstOrDefault(e => e.LocalName == "span" && e.ClassName == "kadencja");

            return el.InnerHtml;
        }
    }
}

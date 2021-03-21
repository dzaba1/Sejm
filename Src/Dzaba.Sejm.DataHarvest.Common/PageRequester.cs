using Abot2.Core;
using Abot2.Poco;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Common
{
    public interface IPageRequesterWrap
    {
        Task<CrawledPage> MakeRequestAsync(Uri uri);
    }

    internal sealed class PageRequesterWrap : IPageRequesterWrap
    {
        public async Task<CrawledPage> MakeRequestAsync(Uri uri)
        {
            using (var contentExtractor = new WebContentExtractor())
            {
                using (var pageRequester = new PageRequester(new CrawlConfiguration(), contentExtractor))
                {
                    var page = await pageRequester.MakeRequestAsync(uri).ConfigureAwait(false);
                    if (page.HttpRequestException != null)
                    {
                        throw new InvalidOperationException("HTTP error.", page.HttpRequestException);
                    }

                    return page;
                }
            }
        }
    }
}

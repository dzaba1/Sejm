using Abot2.Core;
using Abot2.Poco;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Common
{
    public interface IPageRequesterWrap
    {
        Task<CrawledPage> MakeRequestAsync(Uri url);
    }

    internal sealed class PageRequesterWrap : IPageRequesterWrap
    {
        private readonly IPageRequesterManager pageRequesterManager;
        private readonly ILogger<PageRequesterWrap> logger;

        public PageRequesterWrap(IPageRequesterManager pageRequesterManager,
            ILogger<PageRequesterWrap> logger)
        {
            Require.NotNull(pageRequesterManager, nameof(pageRequesterManager));
            Require.NotNull(logger, nameof(logger));

            this.pageRequesterManager = pageRequesterManager;
            this.logger = logger;
        }

        public async Task<CrawledPage> MakeRequestAsync(Uri url)
        {
            Require.NotNull(url, nameof(url));

            var settings = pageRequesterManager.GetSettings(url);
            var retryCount = 0;

            while (true)
            {
                try
                {
                    return await MakeOneRequestAsync(url)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (retryCount <= settings.Retires)
                    {
                        logger.LogWarning(ex, "Error accessing {Url}", url);
                        retryCount++;
                        await Task.Delay(settings.RetryWaitTime)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task<CrawledPage> MakeOneRequestAsync(Uri uri)
        {
            await pageRequesterManager.WaitForCallAsync(uri)
                .ConfigureAwait(false);

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

using Abot2.Core;
using Abot2.Poco;
using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Common
{
    public interface IPageRequesterWrap
    {
        Task<CrawledPage> MakeRequestAsync(Uri url);
    }

    internal sealed class PageRequesterWrap : IPageRequesterWrap
    {
        private readonly IPageRequesterSettingsProvider settingsProvider;
        private readonly ILogger<PageRequesterWrap> logger;
        private readonly ILastCalls lastCalls;

        public PageRequesterWrap(IPageRequesterSettingsProvider settingsProvider,
            ILogger<PageRequesterWrap> logger,
            ILastCalls lastCalls)
        {
            Require.NotNull(settingsProvider, nameof(settingsProvider));
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(lastCalls, nameof(lastCalls));

            this.settingsProvider = settingsProvider;
            this.logger = logger;
            this.lastCalls = lastCalls;
        }

        public async Task<CrawledPage> MakeRequestAsync(Uri url)
        {
            Require.NotNull(url, nameof(url));

            var settings = settingsProvider.GetSettings(url);
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
            var perfWatch = Stopwatch.StartNew();

            await lastCalls.WaitForCallAsync(uri)
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

                    logger.LogDebug("Request to {Url} took {Elapsed}", uri, perfWatch.Elapsed);
                    return page;
                }
            }
        }
    }
}

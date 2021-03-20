using Dzaba.Sejm.Utils;
using System;

namespace Dzaba.Sejm.DataHarvest
{
    internal sealed class CrawlData
    {
        public CrawlData(IDataNotifier dataNotifier, Uri rootUrl,
            SejmCrawlerOptions options)
        {
            Require.NotNull(dataNotifier, nameof(dataNotifier));
            Require.NotNull(rootUrl, nameof(rootUrl));
            Require.NotNull(options, nameof(options));

            DataNotifier = dataNotifier;
            RootUrl = rootUrl;
            Options = options;
        }

        public IDataNotifier DataNotifier { get; }

        public Uri RootUrl { get; }

        public SejmCrawlerOptions Options { get; }
    }
}

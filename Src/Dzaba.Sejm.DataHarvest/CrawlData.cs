using Dzaba.Sejm.Utils;
using System;

namespace Dzaba.Sejm.DataHarvest
{
    internal sealed class CrawlData
    {
        public CrawlData(IDataNotifier dataNotifier, Uri rootUrl)
        {
            Require.NotNull(dataNotifier, nameof(dataNotifier));
            Require.NotNull(rootUrl, nameof(rootUrl));

            DataNotifier = dataNotifier;
            RootUrl = rootUrl;
        }

        public IDataNotifier DataNotifier { get; }

        public Uri RootUrl { get; }
    }
}

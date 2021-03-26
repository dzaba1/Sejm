using Dzaba.Sejm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.Sejm.DataHarvest.Common
{
    internal interface IPageRequesterSettingsProvider
    {
        IPageRequestSettings GetSettings(Uri url);
    }

    internal sealed class PageRequesterSettingsProvider : IPageRequesterSettingsProvider
    {
        private readonly IReadOnlyDictionary<string, IPageRequestSettings> settings;

        public PageRequesterSettingsProvider(IEnumerable<IPageRequestSettings> settings)
        {
            Require.NotNull(settings, nameof(settings));

            this.settings = settings.ToDictionary(s => s.Host, s => s, StringComparer.OrdinalIgnoreCase);
        }

        public IPageRequestSettings GetSettings(Uri url)
        {
            Require.NotNull(url, nameof(url));

            var host = url.Host;
            if (settings.TryGetValue(host, out var settingsLocal))
            {
                return settingsLocal;
            }

            throw new KeyNotFoundException($"Couldn't find settings for host {host}.");
        }
    }
}

using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Common
{
    internal interface IPageRequesterManager
    {
        IPageRequestSettings GetSettings(Uri url);
        Task WaitForCallAsync(Uri url);
    }

    internal sealed class PageRequesterManager : IPageRequesterManager
    {
        private readonly IReadOnlyDictionary<string, IPageRequestSettings> settings;
        private readonly Dictionary<string, DateTime> lastCalls = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim lastCallsSemaphores = new SemaphoreSlim(1);
        private readonly ILogger<PageRequesterManager> logger;

        public PageRequesterManager(IEnumerable<IPageRequestSettings> settings,
            ILogger<PageRequesterManager> logger)
        {
            Require.NotNull(settings, nameof(settings));
            Require.NotNull(logger, nameof(logger));

            this.settings = settings.ToDictionary(s => s.Host, s => s, StringComparer.OrdinalIgnoreCase);
            this.logger = logger;
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

        public async Task WaitForCallAsync(Uri url)
        {
            Require.NotNull(url, nameof(url));

            var settings = GetSettings(url);

            await lastCallsSemaphores.WaitAsync()
                .ConfigureAwait(false);
            try
            {
                if (lastCalls.TryGetValue(settings.Host, out var lastCall))
                {
                    var currentSpan = DateTime.UtcNow - lastCall;
                    if (currentSpan < settings.DelayBetweenCalls)
                    {
                        var toWait = settings.DelayBetweenCalls - currentSpan;
                        logger.LogDebug("Waiting {Delay} for a next call having host {Host}", toWait, settings.Host);
                        await Task.Delay(toWait)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogDebug("Waiting for a next call having host {Host} is not needed. Current span is {Span}", settings.Host, currentSpan);
                    }
                }
                else
                {
                    lastCall = DateTime.UtcNow;
                    lastCalls.Add(settings.Host, lastCall);
                }
            }
            finally
            {
                lastCallsSemaphores.Release();
            }
        }
    }
}

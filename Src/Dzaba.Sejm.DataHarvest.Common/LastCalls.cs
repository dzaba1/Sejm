using Dzaba.Sejm.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Common
{
    internal interface ILastCalls
    {
        Task WaitForCallAsync(Uri url);
    }

    internal sealed class LastCalls : ILastCalls
    {
        private readonly ILogger<LastCalls> logger;
        private readonly IPageRequesterSettingsProvider settingsProvider;
        private readonly ConcurrentDictionary<string, LockData> locks = new ConcurrentDictionary<string, LockData>(StringComparer.OrdinalIgnoreCase);

        public LastCalls(ILogger<LastCalls> logger,
            IPageRequesterSettingsProvider settingsProvider)
        {
            Require.NotNull(logger, nameof(logger));
            Require.NotNull(settingsProvider, nameof(settingsProvider));

            this.logger = logger;
            this.settingsProvider = settingsProvider;
        }

        public async Task WaitForCallAsync(Uri url)
        {
            Require.NotNull(url, nameof(url));

            var @lock = locks.GetOrAdd(url.Host, h => new LockData());
            var settings = settingsProvider.GetSettings(url);

            await WaitForCallAsync(@lock, settings)
                .ConfigureAwait(false);
        }

        private async Task WaitForCallAsync(LockData @lock, IPageRequestSettings settings)
        {
            await @lock.Semaphore.WaitAsync()
                .ConfigureAwait(false);
            try
            {
                var currentSpan = DateTime.UtcNow - @lock.LastCall;
                if (currentSpan >= settings.DelayBetweenCalls)
                {
                    logger.LogDebug("Waiting for a next call having host {Host} is not needed. Current span is {Span}", settings.Host, currentSpan);
                }
                else
                {
                    var toWait = settings.DelayBetweenCalls - currentSpan;
                    logger.LogDebug("Waiting {Delay} for a next call having host {Host}", toWait, settings.Host);
                    await Task.Delay(toWait)
                        .ConfigureAwait(false);
                }
                @lock.LastCall = DateTime.UtcNow;
            }
            finally
            {
                @lock.Semaphore.Release();
            }
        }

        private class LockData
        {
            public LockData()
            {
                Semaphore = new SemaphoreSlim(1);
                LastCall = DateTime.UtcNow;
            }

            public DateTime LastCall { get; set; }
            public SemaphoreSlim Semaphore { get; }
        }
    }
}

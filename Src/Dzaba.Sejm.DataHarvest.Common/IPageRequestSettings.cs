using System;

namespace Dzaba.Sejm.DataHarvest.Common
{
    public interface IPageRequestSettings
    {
        string Host { get; }
        int Retires { get; }
        TimeSpan RetryWaitTime { get; }
        TimeSpan DelayBetweenCalls { get; }
    }

    public class PageRequestSettings : IPageRequestSettings
    {
        public string Host { get; set; }

        public int Retires { get; set; }

        public TimeSpan RetryWaitTime { get; set; }

        public TimeSpan DelayBetweenCalls { get; set; }
    }
}

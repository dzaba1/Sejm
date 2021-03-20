using System;

namespace Dzaba.Sejm.DataHarvest.Model
{
    public sealed class TermOfOffice
    {
        public string Name { get; internal set; }
        public short? From { get; internal set; }
        public short? To { get; internal set; }
        public Uri Url { get; internal set; }
    }
}

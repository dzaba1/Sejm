using System;

namespace Dzaba.Sejm.DataHarvest
{
    public sealed class TermOfOffice
    {
        public string Name { get; set; }
        public short? From { get; set; }
        public short? To { get; set; }
        public Uri Url { get; set; }
    }
}

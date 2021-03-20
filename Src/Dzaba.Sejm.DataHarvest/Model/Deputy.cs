using System;

namespace Dzaba.Sejm.DataHarvest.Model
{
    public sealed class Deputy
    {
        public string Name { get; internal set; }
        public DateTime? DateOfBirth { get; internal set; }
        public string PlaceOfBirth { get; internal set; }
        public Uri Url { get; internal set; }
        public TermOfOffice TermOfOffice { get; internal set; }
    }
}

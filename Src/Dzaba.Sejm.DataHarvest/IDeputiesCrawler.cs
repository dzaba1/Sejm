using Dzaba.Sejm.DataHarvest.Model;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest
{
    internal interface IDeputiesCrawler
    {
        Task CrawlAsync(Uri url, TermOfOffice termOfOffice, CrawlData data);
        bool IsMatch(Uri url);
    }
}

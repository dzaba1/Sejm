using Dzaba.Sejm.DataHarvest.Model;
using System;
using System.Threading.Tasks;

namespace Dzaba.Sejm.DataHarvest.Deputies
{
    internal interface IDeputyCrawler
    {
        Task<Deputy> CrawlAsync(Uri url, TermOfOffice termOfOffice);
        bool IsMatch(Uri url);
    }
}

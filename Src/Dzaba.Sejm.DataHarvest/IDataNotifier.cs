using Dzaba.Sejm.DataHarvest.Model;

namespace Dzaba.Sejm.DataHarvest
{
    public interface IDataNotifier
    {
        void NewTermOfOfficeFound(TermOfOffice termOfOffice);
        void NewDeputyFound(Deputy deputy);
    }
}

namespace Dzaba.Sejm.DataHarvest
{
    public interface IDataNotifier
    {
        void NewTermOfOfficeFound(TermOfOffice termOfOffice);
    }
}

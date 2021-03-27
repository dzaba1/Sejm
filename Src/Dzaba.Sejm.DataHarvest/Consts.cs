using System.Text.RegularExpressions;

namespace Dzaba.Sejm.DataHarvest
{
    public static class Consts
    {
        public static readonly Regex DeputyBirthRegex = new Regex(@"^(?<Date>((\d{2})|(-1))-{1}((\d{2})|(-1))-{1}((\d{4})|(-1))),\s(?<City>.+)$", RegexOptions.IgnoreCase);
        public static readonly Regex DeputyBirthRegex2 = new Regex(@"^(?<Date>((\d{2})|(-1))-{1}((\d{2})|(-1))-{1}((\d{4})|(-1)))", RegexOptions.IgnoreCase);
    }
}

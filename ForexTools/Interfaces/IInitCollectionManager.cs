using ForexTools.Enums;

namespace ForexTools.Interfaces
{
    public interface IInitCollectionManager
    {
        IPeriodsCollectionManager AddPeriod(PeriodTypeEnum periodType);
    }
}

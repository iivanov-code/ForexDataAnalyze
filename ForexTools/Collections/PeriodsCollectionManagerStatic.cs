using ForexCommon.Enums;
using ForexTools.Interfaces;

namespace ForexTools.Collections
{
    public partial class PeriodsCollectionManager
    {
        public static IInitCollectionManager GetManager(StockTypeEnum type)
        {
            return new PeriodsCollectionManager(type) as IInitCollectionManager;
        }
    }
}

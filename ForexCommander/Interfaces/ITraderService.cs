using ForexCommon.Enums;

namespace ForexCommander.Interfaces
{
    public interface ITraderService
    {
        float BuyGoods(float price, uint quantity, StockTypeEnum stockType);

        float BuyToClose(float price, StockTypeEnum stockType);

        float SellGoods(float price, uint quantity, StockTypeEnum stockType);

        float SellToClose(float price, StockTypeEnum stockType);

        float WalletBalance();
    }
}

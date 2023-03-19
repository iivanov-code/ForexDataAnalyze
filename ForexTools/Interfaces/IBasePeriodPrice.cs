namespace ForexTools.Interfaces
{
    public interface IBasePeriodPrice
    {
        float MaxPrice { get; }
        float MinPrice { get; }
        float OpeningPrice { get; }
        float? ClosingPrice { get; }
        float AvgPrice { get; }
    }
}

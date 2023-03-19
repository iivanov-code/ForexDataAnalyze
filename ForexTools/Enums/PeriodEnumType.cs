namespace ForexTools.Enums
{
    /// <summary>
    /// The Period in seconds
    /// </summary>
    public enum PeriodTypeEnum
    {
        NONE = 0,
        Second = 1,
        Minute = 60,
        FiveMinutes = 300,
        TenMinutes = 600,
        FifteenMinutes = 900,
        HalfHour = 1800,
        Hour = 3600,
        FourHours = 14400,
        Day = 86400,
        Week = 604800,
        Month = 2419200
    }
}

using System;

namespace ForexCommander.Enums
{
    public enum PositionType
    {
        Short,
        Long
    }

    public static class PositionExtensions
    {
        public static PositionType Negate(this PositionType type)
        {
            switch (type)
            {
                case PositionType.Short:
                    return PositionType.Long;

                case PositionType.Long:
                    return PositionType.Short;

                default:
                    throw new ArgumentException(nameof(type));
            }
        }
    }
}

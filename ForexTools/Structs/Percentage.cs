using System;

namespace ForexTools.Structs
{
    public struct Percentage
    {
        private float _value;

        public static readonly Percentage MaxValue = 100;
        public static readonly Percentage MinValue = 0;

        public Percentage(float value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException("percentage", "Percentage must be between 0 and 100");

            _value = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Percentage other))
            {
                return false;
            }

            return this == (Percentage)obj;
        }

        public override string ToString()
        {
            return this._value.ToString();
        }

        public override int GetHashCode()
        {
            return this._value.GetHashCode();
        }

        public static Percentage operator +(Percentage first, Percentage second)
        {
            return new Percentage(first._value + second._value);
        }

        public static Percentage operator -(Percentage first, Percentage second)
        {
            return new Percentage(first._value - second._value);
        }

        public static bool operator ==(Percentage first, Percentage second)
        {
            return first._value == second._value;
        }

        public static bool operator !=(Percentage first, Percentage second)
        {
            return first._value != second._value;
        }

        public static bool operator >(Percentage first, Percentage second)
        {
            return first._value > second._value;
        }

        public static bool operator <(Percentage first, Percentage second)
        {
            return first._value < second._value;
        }

        public static bool operator <=(Percentage first, Percentage second)
        {
            return first._value <= second._value;
        }

        public static bool operator >=(Percentage first, Percentage second)
        {
            return first._value >= second._value;
        }

        public static Percentage operator ++(Percentage first)
        {
            return new Percentage(first._value++);
        }

        public static Percentage operator --(Percentage first)
        {
            return new Percentage(first._value--);
        }

        public static explicit operator float(Percentage value)
        {
            return value._value;
        }

        public static Percentage operator /(Percentage first, int devider)
        {
            return new Percentage(first._value / devider);
        }

        public static implicit operator Percentage(float value)
        {
            return new Percentage(value);
        }
    }
}

using System;

namespace ForexTools.ConcreteTypes
{
    public class LinearRegresion
    {
        public double A { get; set; }
        public double B { get; set; }
        public double RSquare { get; set; }

        public override string ToString()
        {
            return "y'=" + A + "x+" + B + Environment.NewLine + "R=" + RSquare;
        }
    }
}

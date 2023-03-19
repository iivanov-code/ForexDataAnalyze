using ForexCommander.Enums;

namespace ForexCommander.Models
{
    public class BidModel
    {
        public BidModel(float price, uint quantity, PositionType type)
        {
            this.Price = price;
            this.Quantity = quantity;
            this.Type = type;
        }

        public PositionType Type { get; set; }
        public float Price { get; set; }
        public uint Quantity { get; set; }

        public float TotalPrice
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}

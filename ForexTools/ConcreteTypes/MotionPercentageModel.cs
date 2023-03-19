using ForexTools.Enums;
using ForexTools.Structs;

namespace ForexTools.ConcreteTypes
{
    public class MotionPercentageModel
    {
        public MotionPercentageModel()
        { }

        public MotionPercentageModel(MotionTypeEnum motion, Percentage percentage)
        {
            this.Motion = motion;
            this.Percentage = percentage;
        }

        public MotionTypeEnum Motion { get; set; }

        private readonly object percentPadlock = new object();
        private Percentage percentage;

        public Percentage Percentage
        {
            get
            {
                lock (percentPadlock)
                {
                    return percentage;
                }
            }
            set
            {
                lock (percentPadlock)
                {
                    percentage = value;
                }
            }
        }
    }
}

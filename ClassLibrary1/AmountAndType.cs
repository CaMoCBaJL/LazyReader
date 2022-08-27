namespace Entities
{
    public class AmountAndType
    {
        public ushort amount;
        public byte type;
        public AmountAndType(ushort a, byte t)
        {
            amount = a;
            type = t;
        }
    }
}

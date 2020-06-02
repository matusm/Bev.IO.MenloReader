namespace Bev.IO.MenloReader
{
    public class AuxPod
    {

        public decimal LogTime { get; set; }
        public decimal Frequency { get; set; }

        public AuxPod(decimal x, decimal y)
        {
            LogTime = x;
            Frequency = y;
        }

        public override string ToString()
        {
            return string.Format("{0,11:F3} {1,23:F3}", LogTime, Frequency);
        }
    }
}

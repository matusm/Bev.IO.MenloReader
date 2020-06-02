namespace Bev.IO.MenloReader
{
    public class StatisticPod
    {
        public string QuantityName { get; set; }
        public decimal Average { get; set; }
        public decimal StdDev { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal Span { get { return Max - Min; } }

        public override string ToString()
        {
            return string.Format("{0,26} {1,20:F3} {2,13:F3} {3,13:F3}", QuantityName, Average, StdDev, Span);
            //return string.Format("[StatisticPod: QuantityName={0}, Average={1:F3}, StdDev={2:F3}, Max={3:F3}, Min={4:F3}, Span={5:F3}]", QuantityName, Average, StdDev, Max, Min, Span);
        }
    }
}

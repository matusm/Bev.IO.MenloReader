namespace Bev.IO.MenloReader
{
    public class OutlierFilter
    {
        #region Properties
        public decimal? TargetRepetitionRate { get; set; } = null;
        public decimal? ToleranceRepetitionRate { get; set; } = null;
        public decimal? TargetOffSet { get; set; } = null;
        public decimal? ToleranceOffSet { get; set; } = null;
        public decimal? ToleranceCycleSlip { get; set; } = null;
        public decimal? LockStatusSetpoint { get; set; } = null;
        public FxmNumber FxmCounter { get; }
        #endregion

        #region Ctor
        public OutlierFilter(FxmNumber fxmNumber)
        {
            FxmCounter = fxmNumber;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[OutlierFilter: TargetRepetitionRate={0}, ToleranceRepetitionRate={1}, TargetOffSet={2}, ToleranceOffSet={3}, ToleranceCycleSlip={4}, FxmCounter={5}]", TargetRepetitionRate, ToleranceRepetitionRate, TargetOffSet, ToleranceOffSet, ToleranceCycleSlip, FxmCounter);
        }
    }
}

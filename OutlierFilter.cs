namespace Bev.IO.MenloReader
{
    public class OutlierFilter
    {
        #region Fields
        #endregion

        #region Properties
        public decimal? TargetRepetitionRate { get; set; }
        public decimal? ToleranceRepetitionRate { get; set; }
        public decimal? TargetOffSet { get; set; }
        public decimal? ToleranceOffSet { get; set; }
        public decimal? ToleranceCycleSlip { get; set; }
        public decimal? LockStatusSetpoint { get; set; }
        public FxmNumber FxmCounter { get; }
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.Menlo.Utils.OutlierFilter"/> class.
        /// A filter is specified for a specific FXM counter.
        /// </summary>
        /// <param name="fxmNumber">The FXM counter.</param>
        public OutlierFilter(FxmNumber fxmNumber)
        {
            FxmCounter = fxmNumber;
            TargetRepetitionRate = null;
            ToleranceRepetitionRate = null;
            TargetOffSet = null;
            ToleranceOffSet = null;
            ToleranceCycleSlip = null;
            LockStatusSetpoint = null;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[OutlierFilter: TargetRepetitionRate={0}, ToleranceRepetitionRate={1}, TargetOffSet={2}, ToleranceOffSet={3}, ToleranceCycleSlip={4}, FxmCounter={5}]", TargetRepetitionRate, ToleranceRepetitionRate, TargetOffSet, ToleranceOffSet, ToleranceCycleSlip, FxmCounter);
        }
    }
}

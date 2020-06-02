namespace Bev.IO.MenloReader
{
    /// <summary>
    /// Container class for filter parameters of recorded frequencies. Per FXM counter.
    /// </summary>
    public class OutlierFilter
    {
        #region Fields
        decimal? targetRepetitionRate;
        decimal? toleranceRepetitionRate;
        decimal? targetOffSet;
        decimal? toleranceOffSet;
        decimal? toleranceCycleSlip;
        decimal? lockStatusSetpoint;
        FxmNumber fxm;
        #endregion

        #region Properties
        public decimal? TargetRepetitionRate { get { return targetRepetitionRate; } set { targetRepetitionRate = value; } }
        public decimal? ToleranceRepetitionRate { get { return toleranceRepetitionRate; } set { toleranceRepetitionRate = value; } }
        public decimal? TargetOffSet { get { return targetOffSet; } set { targetOffSet = value; } }
        public decimal? ToleranceOffSet { get { return toleranceOffSet; } set { toleranceOffSet = value; } }
        public decimal? ToleranceCycleSlip { get { return toleranceCycleSlip; } set { toleranceCycleSlip = value; } }
        public decimal? LockStatusSetpoint { get { return lockStatusSetpoint; } set { lockStatusSetpoint = value; } }
        public FxmNumber FxmCounter { get { return fxm; } }
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.Menlo.Utils.OutlierFilter"/> class.
        /// A filter is specified for a specific FXM counter.
        /// </summary>
        /// <param name="fxmNumber">The FXM counter.</param>
        public OutlierFilter(FxmNumber fxmNumber)
        {
            fxm = fxmNumber;
            targetRepetitionRate = null;
            toleranceRepetitionRate = null;
            targetOffSet = null;
            toleranceOffSet = null;
            toleranceCycleSlip = null;
            lockStatusSetpoint = null;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[OutlierFilter: TargetRepetitionRate={0}, ToleranceRepetitionRate={1}, TargetOffSet={2}, ToleranceOffSet={3}, ToleranceCycleSlip={4}, FxmCounter={5}]", TargetRepetitionRate, ToleranceRepetitionRate, TargetOffSet, ToleranceOffSet, ToleranceCycleSlip, FxmCounter);
        }
    }
}

using System.Text;

namespace Bev.IO.MenloReader
{
    /// <summary>
    /// A simple container class to hold a single result data sample.
    /// </summary>
    /// <remarks>No Ctor nor fields needed. A user friendly <c>ToString()</c> is provided.</remarks>
    public class ResultPod
    {

        #region Properties
        public decimal LogTime { get; set; }
        public decimal RepetitionFrequency { get; set; }
        public decimal SignedOffsetFrequency { get; set; }
        public decimal SignedBeatFrequency { get; set; }
        public decimal AuxData0 { get; set; }
        public decimal AuxData1 { get; set; }
        public decimal? DeltaLaserFrequency { get; set; }
        public decimal? DeltaLaserFrequencyFixed { get; set; }
        public decimal LaserFrequency { get; set; }
        public decimal? LaserFrequencyFixed { get; set; }
        public decimal? OutputPower { get; set; }
        public LockStatus? Status { get; set; }
        #endregion

        /// <summary>
        /// Returns a nicely formatted string presenting all values not beeing <c>null</c>.
        /// </summary>
        /// <returns>The string returned by <c>ToString()</c>.</returns>
        string TextLine()
        {
            //TODO language specific formatting!
            StringBuilder fullLine = new StringBuilder();
            fullLine.AppendFormat("{0,11:F3}", LogTime);
            fullLine.AppendFormat("{0,17:F5}", RepetitionFrequency);
            fullLine.AppendFormat("{0,15:+0.000;-0.000}", SignedOffsetFrequency);
            fullLine.AppendFormat("{0,15:+0.000;-0.000}", SignedBeatFrequency);
            // fullLine.AppendFormat("{0,20:F3}", LaserFrequency); // for dignostics only
            // output laser frequency only if deviation from target does not exist
            if (DeltaLaserFrequency == null)
            {
                fullLine.AppendFormat("{0,21:F3}", LaserFrequency);
                if (LaserFrequencyFixed != null)
                    fullLine.AppendFormat("{0,22:F3}", LaserFrequencyFixed);
            }
            // if (LaserFrequencyFixed!=null) fullLine.AppendFormat("{0,21:F3}", LaserFrequencyFixed); // for dignostics only
            if (DeltaLaserFrequency != null)
                fullLine.AppendFormat("{0,20:F3}", DeltaLaserFrequency);
            if (DeltaLaserFrequencyFixed != null)
                fullLine.AppendFormat("{0,20:F3}", DeltaLaserFrequencyFixed);
            fullLine.AppendFormat("{0,9:F4}", AuxData0);
            fullLine.AppendFormat("{0,9:F4}", AuxData1);
            if (OutputPower != null)
                fullLine.AppendFormat("{0,8:F1}", OutputPower);
            if (Status != null)
                fullLine.AppendFormat(" {0}", Status);
            return fullLine.ToString();
        }

        public override string ToString() { return TextLine(); }

    }
}

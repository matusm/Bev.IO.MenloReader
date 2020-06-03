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
        public LockStatus Status { get; set; } = LockStatus.Unknown;
        #endregion

        private string ToTextLine()
        {
            //TODO language specific formatting!
            StringBuilder textLine = new StringBuilder();
            textLine.Append($"{LogTime,11:F3}");
            textLine.Append($"{RepetitionFrequency,17:F5}");
            textLine.Append($"{SignedOffsetFrequency,15:+0.000;-0.000}");
            textLine.Append($"{SignedBeatFrequency,15:+0.000;-0.000}");
            // fullLine.AppendFormat("{0,20:F3}", LaserFrequency); // for dignostics only
            // output laser frequency only if deviation from target does not exist
            if (DeltaLaserFrequency == null)
            {
                textLine.Append($"{LaserFrequency,21:F3}");
                if (LaserFrequencyFixed != null)
                    textLine.Append($"{LaserFrequencyFixed,22:F3}");
            }
            // if (LaserFrequencyFixed!=null) fullLine.AppendFormat("{0,21:F3}", LaserFrequencyFixed); // for dignostics only
            if (DeltaLaserFrequency != null)
                textLine.Append($"{DeltaLaserFrequency,20:F3}");
            if (DeltaLaserFrequencyFixed != null)
                textLine.Append($"{DeltaLaserFrequencyFixed,20:F3}");
            textLine.Append($"{AuxData0,9:F4}");
            textLine.Append($"{AuxData1,9:F4}");
            if (OutputPower != null)
                textLine.Append($"{OutputPower,8:F1}");
            textLine.Append($" {Status}");
            return textLine.ToString();
        }

        public override string ToString() { return ToTextLine(); }

    }
}

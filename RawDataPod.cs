using System;
using System.Globalization;

namespace Bev.IO.MenloReader
{
    public class RawDataPod
    {
        #region Fields (corresponding data file entries)
        decimal logTime;
        decimal counter0;
        decimal counter1;
        decimal counter2;
        decimal counter3;
        decimal counter4;
        decimal counter5;
        decimal counter6;
        decimal counter7;
        decimal auxData0;
        decimal auxData1;
        decimal? outputPower;
        LockStatus? status;

        bool parseError;
        OutlierType outlierFxm0;
        OutlierType outlierFxm1;
        int dataColumns;
        #endregion

        #region Properties
        // probably just a getter would be better
        public decimal LogTime { get { return logTime; } set { logTime = value; } }
        public decimal Counter0 { get { return counter0; } set { counter0 = value; } }
        public decimal Counter1 { get { return counter1; } set { counter1 = value; } }
        public decimal Counter2 { get { return counter2; } set { counter2 = value; } }
        public decimal Counter3 { get { return counter3; } set { counter3 = value; } }
        public decimal Counter4 { get { return counter4; } set { counter4 = value; } }
        public decimal Counter5 { get { return counter5; } set { counter5 = value; } }
        public decimal Counter6 { get { return counter6; } set { counter6 = value; } }
        public decimal Counter7 { get { return counter7; } set { counter7 = value; } }
        public decimal AuxData0 { get { return auxData0; } set { auxData0 = value; } }
        public decimal AuxData1 { get { return auxData1; } set { auxData1 = value; } }
        public bool ParseError { get { return parseError; } }
        public OutlierType OutlierFxm0 { get { return outlierFxm0; } }
        public OutlierType OutlierFxm1 { get { return outlierFxm1; } }
        public decimal? OutputPower { get { return outputPower; } set { outputPower = value; } }
        public LockStatus? Status { get { return status; } set { status = value; } }
        #endregion

        #region Ctor
        public RawDataPod()
        {
            // all values are 0 at this point
            parseError = false;
            outlierFxm0 = OutlierType.None;
            outlierFxm1 = OutlierType.None;
        }
        public RawDataPod(string str) : this()
        {
            parseError = !ParseLine(str);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Marks a <c>RawDataPod</c> object as an outlier according to criterions set in the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void MarkOutlier(OutlierFilter filter)
        {
            FxmNumber fxm = filter.FxmCounter;
            decimal fsyn = 0;
            decimal foff = 0;
            decimal fbeat1 = 0;
            decimal fbeat2 = 0;
            OutlierType outlierFxmTemp = OutlierType.None;
            if (filter.FxmCounter == FxmNumber.Fxm0)
            {
                fsyn = counter0;
                foff = counter1;
                fbeat1 = counter2;
                fbeat2 = counter3;
            }
            if (filter.FxmCounter == FxmNumber.Fxm1)
            {
                fsyn = counter4;
                foff = counter5;
                fbeat1 = counter6;
                fbeat2 = counter7;
            }

            // check rep-rate
            if (filter.TargetRepetitionRate.HasValue && filter.ToleranceRepetitionRate.HasValue)
                if (Math.Abs(fsyn - (decimal)filter.TargetRepetitionRate) > (decimal)filter.ToleranceRepetitionRate)
                    outlierFxmTemp |= OutlierType.RepetitionRate;
            // check offset
            if (filter.TargetOffSet.HasValue && filter.ToleranceOffSet.HasValue)
                if (Math.Abs(foff - (decimal)filter.TargetOffSet) > (decimal)filter.ToleranceOffSet)
                    outlierFxmTemp |= OutlierType.OffSet;
            // check cycle slip
            if (filter.ToleranceCycleSlip.HasValue)
                if (Math.Abs(fbeat1 - fbeat2) > filter.ToleranceCycleSlip)
                    outlierFxmTemp |= OutlierType.CycleSlip;
            // check laser lock status

            if (filter.FxmCounter == FxmNumber.Fxm0)
                outlierFxm0 = outlierFxmTemp;
            if (filter.FxmCounter == FxmNumber.Fxm1)
                outlierFxm1 = outlierFxmTemp;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Parses a text line (from data file) to fill properties of the <c>RawDataPod</c>.
        /// </summary>
        /// <returns><c>true</c>, if line was parsed without errors, <c>false</c> otherwise.</returns>
        /// <param name="line">The string to be parsed.</param>
        private bool ParseLine(string line)
        {
            // split the line into tokens
            char[] sep = { ' ', '\t' };
            string[] token = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            dataColumns = token.Length;

            if (dataColumns >= 1)
                if (!decimal.TryParse(token[0], NumberStyles.Any, CultureInfo.InvariantCulture, out logTime))
                    return false;
            if (dataColumns >= 2)
                if (!decimal.TryParse(token[1], NumberStyles.Any, CultureInfo.InvariantCulture, out counter0))
                    return false;
            if (dataColumns >= 3)
                if (!decimal.TryParse(token[2], NumberStyles.Any, CultureInfo.InvariantCulture, out counter1))
                    return false;
            if (dataColumns >= 4)
                if (!decimal.TryParse(token[3], NumberStyles.Any, CultureInfo.InvariantCulture, out counter2))
                    return false;
            if (dataColumns >= 5)
                if (!decimal.TryParse(token[4], NumberStyles.Any, CultureInfo.InvariantCulture, out counter3))
                    return false;
            if (dataColumns >= 6)
                if (!decimal.TryParse(token[5], NumberStyles.Any, CultureInfo.InvariantCulture, out counter4))
                    return false;
            if (dataColumns >= 7)
                if (!decimal.TryParse(token[6], NumberStyles.Any, CultureInfo.InvariantCulture, out counter5))
                    return false;
            if (dataColumns >= 8)
                if (!decimal.TryParse(token[7], NumberStyles.Any, CultureInfo.InvariantCulture, out counter6))
                    return false;
            if (dataColumns >= 9)
                if (!decimal.TryParse(token[8], NumberStyles.Any, CultureInfo.InvariantCulture, out counter7))
                    return false;
            if (dataColumns >= 10)
                if (!decimal.TryParse(token[9], NumberStyles.Any, CultureInfo.InvariantCulture, out auxData0))
                    return false;
            if (dataColumns >= 11)
                if (!decimal.TryParse(token[10], NumberStyles.Any, CultureInfo.InvariantCulture, out auxData1))
                    return false;
            return true;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("[RawDataPod: LogTime={0}, Counter0={1}, Counter1={2}, Counter2={3}, Counter3={4}, Counter4={5}, Counter5={6}, Counter6={7}, Counter7={8}, AuxData0={9}, AuxData1={10}, ParseError={11}, OutlierFxm0={12}, OutlierFxm1={13}]", LogTime, Counter0, Counter1, Counter2, Counter3, Counter4, Counter5, Counter6, Counter7, AuxData0, AuxData1, ParseError, OutlierFxm0, OutlierFxm1);
        }

    }

}

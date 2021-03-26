using System;
using System.Globalization;

namespace Bev.IO.MenloReader
{
    public class RawDataPod
    {
        private bool noAuxData = true;

        #region Properties
        // probably just a getter would be better
        public decimal LogTime { get; set; }
        public decimal Counter0 { get; set; }
        public decimal Counter1 { get; set; }
        public decimal Counter2 { get; set; }
        public decimal Counter3 { get; set; }
        public decimal Counter4 { get; set; }
        public decimal Counter5 { get; set; }
        public decimal Counter6 { get; set; }
        public decimal Counter7 { get; set; }
        public decimal AuxData0 { get; set; }
        public decimal AuxData1 { get; set; }
        public bool ParseError { get; }
        public OutlierType OutlierFxm0 { get; private set; } = OutlierType.None;
        public OutlierType OutlierFxm1 { get; private set; } = OutlierType.None;
        public decimal? OutputPower { get; set; } = null;
        public LockStatus Status { get; set; } = LockStatus.Unknown;
        #endregion

        #region Ctor
        public RawDataPod(string str)
        {
            ParseError = !ParseLine(str);
        }
        #endregion

        #region Public Methods
        public void SetLockStatus(double threshold) 
        {
            Status = LockStatus.Unknown;
            if (noAuxData) return;
            if ((double)AuxData0 < threshold)
                Status = LockStatus.Unlocked;
            else
                Status = LockStatus.Locked;
        }

        public void SetOutputPower(double intercept, double slope)
        {
            OutputPower = null;
            if (noAuxData) return;
            OutputPower = (decimal)intercept + (decimal)slope * AuxData1;
        }

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
                fsyn = Counter0;
                foff = Counter1;
                fbeat1 = Counter2;
                fbeat2 = Counter3;
            }
            if (filter.FxmCounter == FxmNumber.Fxm1)
            {
                fsyn = Counter4;
                foff = Counter5;
                fbeat1 = Counter6;
                fbeat2 = Counter7;
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
                OutlierFxm0 = outlierFxmTemp;
            if (filter.FxmCounter == FxmNumber.Fxm1)
                OutlierFxm1 = outlierFxmTemp;
        }
        #endregion

        #region Private Methods

        private decimal? ParseDecimal(string token)
        {
            decimal value;
            if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return value;
            return null;
        }

        private bool ParseLine(string line)
        {
            // split the line into tokens
            char[] sep = { ' ', '\t' };
            string[] columns = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int numberOfColumns = columns.Length;
            decimal? value;

            if (numberOfColumns >= 1)
            {
                value = ParseDecimal(columns[0]);
                if (!value.HasValue) return false;
                LogTime = value.Value;
            }
            if (numberOfColumns >= 2)
            {
                value = ParseDecimal(columns[1]);
                if (!value.HasValue) return false;
                Counter0 = value.Value;
            }
            if (numberOfColumns >= 3)
            {
                value = ParseDecimal(columns[2]);
                if (!value.HasValue) return false;
                Counter1 = value.Value;
            }
            if (numberOfColumns >= 4)
            {
                value = ParseDecimal(columns[3]);
                if (!value.HasValue) return false;
                Counter2 = value.Value;
            }
            if (numberOfColumns >= 5)
            {
                value = ParseDecimal(columns[4]);
                if (!value.HasValue) return false;
                Counter3 = value.Value;
            }
            if (numberOfColumns >= 6)
            {
                value = ParseDecimal(columns[5]);
                if (!value.HasValue) return false;
                Counter4 = value.Value;
            }
            if (numberOfColumns >= 7)
            {
                value = ParseDecimal(columns[6]);
                if (!value.HasValue) return false;
                Counter5 = value.Value;
            }
            if (numberOfColumns >= 8)
            {
                value = ParseDecimal(columns[7]);
                if (!value.HasValue) return false;
                Counter6 = value.Value;
            }
            if (numberOfColumns >= 9)
            {
                value = ParseDecimal(columns[8]);
                if (!value.HasValue) return false;
                Counter7 = value.Value;
            }
            if (numberOfColumns >= 10)
            {
                value = ParseDecimal(columns[9]);
                if (!value.HasValue) return false;
                AuxData0 = value.Value;
                noAuxData = false;
            }
            if (numberOfColumns >= 11)
            {
                value = ParseDecimal(columns[10]);
                if (!value.HasValue) return false;
                AuxData1 = value.Value;
                noAuxData = false;
            }
            return true;
        }

        #endregion

        public override string ToString()
        {
            return $"[RawDataPod: LogTime={LogTime} Counter0={Counter0} Counter1={Counter1} Counter2={Counter2} Counter3={Counter3} Counter4={Counter4} Counter5={Counter5} Counter6={Counter6} Counter7={Counter7} AuxData0={AuxData0} AuxData1={AuxData1} ParseError={ParseError} OutlierFxm0={OutlierFxm0} OutlierFxm1={OutlierFxm1}]";
        }

    }

}

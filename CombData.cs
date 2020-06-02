using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Bev.IO.MenloReader
{
    public class CombData
    {
        #region Fields
        List<decimal> counter0;
        List<decimal> counter1;
        List<decimal> counter2;
        List<decimal> counter3;
        List<decimal> counter4;
        List<decimal> counter5;
        List<decimal> counter6;
        List<decimal> counter7;
        List<decimal> auxdata0;
        List<decimal> auxdata1;
        List<decimal> logTime;
        int? numberOutlierFrep0;
        int? numberOutlierFoff0;
        int? numberOutlierCycl0;
        int? numberOutlierFrep1;
        int? numberOutlierFoff1;
        int? numberOutlierCycl1;
        int? numberOutlierLaserUnlocked;
        decimal unixSystemTime;
        decimal? predictedFrep0;
        decimal? predictedFrep1;
        #endregion

        #region Properties
        public int? NumberLinesInFile { get; private set; }
        public int? NumberRawDataPoint { get; private set; }
        public int? NumberFilteredDataPoint { get; private set; }
        public DateTime SystemTime { get; private set; }
        public decimal UnixSystemTime { get { return unixSystemTime; } }
        public List<string> InputFileHeaders { get; }
        public decimal? PredictedFrep0 { get { return predictedFrep0; } }
        public decimal? PredictedFrep1 { get { return predictedFrep1; } }
        public List<RawDataPod> MeasurementData { get; }

        public decimal Counter0mean { get; private set; }
        public decimal Counter1mean { get; private set; }
        public decimal Counter2mean { get; private set; }
        public decimal Counter3mean { get; private set; }
        public decimal Counter4mean { get; private set; }
        public decimal Counter5mean { get; private set; }
        public decimal Counter6mean { get; private set; }
        public decimal Counter7mean { get; private set; }

        public decimal? VoltageToPowerSlope { get; set; } = null;
        public decimal? VoltageToPowerFixed { get; set; } = null;
        public decimal? LockingSetPoint { get; set; } = null;
        #endregion

        public CombData()
        {
            MeasurementData = new List<RawDataPod>();
            InputFileHeaders = new List<string>();
            ClearData();
        }

        #region Public methods
        /// <summary>
        /// Loads a measurement file in the two lists.
        /// </summary>
        /// <param name="datFileName">Fully qualified filename.</param>
        public bool LoadFile(string datFileName)
        {
            RawDataPod tempRDP;
            if (!File.Exists(datFileName))
                return false;
            ClearData(); // clear only if file exist
            StreamReader hDatFile = File.OpenText(datFileName);
            string datLine;
            NumberLinesInFile = 0; // ClearData() sets it to null
            while ((datLine = hDatFile.ReadLine()) != null)
                if (!string.IsNullOrWhiteSpace(datLine))
                {
                    NumberLinesInFile++;
                    if (datLine.Contains("#")) // every line containing # is considered a comment
                    {
                        if (!datLine.Contains("# Measurement finished at...")) // the last line in file, discard
                                                                               // strip # and spaces
                            InputFileHeaders.Add(datLine.Replace("#", " ").Trim());
                    }
                    else
                    {
                        tempRDP = new RawDataPod(datLine);
                        tempRDP.OutputPower = CalculateLaserOutputPower(tempRDP.AuxData0);
                        tempRDP.Status = CheckLockStatus(tempRDP.AuxData1);
                        MeasurementData.Add(tempRDP);
                    }
                }
            hDatFile.Close();
            // now remove all measurement data with parsing errors
            MeasurementData.RemoveAll(item => item.ParseError);
            NumberRawDataPoint = MeasurementData.Count;
            ParseSystemTime();
            // predict synthesizer freqency
            predictedFrep0 = PredictSynthesizerFrequency(FxmNumber.Fxm0, 5);
            predictedFrep1 = PredictSynthesizerFrequency(FxmNumber.Fxm1, 5);
            return true;
        }

        /// <summary>
        /// Handy function to set all three voltage coefficients at once.
        /// </summary>
        /// <remarks>Thus avoiding calls to the respective properties.</remarks>
        /// <param name="k">The slope of the linear voltage to power function.</param>
        /// <param name="d">The fixed part of the linear voltage to power function.</param>
        /// <param name="s">The locking setpoint voltage;</param>
        public void SetCoefficients(decimal? k, decimal? d, decimal? s)
        {
            VoltageToPowerSlope = k;
            VoltageToPowerFixed = d;
            LockingSetPoint = s;
        }
        public void SetCoefficients(decimal? k, decimal? d) { SetCoefficients(k, d, null); }
        public void SetCoefficients(decimal? s) { SetCoefficients(null, null, s); }

        public void RemoveAllOutliers(OutlierFilter flt)
        {
            MarkAllOutliers(flt);
            MeasurementData.RemoveAll(
                item => item.OutlierFxm0 != OutlierType.None || item.OutlierFxm1 != OutlierType.None);
            NumberFilteredDataPoint = MeasurementData.Count;
            AverageCounterValues();
        }

        public void AverageCounterValues()
        {
            counter0 = new List<decimal>();
            counter1 = new List<decimal>();
            counter2 = new List<decimal>();
            counter3 = new List<decimal>();
            counter4 = new List<decimal>();
            counter5 = new List<decimal>();
            counter6 = new List<decimal>();
            counter7 = new List<decimal>();
            auxdata0 = new List<decimal>();
            auxdata1 = new List<decimal>();
            logTime = new List<decimal>();

            if (MeasurementData.Count < 1) return;

            foreach (var dp in MeasurementData)
            {
                counter0.Add(dp.Counter0);
                counter1.Add(dp.Counter1);
                counter2.Add(dp.Counter2);
                counter3.Add(dp.Counter3);
                counter4.Add(dp.Counter4);
                counter5.Add(dp.Counter5);
                counter6.Add(dp.Counter6);
                counter7.Add(dp.Counter7);
                auxdata0.Add(dp.AuxData0);
                auxdata1.Add(dp.AuxData1);
                logTime.Add(dp.LogTime);
            }

            Counter0mean = counter0.Average();
            Counter1mean = counter1.Average();
            Counter2mean = counter2.Average();
            Counter3mean = counter3.Average();
            Counter4mean = counter4.Average();
            Counter5mean = counter5.Average();
            Counter6mean = counter6.Average();
            Counter7mean = counter7.Average();
        }

        public decimal? PredictedFrep(FxmNumber fxm)
        {
            if (fxm == FxmNumber.Fxm0) return predictedFrep0;
            if (fxm == FxmNumber.Fxm1) return predictedFrep1;
            return null;
        }

        public int? OutlierFrep(FxmNumber fxm)
        {
            if (fxm == FxmNumber.Fxm0) return numberOutlierFrep0;
            if (fxm == FxmNumber.Fxm1) return numberOutlierFrep1;
            return null;
        }

        public int? OutlierFoff(FxmNumber fxm)
        {
            if (fxm == FxmNumber.Fxm0) return numberOutlierFoff0;
            if (fxm == FxmNumber.Fxm1) return numberOutlierFoff1;
            return null;
        }

        public int? OutlierCycl(FxmNumber fxm)
        {
            if (fxm == FxmNumber.Fxm0) return numberOutlierCycl0;
            if (fxm == FxmNumber.Fxm1) return numberOutlierCycl1;
            return null;
        }

        #endregion

        #region Private methods
        private decimal? PredictSynthesizerFrequency(FxmNumber fxm, int n)
        {
            if (MeasurementData.Count <= n)
                return null;
            List<decimal> sample = new List<decimal>();
            switch (fxm)
            {
                case FxmNumber.Fxm0:
                    for (int i = 0; i < n; i++)
                        sample.Add(Math.Round(MeasurementData[i].Counter0, MidpointRounding.AwayFromZero));
                    break;
                case FxmNumber.Fxm1:
                    for (int i = 0; i < n; i++)
                        sample.Add(Math.Round(MeasurementData[i].Counter4, MidpointRounding.AwayFromZero));
                    break;
                default:
                    return null;
            }
            if (sample.Max() != sample.Min())
                return null;
            return sample.Max();
        }

        private void MarkAllOutliers(OutlierFilter flt)
        {
            foreach (var dp in MeasurementData)
                dp.MarkOutlier(flt);
            CountOutliers();
        }

        private void ClearData()
        {
            if (MeasurementData != null)
                MeasurementData.Clear();
            if (InputFileHeaders != null)
                InputFileHeaders.Clear();
            NumberLinesInFile = null;
            NumberFilteredDataPoint = null;
            NumberRawDataPoint = null;
            SystemTime = DateTime.UtcNow;
            numberOutlierFrep0 = null;
            numberOutlierFoff0 = null;
            numberOutlierCycl0 = null;
            numberOutlierFrep1 = null;
            numberOutlierFoff1 = null;
            numberOutlierCycl1 = null;
            numberOutlierLaserUnlocked = null;
            //TODO more fields
        }

        /// <summary>
        /// Transforms a laser supplied voltage in V to the laser output power in µW. 
        /// </summary>
        /// <remarks>The tranformation is realized as a linear function.</remarks>
        /// <param name="voltage">The measured DC-voltage in V.</param>
        /// <returns></returns>
        decimal? CalculateLaserOutputPower(decimal voltage)
        {
            if (VoltageToPowerFixed == null) return null;
            if (VoltageToPowerSlope == null) return null;
            decimal? power = VoltageToPowerSlope * voltage + VoltageToPowerFixed;
            if (power < 0) power = 0; // a power can not be negative!
            return power;
        }

        /// <summary>
        /// Some lasers provide a lock status output voltage. This function evaluates this voltage.
        /// </summary>
        /// <param name="voltage">The lock status output voltage in V.</param>
        /// <returns></returns>
        LockStatus CheckLockStatus(decimal voltage)
        {
            if (LockingSetPoint == null) return LockStatus.Unknown;
            if (voltage > LockingSetPoint) return LockStatus.Locked;
            return LockStatus.Unlocked;
        }

        private void ParseSystemTime()
        {
            unixSystemTime = 0.0m;
            if (InputFileHeaders == null)
                return;
            foreach (string str in InputFileHeaders)
                if (str.Contains("System time is"))
                {
                    string st = str.Replace("System time is", "");
                    decimal.TryParse(st, NumberStyles.Any, CultureInfo.InvariantCulture, out unixSystemTime);
                    SystemTime = UnixTimeStampToDateTime(unixSystemTime);
                    return;
                }
        }

        private void CountOutliers()
        {
            numberOutlierFrep0 = 0;
            numberOutlierFoff0 = 0;
            numberOutlierCycl0 = 0;
            numberOutlierFrep1 = 0;
            numberOutlierFoff1 = 0;
            numberOutlierCycl1 = 0;
            numberOutlierLaserUnlocked = 0;
            foreach (var item in MeasurementData)
            {
                if ((item.OutlierFxm0 & OutlierType.RepetitionRate) == OutlierType.RepetitionRate)
                    numberOutlierFrep0++;
                if ((item.OutlierFxm1 & OutlierType.RepetitionRate) == OutlierType.RepetitionRate)
                    numberOutlierFrep1++;
                if ((item.OutlierFxm0 & OutlierType.OffSet) == OutlierType.OffSet)
                    numberOutlierFoff0++;
                if ((item.OutlierFxm1 & OutlierType.OffSet) == OutlierType.OffSet)
                    numberOutlierFoff1++;
                if ((item.OutlierFxm0 & OutlierType.CycleSlip) == OutlierType.CycleSlip)
                    numberOutlierCycl0++;
                if ((item.OutlierFxm1 & OutlierType.CycleSlip) == OutlierType.CycleSlip)
                    numberOutlierCycl1++;

            }
        }

        private DateTime UnixTimeStampToDateTime(decimal unixT)
        {
            // Unix timestamp is seconds past epoch
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds((double)unixT);
        }

        #endregion
    }
}

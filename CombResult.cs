using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bev.IO.MenloReader
{
    public class CombResult
    {
        #region Fields
        private CombData cd;
        private CwBeatCalculation cw;
        private FxmNumber fxm;
        private decimal? target;
        private decimal? fixedIF;
        #endregion

        #region Properties
        public List<ResultPod> ResultData { get; }
        public string[] ColumnHeaders => GenerateColumnHeadings();
        public List<StatisticPod> Statistics => GenerateStatistic();
        public decimal[] YDataForPlot => GenerateYDataForPlot();
        public decimal[] XDataForPlot => ExtractSingleColumn("LogTime");
        public PlotType AuxType { get; private set; } = PlotType.None;
        #endregion

        #region Ctor
        public CombResult(CombData cd, CwBeatCalculation cw, FxmNumber fxm, decimal? target, decimal? fixedIF)
        {
            this.cd = cd;
            this.cw = cw;
            this.fxm = fxm;
            this.target = target;
            this.fixedIF = fixedIF;
            ResultData = new List<ResultPod>();
            GenerateResults();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Here the actual data evaluation takes place. The result is stored in <c>resultData</c>.
        /// </summary>
        private void GenerateResults()
        {
            ResultPod resultPod;
            decimal chn1 = 0m; // raw IF
            decimal chn2 = 0m; // raw offset
            decimal chn3 = 0m; // beat
            foreach (var rawDataPod in cd.MeasurementData)
            {
                resultPod = new ResultPod();
                resultPod.LogTime = rawDataPod.LogTime;
                resultPod.AuxData0 = rawDataPod.AuxData0;
                resultPod.AuxData1 = rawDataPod.AuxData1;
                resultPod.OutputPower = rawDataPod.OutputPower;
                resultPod.Status = rawDataPod.Status;
                if (fxm == FxmNumber.Fxm0)
                {
                    chn1 = rawDataPod.Counter0;
                    chn2 = rawDataPod.Counter1;
                    chn3 = rawDataPod.Counter2;
                }
                if (fxm == FxmNumber.Fxm1)
                {
                    chn1 = rawDataPod.Counter4;
                    chn2 = rawDataPod.Counter5;
                    chn3 = rawDataPod.Counter6;
                }
                resultPod.RepetitionFrequency = cw.RepetitionFrequency(chn1);
                resultPod.SignedOffsetFrequency = cw.ActualOffsetFrequency(chn2);
                resultPod.SignedBeatFrequency = cw.SignBeat * chn3;
                resultPod.LaserFrequency = cw.AbsoluteLaserFrequency(chn1, chn2, chn3);
                // set optionals to null first
                resultPod.LaserFrequencyFixed = null;
                resultPod.DeltaLaserFrequency = null;
                resultPod.DeltaLaserFrequencyFixed = null;
                // now fill optional with value, if possible
                if (fixedIF != null)
                {
                    resultPod.LaserFrequencyFixed = cw.AbsoluteLaserFrequency((decimal)fixedIF, chn2, chn3);
                }
                if (target != null)
                {
                    resultPod.DeltaLaserFrequency = resultPod.LaserFrequency - target;
                    if (fixedIF != null) resultPod.DeltaLaserFrequencyFixed = resultPod.LaserFrequencyFixed - target;
                }
                ResultData.Add(resultPod);
            }
        }

        /// <summary>
        /// Determines the column headers for the output file.
        /// </summary>
        /// <returns>The array of headings.</returns>
        /// <remarks>The first element of the <c>resultData</c> list is analyzed.</remarks>
        private string[] GenerateColumnHeadings()
        {
            List<string> headings = new List<string>();
            if (ResultData.Count == 0) return headings.ToArray();
            ResultPod resultPod = ResultData[0];
            headings.Add("Time / s");
            headings.Add("Repetition frequency / Hz");
            headings.Add("Offset frequency / Hz");
            headings.Add("Beat frequency / Hz");
            if (resultPod.DeltaLaserFrequency == null)
            {
                headings.Add("Laser frequency (using measured f_rep) / Hz");
                if (resultPod.LaserFrequencyFixed != null)
                    headings.Add("Laser frequency (using set f_rep) / Hz");
            }
            if (resultPod.DeltaLaserFrequency != null)
                headings.Add("Laser frequency (using measured f_rep) - target frequency / Hz");
            if (resultPod.DeltaLaserFrequencyFixed != null)
                headings.Add("Laser frequency (using set f_rep) - target frequency / Hz");
            headings.Add("Auxiliary channel 0 / V");
            headings.Add("Auxiliary channel 1 / V");
            if (resultPod.OutputPower != null)
                headings.Add("Optical output power / µW");
            if (resultPod.Status != LockStatus.Unknown)
                headings.Add("Lock status");
            return headings.ToArray();
        }

        /// <summary>
        /// Generates statistic values for each calculated and filtered quantity.
        /// </summary>
        /// <returns>The <c>List of StatisticPod</c>.</returns>
        private List<StatisticPod> GenerateStatistic()
        {
            string[] allProperties =
            {
                "RepetitionFrequency",
                "SignedOffsetFrequency",
                "SignedBeatFrequency",
                "LaserFrequency",
                "LaserFrequencyFixed",
                "DeltaLaserFrequency",
                "DeltaLaserFrequencyFixed",
                "AuxData0",
                "AuxData1"
            };
            StatisticPod statisticPod;
            List<StatisticPod> statisticsPods = new List<StatisticPod>();
            foreach (string s in allProperties)
            {
                statisticPod = GenerateStatisticPod(s);
                if (statisticPod != null) statisticsPods.Add(statisticPod);
            }
            return statisticsPods;
        }

        /// <summary>
        /// Generates a single <c>StatisticPod</c>.
        /// </summary>
        /// <returns>The <c>StatisticPod</c>.</returns>
        /// <param name="propName">Name of property.</param>
        private StatisticPod GenerateStatisticPod(string propName)
        {
            decimal[] singleCol = ExtractSingleColumn(propName);
            if (singleCol == null) return null;
            if (singleCol.Length <= 2) return null;
            StatisticPod statisticPod = new StatisticPod();
            statisticPod.QuantityName = propName;
            statisticPod.Average = singleCol.Average();
            statisticPod.StdDev = (decimal)singleCol.StandardDeviation();
            statisticPod.Max = singleCol.Max();
            statisticPod.Min = singleCol.Min();
            return statisticPod;
        }

        /// <summary>
        /// Generates an array of the ordinates for a plot file.
        /// </summary>
        /// <returns>An array of the y-values.</returns>
        private decimal[] GenerateYDataForPlot()
        {
            decimal[] singleCol;
            singleCol = ExtractSingleColumn("DeltaLaserFrequencyFixed");
            if (singleCol != null)
            {
                AuxType = PlotType.DeltaLaserFrequencyFixed;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("DeltaLaserFrequency");
            if (singleCol != null)
            {
                AuxType = PlotType.DeltaLaserFrequency;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("LaserFrequencyFixed");
            if (singleCol != null)
            {
                AuxType = PlotType.LaserFrequencyFixed;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("LaserFrequency");
            AuxType = PlotType.LaserFrequency;
            return singleCol;
        }

        /// <summary>
        /// Extracts a single property as array.
        /// </summary>
        /// <param name="propName">The name of a property exported by <c>CombResult</c>. </param>
        /// <returns>A single column.</returns>
        private decimal[] ExtractSingleColumn(string propName)
        {
            // check if resultData exists
            if (ResultData == null) return null;
            if (ResultData.Count == 0) return null;
            PropertyInfo prop;
            // now generate the extracted list and return
            List<decimal> col = new List<decimal>();
            foreach (var rdp in ResultData)
            {
                prop = rdp.GetType().GetProperty(propName);
                if (prop == null) return null;
                if (prop.GetValue(rdp) == null) return null;
                col.Add((decimal)prop.GetValue(rdp));
            }
            return col.ToArray();
        }

        #endregion
    }

    public enum PlotType
    {
        None,
        DeltaLaserFrequencyFixed,
        DeltaLaserFrequency,
        LaserFrequencyFixed,
        LaserFrequency
    }

}

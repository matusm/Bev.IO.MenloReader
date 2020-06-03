using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bev.IO.MenloReader
{
    public class CombResult
    {
        #region Fields
        /// <summary>
        /// The result list for output and further evaluation.
        /// </summary>
        List<ResultPod> resultData;
        /// <summary>
        /// The (filtered) measurement data.
        /// </summary>
        CombData cd;
        /// <summary>
        /// The object for the actual calculation. 
        /// </summary>
        CwBeatCalculation cw;
        /// <summary>
        /// The used FXM counter.
        /// </summary>
        FxmNumber fxm;
        decimal? target;
        decimal? fixedIF;
        PlotType plotType = PlotType.None;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the result data.
        /// </summary>
        /// <value>The result data.</value>
        public List<ResultPod> ResultData { get { return resultData; } }

        /// <summary>
        /// Gets the array of column headers.
        /// </summary>
        /// <value>The column headers.</value>
        public string[] ColumnHeaders { get { return GenerateColumnHeadings(); } }

        /// <summary>
        /// Gets the statistics list.
        /// </summary>
        /// <value>The statistics.</value>
        public List<StatisticPod> Statistics { get { return GenerateStatistic(); } }

        public decimal[] YDataForPlot { get { return GenerateYDataForPlot(); } }

        public decimal[] XDataForPlot { get { return ExtractSingleColumn("LogTime"); } }

        public PlotType AuxType { get { return plotType; } }

        #endregion

        #region Ctor
        public CombResult(CombData cd, CwBeatCalculation cw, FxmNumber fxm, decimal? target, decimal? fixedIF)
        {
            this.cd = cd;
            this.cw = cw;
            this.fxm = fxm;
            this.target = target;
            this.fixedIF = fixedIF;
            resultData = new List<ResultPod>();
            GenerateResults();
            //Statistics();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Here the actual data evaluation takes place. The result is stored in <c>resultData</c>.
        /// </summary>
        void GenerateResults()
        {
            ResultPod rp;
            decimal chn1 = 0m; // raw IF
            decimal chn2 = 0m; // raw offset
            decimal chn3 = 0m; // beat
            foreach (var rdp in cd.MeasurementData)
            {
                rp = new ResultPod();
                rp.LogTime = rdp.LogTime;
                rp.AuxData0 = rdp.AuxData0;
                rp.AuxData1 = rdp.AuxData1;
                rp.OutputPower = rdp.OutputPower;
                rp.Status = rdp.Status;
                if (fxm == FxmNumber.Fxm0)
                {
                    chn1 = rdp.Counter0;
                    chn2 = rdp.Counter1;
                    chn3 = rdp.Counter2;
                }
                if (fxm == FxmNumber.Fxm1)
                {
                    chn1 = rdp.Counter4;
                    chn2 = rdp.Counter5;
                    chn3 = rdp.Counter6;
                }
                rp.RepetitionFrequency = cw.RepetitionFrequency(chn1);
                rp.SignedOffsetFrequency = cw.ActualOffsetFrequency(chn2);
                rp.SignedBeatFrequency = cw.SignBeat * chn3;
                rp.LaserFrequency = cw.AbsoluteLaserFrequency(chn1, chn2, chn3);
                // set optionals to null first
                rp.LaserFrequencyFixed = null;
                rp.DeltaLaserFrequency = null;
                rp.DeltaLaserFrequencyFixed = null;
                // now fill optional with value, if possible
                if (fixedIF != null)
                {
                    rp.LaserFrequencyFixed = cw.AbsoluteLaserFrequency((decimal)fixedIF, chn2, chn3);
                }
                if (target != null)
                {
                    rp.DeltaLaserFrequency = rp.LaserFrequency - target;
                    if (fixedIF != null) rp.DeltaLaserFrequencyFixed = rp.LaserFrequencyFixed - target;
                }
                resultData.Add(rp);
            }
        }

        /// <summary>
        /// Determines the column headers for the output file.
        /// </summary>
        /// <returns>The array of headings.</returns>
        /// <remarks>The first element of the <c>resultData</c> list is analyzed.</remarks>
        string[] GenerateColumnHeadings()
        {
            List<string> head = new List<string>();
            if (resultData.Count == 0) return head.ToArray();
            ResultPod rp = resultData[0];
            head.Add("Time / s");
            head.Add("Repetition frequency / Hz");
            head.Add("Offset frequency / Hz");
            head.Add("Beat frequency / Hz");
            if (rp.DeltaLaserFrequency == null)
            {
                head.Add("Laser frequency (using measured f_rep) / Hz");
                if (rp.LaserFrequencyFixed != null)
                    head.Add("Laser frequency (using set f_rep) / Hz");
            }
            if (rp.DeltaLaserFrequency != null)
                head.Add("Laser frequency (using measured f_rep) - target frequency / Hz");
            if (rp.DeltaLaserFrequencyFixed != null)
                head.Add("Laser frequency (using set f_rep) - target frequency / Hz");
            head.Add("Auxiliary channel 0 / V");
            head.Add("Auxiliary channel 1 / V");
            if (rp.OutputPower != null)
                head.Add("Optical output power / µW");
            if (rp.Status != LockStatus.Unknown)
                head.Add("Lock status");
            return head.ToArray();
        }

        /// <summary>
        /// Generates statistic values for each calculated and filtered quantity.
        /// </summary>
        /// <returns>The <c>List of StatisticPod</c>.</returns>
        List<StatisticPod> GenerateStatistic()
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
            StatisticPod sPod;
            List<StatisticPod> sPods = new List<StatisticPod>();
            foreach (string s in allProperties)
            {
                sPod = GenerateStatisticPod(s);
                if (sPod != null) sPods.Add(sPod);
            }
            return sPods;
        }

        /// <summary>
        /// Generates a single <c>StatisticPod</c>.
        /// </summary>
        /// <returns>The <c>StatisticPod</c>.</returns>
        /// <param name="propName">Name of property.</param>
        StatisticPod GenerateStatisticPod(string propName)
        {
            decimal[] singleCol = ExtractSingleColumn(propName);
            if (singleCol == null) return null;
            if (singleCol.Length <= 2) return null;
            StatisticPod sp = new StatisticPod();
            sp.QuantityName = propName;
            sp.Average = singleCol.Average();
            sp.StdDev = (decimal)singleCol.StandardDeviation();
            sp.Max = singleCol.Max();
            sp.Min = singleCol.Min();
            return sp;
        }

        /// <summary>
        /// Generates an array of the ordinates for a plot file.
        /// </summary>
        /// <returns>An array of the y-values.</returns>
        decimal[] GenerateYDataForPlot()
        {
            decimal[] singleCol;
            singleCol = ExtractSingleColumn("DeltaLaserFrequencyFixed");
            if (singleCol != null)
            {
                plotType = PlotType.DeltaLaserFrequencyFixed;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("DeltaLaserFrequency");
            if (singleCol != null)
            {
                plotType = PlotType.DeltaLaserFrequency;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("LaserFrequencyFixed");
            if (singleCol != null)
            {
                plotType = PlotType.LaserFrequencyFixed;
                return singleCol;
            }
            singleCol = ExtractSingleColumn("LaserFrequency");
            plotType = PlotType.LaserFrequency;
            return singleCol;
        }

        /// <summary>
        /// Extracts a single property as array.
        /// </summary>
        /// <param name="propName">The name of a property exported by <c>CombResult</c>. </param>
        /// <returns>A single column.</returns>
        decimal[] ExtractSingleColumn(string propName)
        {
            // check if resultData exists
            if (resultData == null) return null;
            if (resultData.Count == 0) return null;
            PropertyInfo prop;
            // now generate the extracted list and return
            List<decimal> col = new List<decimal>();
            foreach (var rdp in resultData)
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
        None, DeltaLaserFrequencyFixed, DeltaLaserFrequency, LaserFrequencyFixed, LaserFrequency
    }

}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bev.IO.MenloReader
{
    /// <summary>
    /// Implements methods used to calculate the frequency out of comb data files.
    /// The comb type must be provided.
    /// High level methods to determine parameters when laser is known to be of a MeP type.
    /// </summary>
    public class CwBeatCalculation
    {
        #region Fields
        CombType combType;  //
        int modeNumber;     // mode number for frequency calculation
        decimal signBeat;   // +1.0 or -1.0
        decimal signOff;    // +1.0 or -1.0
        //CclHFS? ccl;      // (optional) CCL HFS

        // comb type specific parameters
        decimal A; // multiplication factor of repetition frequency
        decimal B; // IF for repetition frequency
        decimal C; // harmonic factor of offset frequency
        decimal foSet; // fixed offset setting point
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the mode number for calculation of the absolute frequency.
        /// </summary>
        public int ModeNumber { get { return modeNumber; } set { modeNumber = value; } }

        /// <summary>
        /// Gets or sets the beat frequency sign for calculation of the absolute frequency.
        /// </summary>
        /// <remarks>Either +1.0 or -1.0.</remarks>
        public decimal SignBeat { get { return signBeat; } set { signBeat = value; } }

        /// <summary>
        /// Gets or sets the offset frequency sign for calculation of the absolute frequency.
        /// </summary>
        /// <remarks>Either +1.0 or -1.0.</remarks>
        public decimal SignOff { get { return signOff; } set { signOff = value; } }

        /// <summary>
        /// Gets the comb specific set point for the (fundamental) offset frequency in Hz.
        /// </summary>
        /// <remarks>Either 20 MHz or 40 MHz.</remarks>
        public decimal OffsetSetPoint { get { return foSet; } }

        /// <summary>
        /// Gets a textual description of the comb type used in the calculations.
        /// </summary>
        public string CombDescription { get { return CombString(combType); } }
        #endregion

        #region Ctor
        /// <summary>
        /// Instantiates a <c>CwBeatCalculation</c> object. Sets comb type specific fields.
        /// </summary>
        /// <param name="ct">The comb type used.</param>
        public CwBeatCalculation(CombType ct)
        {
            combType = ct;
            modeNumber = 0;
            //ccl = null;
            signBeat = +1m;
            signOff = +1m;
            switch (ct)
            {
                case CombType.BevFiberShg:
                    A = 4m;
                    B = 245000000m;
                    C = 2m;
                    foSet = 20000000m;
                    break;
                case CombType.BevFiber:
                    A = 4m;
                    B = 245000000m;
                    C = 1m;
                    foSet = 20000000m;
                    break;
                case CombType.BevUln:
                    A = 4m;
                    B = 245000000m;
                    C = 1m;
                    foSet = 35000000m;
                    break;
                case CombType.BevUlnShg:
                    A = 4m;
                    B = 245000000m;
                    C = 2m;
                    foSet = 35000000m;
                    break;
                case CombType.BevTiSa:
                    A = 1m;
                    B = 970000000m;
                    C = 1m;
                    foSet = 40000000m;
                    break;
                case CombType.CmiTiSa:
                    A = 5m;
                    B = 196000000m;
                    C = 1m;
                    foSet = 20000000m;
                    break;
                default:
                    // an ideal offset-free direct comb
                    A = 1m;
                    B = 0m;
                    C = 1m;
                    foSet = 0m;
                    break;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Given the beat frequency, the synthesizer frequency and a guess for the target frequency this method estimates the <c>modeNumber</c>.
        /// The signs <c>signOff</c> and <c>signBeat</c> must be set in advance.
        /// </summary>
        /// <remarks>Sets found value for <c>modeNumber</c> in the respective field.</remarks>
        /// <param name="f_target">The estimated laser frequency in Hz.</param>
        /// <param name="f_syn">Synthesizer frequency in Hz.</param>
        /// <param name="f_beat">Unsigned beat frequency in Hz.</param>
        /// <returns>The mode number.</returns>
        public int PredictModeNumber(decimal f_target, decimal f_syn, decimal f_beat)
        {
            decimal f_rep = (f_syn / A + B);
            int mn = (int)Math.Round((f_target - signOff * C * foSet - signBeat * f_beat) / f_rep, MidpointRounding.AwayFromZero);
            modeNumber = mn; // the respective field is set here!
            return mn;
        }

        /// <summary>
        /// Given the beat frequency, the synthesizer frequency, the <c>modeNumber</c> and a guess for the target frequency this method estimates the two signs <c>signOff</c>, <c>signBeat</c>
        /// </summary>
        /// <remarks>Sets found values for <c>modeNumber</c>, <c>signOff</c>, <c>signBeat</c> in the respective fields.</remarks>
        /// <param name="f_target">The estimated laser frequency in Hz.</param>
        /// <param name="f_syn">Synthesizer frequency in Hz.</param>
        /// <param name="f_beat">Unsigned beat frequency in Hz.</param>
        /// <returns>Residual of actual to estimated laser frequency in Hz.</returns>
        public decimal PredictSigns(decimal f_target, decimal f_syn, decimal f_beat)
        {
            List<decimal> delta = new List<decimal>();
            List<int> mode = new List<int>();
            List<decimal> listSignBeat = new List<decimal>();
            List<decimal> listSignOff = new List<decimal>();

            signBeat = +1m; signOff = +1m;
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = +1m; signOff = -1m;
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = -1m; signOff = +1m;
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = -1m; signOff = -1m;
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            int idx = delta.IndexOf(delta.Min());

            signBeat = listSignBeat[idx];
            signOff = listSignOff[idx];

            return delta.Min();
        }

        /// <summary>
        /// Given the beat frequency, the synthesizer frequency and a guess for the target frequency this method estimates the <c>modeNumber</c> and the two signs <c>signOff</c>, <c>signBeat</c>
        /// </summary>
        /// <remarks>Sets found values for <c>modeNumber</c>, <c>signOff</c>, <c>signBeat</c> in the respective fields.</remarks>
        /// <param name="f_target">The estimated laser frequency in Hz.</param>
        /// <param name="f_syn">Synthesizer frequency in Hz.</param>
        /// <param name="f_beat">Unsigned beat frequency in Hz.</param>
        /// <returns>Residual of actual to estimated laser frequency in Hz.</returns>
        public decimal PredictModeAndSigns(decimal f_target, decimal f_syn, decimal f_beat)
        {
            List<decimal> delta = new List<decimal>();
            List<int> mode = new List<int>();
            List<decimal> listSignBeat = new List<decimal>();
            List<decimal> listSignOff = new List<decimal>();

            signBeat = +1m; signOff = +1m;
            PredictModeNumber(f_target, f_syn, f_beat);
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = +1m; signOff = -1m;
            PredictModeNumber(f_target, f_syn, f_beat);
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = -1m; signOff = +1m;
            PredictModeNumber(f_target, f_syn, f_beat);
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            signBeat = -1m; signOff = -1m;
            PredictModeNumber(f_target, f_syn, f_beat);
            mode.Add(modeNumber);
            listSignBeat.Add(signBeat);
            listSignOff.Add(signOff);
            delta.Add(Math.Abs(f_target - AbsoluteLaserFrequency(f_syn, foSet, f_beat)));

            int idx = delta.IndexOf(delta.Min());

            modeNumber = mode[idx];
            signBeat = listSignBeat[idx];
            signOff = listSignOff[idx];

            return delta.Min();
        }

        /// <summary>
        /// Given the beat frequency and the synthesizer frequency this method estimates the HFS for a 633 nm MeP laser, the <c>modeNumber</c> and the two signs <c>signOff</c>, <c>signBeat</c>
        /// </summary>
        /// <remarks>
        /// Sets found values for <c>modeNumber</c>, <c>signOff</c>, <c>signBeat</c> in the respective fields.
        /// Does not work correctly for all combinations!
        /// </remarks>
        /// <param name="f_syn">Synthesizer frequency in Hz.</param>
        /// <param name="f_beat">Unsigned beat frequency in Hz.</param>
        /// <returns>The predicted HFS.</returns>
        public CclHFS PredictModeAndSigns633Hfs(decimal f_syn, decimal f_beat)
        {
            List<decimal> delta = new List<decimal>();
            List<int> mode = new List<int>();
            List<decimal> listSignBeat = new List<decimal>();
            List<decimal> listSignOff = new List<decimal>();
            List<CclHFS> hfs = new List<CclHFS>();

            for (CclHFS i = CclHFS.F633a; i <= CclHFS.F633n; i++)
            {
                decimal d = PredictModeAndSigns(CwCcl.Frequency(i), f_syn, f_beat);
                delta.Add(d);
                hfs.Add(i);
                listSignBeat.Add(signBeat);
                listSignOff.Add(signOff);
                mode.Add(modeNumber);
            }

            int idx = delta.IndexOf(delta.Min());
            modeNumber = mode[idx];
            signBeat = listSignBeat[idx];
            signOff = listSignOff[idx];
            return hfs[idx];
        }

        /// <summary>
        /// Calculates the absolute laser frequency using standard <c>decimal</c> arithmetic.
        /// The signs <c>signOff</c> and <c>signBeat</c> as well as the <c>modeNumber</c> must be set in advance.
        /// </summary>
        /// <param name="f_syn">Synthesizer frequency in Hz.</param>
        /// <param name="f_off">Unsigned offset frequency as measured by counter in Hz.</param>
        /// <param name="f_beat">Unsigned beat frequency in Hz.</param>
        /// <returns>The laser frequency in Hz.</returns>
        public decimal AbsoluteLaserFrequency(decimal f_syn, decimal f_off, decimal f_beat)
        {
            decimal f_rep = RepetitionFrequency(f_syn);
            decimal x = (decimal)modeNumber * f_rep + ActualOffsetFrequency(f_off) + signBeat * f_beat;
            return x;
        }

        /// <summary>
        /// The comb specific repetition frequency.
        /// </summary>
        /// <returns>The repetition frequency.</returns>
        /// <param name="f_syn">The frequency measured by the IF mixer.</param>
        public decimal RepetitionFrequency(decimal f_syn)
        {
            return (f_syn / A + B);
        }

        /// <summary>
        /// The comb and experiment specific offset frequency.
        /// </summary>
        /// <returns>The offset frequency.</returns>
        /// <param name="f_off">The frequency measured by the f-2f interferometer.</param>
        public decimal ActualOffsetFrequency(decimal f_off)
        {
            return signOff * C * f_off;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Provides a textual description for the comb type enum.
        /// </summary>
        /// <returns>The description.</returns>
        /// <param name="ct">The comb type.</param>
        private string CombString(CombType ct)
        {
            switch (ct)
            {
                case CombType.Generic:
                    return "Generic (ideal) comb type";
                case CombType.BevFiberShg:
                    return "BEV FC1500-0-053 fiber comb, frequency doubled output";
                case CombType.BevFiber:
                    return "BEV FC1500-0-053 fiber comb, fundamental output";
                case CombType.BevTiSa:
                    return "BEV FC8003/01 Ti:Sa comb";
                case CombType.CmiTiSa:
                    return "CMI FC8004 Ti:Sa comb";
                case CombType.BevUln:
                    return "BEV FC1500-250-ULN fiber comb, fundamental output";
                case CombType.BevUlnShg:
                    return "BEV FC1500-250-ULN fiber comb, frequency doubled output";
                default:
                    return "Unknown (generic) comb type";
            }
        }
        #endregion
    }
}

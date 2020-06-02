namespace Bev.IO.MenloReader
{
    /// <summary>
    /// Enumeration of CIPM recommended iodine HFS components
    /// </summary>
    public enum CclHFS
    {
        None,
        F633a,
        F633b,
        F633c,
        F633d,
        F633e,
        F633f,
        F633g,
        F633h,
        F633i,
        F633j,
        F633k,
        F633l,
        F633m,
        F633n,
        F532a2,
        F532a10
    }

    /// <summary>
    /// Class for use of selected iodine hyper fine structure transitions.
    /// Values recommended by the CIPM
    /// </summary>
    public static class CwCcl
    {
        /// <summary>
        /// Returns the frequency in Hz of the provided HFS.
        /// </summary>
        /// <param name="comp">The HFS component.</param>
        public static decimal Frequency(CclHFS comp)
        {
            switch (comp)
            {
                case CclHFS.F633a:
                    return 473612514643000;
                case CclHFS.F633b:
                    return 473612505812000;
                case CclHFS.F633c:
                    return 473612497718000;
                case CclHFS.F633d:
                    return 473612379828000;
                case CclHFS.F633e:
                    return 473612366967000;
                case CclHFS.F633f:
                    return 473612353604000;
                case CclHFS.F633g:
                    return 473612340406000;
                case CclHFS.F633h:
                    return 473612236651000;
                case CclHFS.F633i:
                    return 473612214712000;
                case CclHFS.F633j:
                    return 473612193147000;
                case CclHFS.F633k:
                    return 473612084762000;
                case CclHFS.F633l:
                    return 473612076718000;
                case CclHFS.F633m:
                    return 473612060911000;
                case CclHFS.F633n:
                    return 473612051898000;
                case CclHFS.F532a2:
                    return 563259911669000;
                case CclHFS.F532a10:
                    return 563260223513000;
                default:
                    return 0;
            }
        }
    }
}

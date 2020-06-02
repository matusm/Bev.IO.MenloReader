using System;

namespace Bev.IO.MenloReader
{
    [Flags]
    public enum OutlierType
    {
        None = 0,
        RepetitionRate = 1,
        OffSet = 2,
        CycleSlip = 4,
        BandPass = 8,
        LaserUnlocked = 16
    }

    /// <summary>
    /// The BEV software records values of two concatenated FXM counters.
    /// </summary>
    public enum FxmNumber
    {
        Unknown,
        Fxm0,
        Fxm1
    }

    public enum CombType
    {
        Generic,
        BevFiberShg,
        BevFiber,
        BevTiSa,
        CmiTiSa,
        BevUln,
        BevUlnShg
    }

    /// <summary>
    /// Some lasers provide a voltage to signal the lock status (WEO M100).
    /// </summary>
    public enum LockStatus
    {
        Unknown,
        Locked,
        Unlocked
    }

}

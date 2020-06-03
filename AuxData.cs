using System.Collections.Generic;

namespace Bev.IO.MenloReader
{
    public class AuxData
    {
        private readonly List<AuxPod> auxList;

        public AuxPod[] AuxPods => auxList.ToArray();

        public AuxData(decimal[] x, decimal[] y)
        {
            auxList = new List<AuxPod>();
            if (x.Length == y.Length)
            {
                for (int i = 0; i < x.Length; i++)
                    auxList.Add(new AuxPod(x[i], y[i]));
            }
        }
    }
}

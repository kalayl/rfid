using System;

namespace Reader
{
    public class PhaseUtils
    {
        public float PhaseAngle(string rawPhase)
        {
            var a = Convert.ToInt32(rawPhase, 16);
            if (a > 32768)
            {
                a = a - 32768;
            }
            
            var phasea = (((float)a) / 32768) * 180;

            return phasea;
        }

    }
}

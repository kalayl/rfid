using System;

namespace Reader
{
    public class Reading
    {
        public string TagId { get; set; }
        public float PhaseAngle { get; set; }
        public string Antenna { get; set; }
        public string Time { get; set; }

        public override string ToString()
        {
            return String.Format("[TagId: {0}, PhaseAngle: {1}, Antenna: {2}, Time: {3}]", TagId, PhaseAngle, Antenna, Time);
        }
    }
}

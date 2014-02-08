using System;

namespace Reader
{
    public class Reading
    {
        public string TagId { get; set; }
        public float PhaseAngle { get; set; }
        public string Antenna { get; set; }
        public string Time { get; set; }
        public string Frequency { get; set; }
        public string Rssi { get; set; }

        public override string ToString()
        {
            return String.Format("[TagId: {0}, PhaseAngle: {1}, Antenna: {2}, Frequency: {3}, Rssi: {4}, Time: {5}]", TagId, PhaseAngle, Antenna, Frequency, Rssi, Time);
        }
    }
}

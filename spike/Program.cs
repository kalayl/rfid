using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reader;

namespace rfid
{
    public class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = args.Length > 0 ? args[0]: "192.168.0.111";

            var phaseUtils = new PhaseUtils();
            for (int i = 0; i < 250; i++)
            {
                var readerService = new ReaderService(ipAddress, "admin", "readeradmin");
                using (var eventReader = new EventReader(phaseUtils, readerService))
                {
                    eventReader.Run();
                    Thread.Sleep(3000);
                    ProcessResults(i, eventReader.PerAntennaReadings);
                }
            }
            

            Console.WriteLine("Exiting");
            Console.ReadLine();
        }

        private static void ProcessResults(int iteration, IDictionary<string, List<Reading>> perAntennaReadings)
        {
            var antenna1 = new List<Reading>();
            var antenna2 = new List<Reading>();
            var phaseDifference12 = new List<float>();
            if (!perAntennaReadings.TryGetValue("1", out antenna1))
            {
                throw new Exception("Could not retrieve antenna1");
            }
            if (!perAntennaReadings.TryGetValue("2", out antenna2))
            {
                throw new Exception("Could not retrieve antenna2");
            }
            var antenna1Array = antenna1.ToArray();
            var antenna2Array = antenna2.ToArray();

            // calculate the phase difference
            for (int i = 0; i < antenna1.Count; i++)
            {
                if (i < antenna2Array.Length && i < antenna2Array.Length)
                {
                    float phaseDifference = antenna2Array[i].PhaseAngle - antenna1Array[i].PhaseAngle;
                    if (Math.Abs(phaseDifference) > 90)
                    {
                        phaseDifference = phaseDifference - (180*Math.Sign(phaseDifference));
                    }
                    phaseDifference12.Add(phaseDifference);
                }
            }

            // average phase difference
            var averagePhaseDifference = phaseDifference12.Sum() / phaseDifference12.Count;

            var xPosition = Convert.ToInt32(Math.Floor((averagePhaseDifference + 90) * 7.6));

            Console.WriteLine("{0} => xPosition: {1}", iteration, xPosition);
        }
    }
}

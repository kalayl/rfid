using System;
using System.Threading;
using Reader;

namespace rfid
{
    public class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = args.Length > 0 ? args[0]: "192.168.0.115";

            PhaseUtils phaseUtils = new PhaseUtils();
            using (var eventReader = new EventReader(ipAddress, phaseUtils))
            {
                eventReader.Run();
                Thread.Sleep(2000);
            }

            Console.WriteLine("Exiting");
            Console.ReadLine();
        }
    }
}

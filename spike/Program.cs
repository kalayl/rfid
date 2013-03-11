using System;
using System.Globalization;
using System.Threading;
using Reader;
using Sirit.Mapping;

namespace rfid
{
    class Program
    {
        /// <summary>
        /// IP Address of the reader
        /// </summary>
        private string m_ipAddress = "192.168.0.122";

        /// <summary>
        /// Constructor
        /// </summary>
        public Program()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipAddress">IP Address of the reader</param>
        public Program(string ipAddress)
        {
            m_ipAddress = ipAddress;
        }

        /// <summary>
        /// Executes the program
        /// </summary>
        public void Run()
        {
            try
            {
                var readerService = new ReaderService(m_ipAddress, "admin", "readeradmin");
                readerService.Activate();
                var eventRegistrationId = readerService.RegisterHandler("event.tag.report", EventReceivedHandler);
               

                // Sleep while handling tag events
                Thread.Sleep(500);

                readerService.Standby();

                readerService.UnregisterHandler(eventRegistrationId, "event.tag.report");
                readerService.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error: " + exc.Message);
            }
        }

        /// <summary>
        /// Main method for the application
        /// </summary>
        /// <param name="args">Command line arguments
        /// first argument - IP Address of the reader (optional)</param>
        static void Main(string[] args)
        {
            // Create the program object
            Program prog;
            if (args.Length > 0)
                prog = new Program(args[0]);
            else
                prog = new Program();
            // Execute the application
            prog.Run();
            Console.WriteLine("Exiting");
            Console.ReadLine();
        }

        private float min = 360;
        private float max = 0;

        /// <summary>
        /// EventFound delegate is used to notify interested entities whenever an 
        /// event is triggered on the reader
        /// </summary>
        public void EventReceivedHandler(object sender, EventInfo eventInfo)
        {
            if (eventInfo.Type == EventInfo.EventTypes.TAG_REPORT)
            {
                string tagid = eventInfo.GetParameter(EventInfo.EventTagReportParams.TagId);
                string phase = eventInfo.GetParameter(EventInfo.EventTagReportParams.Phase);
                string antenna = eventInfo.GetParameter(EventInfo.EventTagReportParams.Antenna);
                string time = eventInfo.GetParameter(EventInfo.EventTagReportParams.Time);

                var a = Convert.ToInt32(phase, 16);
                Console.WriteLine(a);
                if (a > 32768)
                {
                    a = a - 32768;
                }
                float phasea = (float)(((float)a) / 32768) * 180;
                Console.WriteLine(phasea);
                

                var i = Int16.Parse(phase.Substring(2), NumberStyles.HexNumber);
                var f = ((float)i) / 32768 * 360;
                if (f > max)
                {
                    max = f;
                }
                if (f < min)
                {
                    min = f;
                }

                if (tagid != null)
                {
                    //Console.WriteLine("TagID: {0}; Antenna: {1}; Time: {2} ", tagid, antenna, time);
                    Console.WriteLine("Phase: {0}; phase-deg: {1}; max: {2}; min: {3} ", phase, phasea, max, min);
                }
            }
            
        }
    }
}

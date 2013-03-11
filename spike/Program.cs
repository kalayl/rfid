using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Sirit.Data;
using Sirit.Driver;
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
                // Open a connection to the reader
                DataManager dataManager = new DataManager(DataManager.ConnectionTypes.SOCKET, m_ipAddress, 0);
                dataManager.OpenConnection();
                Console.WriteLine("Connection Opened");

                // Get the reader's name
                InfoManager infoManager = new InfoManager(dataManager);
                String v = infoManager.Name;
                Console.WriteLine("Name: " + v);
                infoManager = null;

                // Login as administrator
                ReaderManager readerManager = new ReaderManager(dataManager);
                if (!readerManager.Login("admin", "readeradmin"))
                    throw new Exception("Login attempt failed: " + readerManager.LastErrorMessage);
                v = readerManager.WhoAmI();
                Console.WriteLine("Login: " + v);

                // Open an event channel and get it's ID
                String id = dataManager.GetEventChannel(new EventFound(EventReceivedHandler));
                Console.WriteLine("Event Channel ID: " + id);

                // Register for event.tag.report
                if (!readerManager.EventsRegister(id, "event.tag.report"))
                    throw new Exception("Failure to register for event: " + readerManager.LastErrorMessage);
                Console.WriteLine("Registered for event.tag.report");

                // Set operating mode to active
                SetupManager setupManager = new SetupManager(dataManager);
                setupManager.OperatingMode = SetupManager.OperatingModeTypes.ACTIVE;
                Console.WriteLine("Operating Mode: Active");

                // Sleep while handling tag events
                Thread.Sleep(500);

                // Set operating mode to standby
                setupManager.OperatingMode = SetupManager.OperatingModeTypes.STANDBY;
                Console.WriteLine("Operating Mode: Standby");

                // Unregister for event.tag.report
                if (!readerManager.EventsUnregister(id, "event.tag.report"))
                    throw new Exception("Failure to unregister for event: " + readerManager.LastErrorMessage);
                Console.WriteLine("Unregistered for event.tag.report");
               
                // Close the connection
                setupManager = null;
                readerManager = null;
                dataManager.Close();
                Console.WriteLine("Connection Closed");
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
                Convert.ToInt32(phase);
                var i = Int16.Parse(phase.Substring(2), NumberStyles.HexNumber);
                var f = ((float) i)/32768*360;
                if (f > max)
                {
                    max = f;
                }
                if (f < min)
                {
                    min = f;
                }
                
                if (tagid != null)
                    Console.WriteLine("TagID: {0}; Antenna: {1}; Time: {2} ", tagid, antenna, time);
                    Console.WriteLine("Phase: {0}; phase-deg: {1}; max: {2}; min: {3} ", phase, f, max, min);
            }
            
        }
    }
}

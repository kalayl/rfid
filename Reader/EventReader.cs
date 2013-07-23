using System;
using System.Threading;
using Sirit.Mapping;

namespace Reader
{
    public class EventReader : IDisposable
    {
        // Track whether Dispose has been called. 
        private bool _disposed = false;

        private readonly string _ipAddress;
        private readonly PhaseUtils _phaseUtils;
        private readonly ReaderService _readerService;
        private string _eventRegistrationId;

        public EventReader(string ipAddress, PhaseUtils phaseUtils)
        {
            _ipAddress = ipAddress;
            _phaseUtils = phaseUtils;

            _readerService = new ReaderService(_ipAddress, "admin", "readeradmin");
        }

        public void Run()
        {
            try
            {
                _readerService.Activate();
                _eventRegistrationId = _readerService.RegisterHandler("event.tag.report", EventReceivedHandler);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error: " + exc.Message);
            }
        }

        public void EventReceivedHandler(object sender, EventInfo eventInfo)
        {
            if (eventInfo.Type == EventInfo.EventTypes.TAG_REPORT)
            {
                string tagid = eventInfo.GetParameter(EventInfo.EventTagReportParams.TagId);
                string antenna = eventInfo.GetParameter(EventInfo.EventTagReportParams.Antenna);
                string time = eventInfo.GetParameter(EventInfo.EventTagReportParams.Time);
                string phase = eventInfo.GetParameter(EventInfo.EventTagReportParams.Phase);
                float phaseAngle = _phaseUtils.PhaseAngle(phase);

                if (tagid != null)
                {
                    var reading = new Reading
                    {
                        TagId = tagid,
                        Antenna = antenna,
                        PhaseAngle = phaseAngle,
                        Time = time
                    };

                    Console.WriteLine(reading);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~EventReader()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                if (disposing)
                {
                    _readerService.Standby();
                    _readerService.UnregisterHandler(_eventRegistrationId, "event.tag.report");
                    _readerService.Close();

                    Console.WriteLine("Closed Reader Service.");
                }
                _disposed = true;
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Sirit.Mapping;
using Sirit.Mapping.Modem;

namespace Reader
{
    public interface IEventReader
    {
        ObservableCollection<Reading> Readings { get; set; }
        IDictionary<string, List<Reading>> PerAntennaReadings { get; set; }
    }

    public class EventReader : IDisposable, IEventReader
    {
        // Track whether Dispose has been called. 
        private bool _disposed;

        private readonly PhaseUtils _phaseUtils;
        private readonly IReaderService _readerService;
        private string _eventRegistrationId;

        public ObservableCollection<Reading> Readings { get; set; }
        public IDictionary<string, List<Reading>> PerAntennaReadings { get; set; }
 
        public EventReader(PhaseUtils phaseUtils, IReaderService readerService)
        {
            _phaseUtils = phaseUtils;
            _readerService = readerService;

            Readings = new ObservableCollection<Reading>();
            PerAntennaReadings = new Dictionary<string, List<Reading>>();

            Readings.CollectionChanged += ReadingsChanged;
        }

        private void ReadingsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var reading = ((Reading)item);
                var readings = GetAntennaReadings(reading.Antenna);
                readings.Add(reading);

                //Console.WriteLine(reading.Antenna + " " + readings.Count);
            }            
        }

        private List<Reading> GetAntennaReadings(string antenna)
        {
            List<Reading> readings;
            if (!PerAntennaReadings.TryGetValue(antenna, out readings))
            {
                readings = new List<Reading>();
                PerAntennaReadings.Add(antenna, readings);
            }

            return readings;
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
                var tagid = eventInfo.GetParameter(EventInfo.EventTagReportParams.TagId);
                var antenna = eventInfo.GetParameter(EventInfo.EventTagReportParams.Antenna);
                var time = eventInfo.GetParameter(EventInfo.EventTagReportParams.Time);
                var phase = eventInfo.GetParameter(EventInfo.EventTagReportParams.Phase);
                var frequency = eventInfo.GetParameter(EventInfo.EventTagReportParams.Frequency);
                var rssi = eventInfo.GetParameter(EventInfo.EventTagReportParams.Rssi);
                
                var phaseAngle = _phaseUtils.PhaseAngle(phase);

                if (tagid != null)
                {
                    var reading = new Reading
                    {
                        TagId = tagid,
                        Antenna = antenna,
                        PhaseAngle = phaseAngle,
                        Frequency = frequency,
                        Rssi = rssi,
                        Time = time
                    };

                    //Console.WriteLine(reading);
                    Readings.Add(reading);
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

                    //Console.WriteLine("Closed Reader Service.");
                }

                _disposed = true;
            }
        }

    }
}
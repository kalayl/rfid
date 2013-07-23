using System;
using Sirit.Data;
using Sirit.Driver;
using Sirit.Mapping;

namespace Reader
{
    public class ReaderService
    {
        private readonly string _ipAddress;
        private readonly string _login;
        private readonly string _password;

        private DataManager _dataManager;
        private InfoManager _infoManager;
        private ReaderManager _readerManager;
        private SetupManager _setupManager;

        public ReaderService(string ipAddress, string login, string password)
        {
            _ipAddress = ipAddress;
            _login = login;
            _password = password;

            SetupManagers();
        }

        private void SetupManagers()
        {
            // Open a connection to the reader
            _dataManager = new DataManager(DataManager.ConnectionTypes.SOCKET, _ipAddress, 0);
            _dataManager.OpenConnection();
            Console.WriteLine("Connection Opened");

            // Get the reader's name
            _infoManager = new InfoManager(_dataManager);
            String v = _infoManager.Name;
            Console.WriteLine("Name: " + v);
            _infoManager = null;

            // Login as administrator
            _readerManager = new ReaderManager(_dataManager);
            if (!_readerManager.Login(_login, _password))
            {
                throw new Exception("Login attempt failed: " + _readerManager.LastErrorMessage);
            }

            v = _readerManager.WhoAmI();
            Console.WriteLine("Login: " + v);
        }

        public void Close()
        {
            // Close the connection
            _setupManager = null;
            _readerManager = null;
            _infoManager = null;

            _dataManager.Close();
            Console.WriteLine("Connection Closed");
        }

        public String RegisterHandler(string tagEventName, EventFound eventReceivedHandler)
        {
            // Open an event channel and get it's ID
            String id = _dataManager.GetEventChannel(eventReceivedHandler);
            Console.WriteLine("Event Channel ID: " + id);

            // Register for event.tag.report
            if (!_readerManager.EventsRegister(id, tagEventName))
                throw new Exception("Failure to register for event: " + _readerManager.LastErrorMessage);
            Console.WriteLine("Registered for " + tagEventName);

            return id;
        }

        public void Activate()
        {
            // Set operating mode to active
            _setupManager = new SetupManager(_dataManager);
            _setupManager.OperatingMode = SetupManager.OperatingModeTypes.ACTIVE;
            Console.WriteLine("Operating Mode: Active");
        }

        public void Standby()
        {
            // Set operating mode to standby
            _setupManager.OperatingMode = SetupManager.OperatingModeTypes.STANDBY;
            Console.WriteLine("Operating Mode: Standby");
        }

        public void UnregisterHandler(string eventRegistrationId, string tagEventName)
        {
            // Unregister for event.tag.report
            if (!_readerManager.EventsUnregister(eventRegistrationId, tagEventName))
                throw new Exception("Failure to unregister for event: " + _readerManager.LastErrorMessage);
            Console.WriteLine("Unregistered for " + tagEventName);
        }
    }
}

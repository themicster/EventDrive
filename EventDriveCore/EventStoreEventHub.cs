using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EventDriveCore.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventDriveCore
{
    public class EventStoreEventHub : IEventHub
    {
        private string _connectionString;
        private string _connectionName;
        private string _streamName;
        private IEventStoreConnection _conn;
        private long _nextEventId = 0;
        public EventStoreEventHub(string connectionString, string streamName, string connectionName = "EventDriveEventHub")
        {
            _connectionString = connectionString;
            _connectionName = connectionName;
            _streamName = streamName;
            _conn = EventStoreConnection.Create(new Uri(_connectionString), _connectionName);
        }
        public void Connect()
        {
            _conn.ConnectAsync().Wait();
        }
        public void Disconnect()
        {
            _conn.Close();
        }
        public List<Event> GetNextEvents()
        {
            List<Event> events = new List<Event>();
            try
            {
                StreamEventsSlice readEvents = _conn.ReadStreamEventsForwardAsync(_streamName, _nextEventId, 100, true).Result;
                foreach (var evt in readEvents.Events)
                {
                    string strData = Encoding.UTF8.GetString(evt.Event.Data);
                    Console.WriteLine(strData);
                    Event myEvent = new Event();
                    myEvent.EventName = evt.Event.EventType;
                    myEvent.EventData = JsonConvert.DeserializeObject(strData);
                    events.Add(myEvent);
                }
                _nextEventId = readEvents.NextEventNumber;
                if(readEvents.Status == SliceReadStatus.StreamNotFound)
                {
                    SendEvent(new EventStreamCreated(_streamName));
                    _nextEventId = 0;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return events;
        }
        public void SendEvent(Event evt)
        {
            // TODO: Is conn threadsafe?
            string dataStr = JsonConvert.SerializeObject(evt.EventData);
            EventData ed = new EventData(Guid.NewGuid(), evt.EventName, true, Encoding.ASCII.GetBytes(dataStr), Encoding.ASCII.GetBytes(""));
            _conn.AppendToStreamAsync(_streamName, ExpectedVersion.Any, ed).Wait();
        }
        public void Dispose()
        {
            _conn.Close();
        }
    }
}

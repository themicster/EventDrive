using System;

namespace EventDriveCore.Events
{
    public class EventStreamCreated : Event
    {
        public EventStreamCreated(string streamName)
        {
            EventName = "EventStreamCreated";
            EventData.streamName = streamName;
        }

        public static void OnEventStreamCreated(Event evt)
        {
            Console.WriteLine("Got EventStreamCreated");
        }
    }
}

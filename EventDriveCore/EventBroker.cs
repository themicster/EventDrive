using System;
using System.Collections.Generic;
using EventDriveCore;

namespace EventDriveCore
{
    public class EventBroker
    {
        public List<Event> Events = new List<Event>();
        public Dictionary<string, EventHandler<Event>> Handlers = new Dictionary<string,EventHandler<Event>>();

        public void AddHandler(string type, EventHandler<Event> handler)
        {
            if(Handlers.ContainsKey(type))
            {
                Handlers[type] += handler;
            }
            else
            {
                EventHandler<Event> newHandler = new EventHandler<Event>(handler);
                Handlers.Add(type, newHandler);
            }
        }
        public void Send(Event evt)
        {
            if(Handlers.ContainsKey(evt.GetType().ToString()))
            {
                EventHandler<Event> handler = Handlers[evt.GetType().ToString()];
                handler?.Invoke(this, evt);
            }
        }
    }
}
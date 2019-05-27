using System;
using System.Collections.Generic;

namespace EventDriveCore
{
    public interface IEventHub : IDisposable
    {
        void Connect();
        void Disconnect();
        List<Event> GetNextEvents();
        void SendEvent(Event evt);
    }
}

using System;
using System.Security;
using EventDriveCore;

namespace EventDriveCore
{
    [SecuritySafeCritical]
    public class MessagePayload<T> where T : Event
    {
        public object Who { get; private set; }
        public T What { get; private set; }
        public DateTime When { get; private set; }
        public MessagePayload(T payload, object source)
        {
            Who = source; What = payload; When = DateTime.UtcNow;
        }
    }
}

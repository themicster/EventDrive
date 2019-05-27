using System;
using System.Security;
using System.Dynamic;

namespace EventDriveCore
{
    [SecuritySafeCritical]
    public class Event
    {
        public string EventName;
        public bool PersistToDisk = false;
        public bool SendToHub = false;
        public bool RequireACK = false;
        public bool SameThread = false;
        public bool CanCache = false;
        public bool Secure = false;
        public dynamic EventData = new ExpandoObject();

        public Event()
        {

        }
    }
}

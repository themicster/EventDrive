using System;
using EventDriveCore;

namespace ConsoleSample
{
    public class GetCustomerNameQuery : Event
    {
        public GetCustomerNameQuery()
        {
            EventData.EventName = "GetCustomerNameQuery";
            EventName = EventData.EventName;
            SameThread = false;
            SendToHub = true;
        }
        public static void OnGetCustomerNameQuery(Event evt)
        {
            GetCustomerNameResponse gcnr = new GetCustomerNameResponse("Michael Tickle");
            Console.WriteLine($"Sending the name you are looking for {gcnr.EventData.Name}");
            DefaultMessageBroker.Instance.Publish("GetCustomerNameResponse", evt, gcnr);
        }

    }
}

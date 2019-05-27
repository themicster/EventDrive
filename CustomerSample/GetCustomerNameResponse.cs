using System;
using EventDriveCore;

namespace ConsoleSample
{
    public class GetCustomerNameResponse : Event
    {
        public GetCustomerNameResponse(string name)
        {
            EventName = "GetCustomerNameResponse";
            EventData.Name = name;
            SendToHub = true;
        }

        public static void OnGetCustomerNameResponse(Event evt)
        {
            Console.WriteLine($"We got the name we were looking for {evt.EventData.Name}");
        }
    }
}

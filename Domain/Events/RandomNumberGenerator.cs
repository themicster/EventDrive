using System;
using System.Security;
using EventDriveCore;

namespace ConsoleSample
{
    [SecuritySafeCritical]
    public class RandomNumberGeneratorEvent : Event
    {
        IMessageBroker _messageBroker;
        public RandomNumberGeneratorEvent(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
            PersistToDisk = false;
            SendToHub = false;
            SameThread = false;
            Random rnd = new Random();
            EventName = "RandomNumberGeneratorEvent";
            EventData.Number = rnd.Next();
        }
        public void Publish()
        {
            _messageBroker.Subscribe(EventName, new Action<Event>(this.OnRandomNumberGeneratorEventInstance));
            _messageBroker.Publish(EventName, this, this);
        }
        public void OnRandomNumberGeneratorEventInstance(Event evt)
        {
            Console.WriteLine($"Got our instance event: {evt.EventName}, {evt.EventData.Number}");
        }
        public static void OnRandomNumberGeneratorEvent(Event evt)
        {
            Console.WriteLine($"RandomNumberGeneratorEvent: {evt.EventData.Number}");
        }
        public static void OnRandomNumberGeneratorBeforeEvent(Event evt)
        {
            Console.WriteLine("Before RandomNumberGeneratorEvent");
        }
        public static void OnRandomNumberGeneratorAfterEvent(Event evt)
        {
            Console.WriteLine("After RandomNumberGeneratorEvent");
        }
    }
    [SecuritySafeCritical]
    public class RandomNumberGeneratorRequest : Event
    {
        public RandomNumberGeneratorRequest()
        {
            PersistToDisk = false;
            SendToHub = false;
            SameThread = true;
            EventName = "RandomNumberGeneratorRequest";
        }
        public static void OnRandomNumberGeneratorRequest(Event evt)
        {
            Console.WriteLine($"Received RandomNumberGeneratorRequest Message");
            RandomNumberGeneratorResponse response = new RandomNumberGeneratorResponse();
            DefaultMessageBroker.Instance.Publish("RandomNumberGeneratorResponse", evt, response);
        }
        public static void OnRandomNumberGeneratorBeforeRequest(Event evt)
        {
            Console.WriteLine("Before RandomNumberGeneratorRequest");
        }
        public static void OnRandomNumberGeneratorAfterRequest(Event evt)
        {
            Console.WriteLine("After RandomNumberGeneratorRequest");
        }
    }
    [SecuritySafeCritical]
    public class RandomNumberGeneratorResponse : Event
    {
        public RandomNumberGeneratorResponse()
        {
            PersistToDisk = false;
            SendToHub = false;
            SameThread = true;
            Random rnd = new Random();
            
            EventName = "RandomNumberGeneratorResponse";
            EventData.Number = rnd.Next();
        }
        public static void OnRandomNumberGeneratorResponse(Event evt)
        {
            Console.WriteLine($"Received RandomNumberGeneratorResponse with Number: {evt.EventData.Number}");
        }
        public static void OnRandomNumberGeneratorBeforeResponse(Event evt)
        {
            Console.WriteLine("Before RandomNumberGeneratorResponse");
        }
        public static void OnRandomNumberGeneratorAfterResponse(Event evt)
        {
            Console.WriteLine("After RandomNumberGeneratorResponse");
        }
    }

}
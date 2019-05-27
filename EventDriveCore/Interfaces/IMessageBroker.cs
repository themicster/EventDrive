using System;
using System.Security;

namespace EventDriveCore
{
    [SecuritySafeCritical]
    public interface IMessageBroker : IDisposable
    {
        void Publish(string eventName, object source, Event message);
        void Subscribe(string eventName, Action<Event> callback);
        // void Publish<T>(object source, T message) where T : Event;
        // void Subscribe<T>(Action<MessagePayload<T>> subscription) where T : Event;
        // void Unsubscribe<T>(Action<MessagePayload<T>> subscription) where T : Event;
        void Unsubscribe(string eventName, Action<Event> callback);
        void UseEventStore(string connectionString);
        void StartInboundQueue(string groupName);
        void StartOutboundQueue(string groupName);
    }
}

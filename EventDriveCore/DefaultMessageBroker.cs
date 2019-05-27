using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventDriveCore
{
    [SecuritySafeCritical]
    public class DefaultMessageBroker : IMessageBroker
    {
        private static DefaultMessageBroker _instance;
        private readonly List<ConcurrentDictionary<string, List<Delegate>>> _allSubs;
        private readonly ConcurrentDictionary<string, List<Delegate>> _subscribers;
        private readonly ConcurrentDictionary<string, List<Delegate>> _subscribersBefore;
        private readonly ConcurrentDictionary<string, List<Delegate>> _subscribersAfter;
        private ConcurrentQueue<Event> _outboundEvents = new ConcurrentQueue<Event>();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private string _eventStoreConnectionString = null;
        public static DefaultMessageBroker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DefaultMessageBroker();
                return _instance;
            }
        }

        private DefaultMessageBroker()
        {
            _subscribers = new ConcurrentDictionary<string, List<Delegate>>();
            _subscribersBefore = new ConcurrentDictionary<string, List<Delegate>>();
            _subscribersAfter = new ConcurrentDictionary<string, List<Delegate>>();
            _allSubs = new List<ConcurrentDictionary<string,List<Delegate>>>();
            _allSubs.Add(_subscribersBefore);
            _allSubs.Add(_subscribers);
            _allSubs.Add(_subscribersAfter);
        }

        public void Publish(string eventName, object source, Event message) 
        {
            if (eventName == null || message == null || source == null)
                return;
            // This isn't as bad as it looks, this is just going through _subscribersBefore, _subscribers, then _subscribersAfter
            foreach(ConcurrentDictionary<string, List<Delegate>> subs in _allSubs)
            {
                if(subs.ContainsKey(eventName))
                {
                    var delegates = subs[eventName];
                    if (delegates == null || delegates.Count == 0) return;
                    //var payload = new MessagePayload<Event>(message, source);
                    foreach(Action<Event> handler in delegates)
                    {
                        if(message.SameThread)
                        {
                            handler?.Invoke(message);
                        }
                        else 
                        {
                            Task t = Task.Factory.StartNew(() => handler?.Invoke(message));
                            int index = Task.WaitAny(new Task[] { t }, 2000);
                            if(index == 0) // Completed
                            {

                            }
                            else if(index == -1) // Timeout
                            {
                                Console.WriteLine($"Timeout waiting for event handler response: {eventName}");
                            }
                        }
                    }
                }
            }
            if(message.SendToHub)
            {
                _outboundEvents.Enqueue(message);
                _semaphore.Release();
            }
        }
        public void OutboundQueue(string groupName)
        {
            while(true)
            {
                _semaphore.Wait(10000);
                if(_outboundEvents.TryDequeue(out Event evt))
                {
                    string dataStr = JsonConvert.SerializeObject(evt.EventData);
                    EventData ed = new EventData(Guid.NewGuid(), evt.EventName, true, Encoding.ASCII.GetBytes(dataStr), Encoding.ASCII.GetBytes(""));
                    var conn = EventStoreConnection.Create(new Uri(_eventStoreConnectionString), "EventDriveMessagePump");
                    conn.ConnectAsync().Wait();

                    conn.AppendToStreamAsync(groupName, ExpectedVersion.Any, ed).Wait();
                    conn.Close();
                }
            }
        }
        public void InboundQueue(string groupName)
        {
            // Start waiting on all our Message Buses
            IEventHub hub = new EventStoreEventHub(_eventStoreConnectionString, groupName);
            hub.Connect();
            //EventData ed = new EventData(Guid.NewGuid(), "testType", true, Encoding.ASCII.GetBytes(dataStr), Encoding.ASCII.GetBytes(""));
            //conn.AppendToStreamAsync(streamName, ExpectedVersion.Any,ed).Wait();
            while(true)
            {
                List<Event> events = hub.GetNextEvents();
                foreach (Event evt in events)
                {
                    //Console.WriteLine($"Num: {evt.EventNumber} EventId: {evt.EventId} EventType: {evt.Event.EventType}");
                    Publish(evt.EventName, this, evt);
                }
            }

        }

        // public void Subscribe<T>() where T : Event
        // {
        //     string TypeName = typeof(T).ToString();
        //     string[] Names = TypeName.Split('.');
        //     String MethodName = "On" + Names[Names.Count()-1];
        //     //MethodInfo[] methods = typeof(T).GetMethods();
        //     MethodInfo method = typeof(T).GetMethod(MethodName, new Type[] {typeof(MessagePayload<T>)});
        //     // if(method == null)
        //     //     method = methods[1];
        //     Action<MessagePayload<T>> myFunc = (Action<MessagePayload<T>>)method.CreateDelegate(typeof(Action<MessagePayload<T>>));
        //     Subscribe<T>(new Action<MessagePayload<T>>(myFunc));
        // }

        public void Subscribe(string eventName, Action<Event> callback)
        {
            var delegates = _subscribers.ContainsKey(eventName) ? 
                            _subscribers[eventName] : new List<Delegate>();
            lock(_instance)
            {
                if (!delegates.Contains(callback))
                {
                    delegates.Add(callback);
                }
                _subscribers[eventName] = delegates;
            }
        }

        public void SubscribeBefore(string eventName, Action<Event> callback)
        {
            var delegates = _subscribersBefore.ContainsKey(eventName) ?
                            _subscribersBefore[eventName] : new List<Delegate>();
            lock (_instance)
            {
                if (!delegates.Contains(callback))
                {
                    delegates.Add(callback);
                }
                _subscribersBefore[eventName] = delegates;
            }
        }

        // public void SubscribeAfter<T>(Action<MessagePayload<T>> subscription) where T : Event
        // {
        //     var delegates = _subscribersAfter.ContainsKey(typeof(T)) ? 
        //                     _subscribersAfter[typeof(T)] : new List<Delegate>();
        //     if(!delegates.Contains(subscription))
        //     {
        //         delegates.Add(subscription);
        //     }
        //     _subscribersAfter[typeof(T)] = delegates;
        // }

        // public void Unsubscribe<T>(Action<MessagePayload<T>> subscription) where T : Event
        // {
        //     if (!_subscribers.ContainsKey(typeof(T))) return;
        //     var delegates = _subscribers[typeof(T)];
        //     if (delegates.Contains(subscription))
        //         delegates.Remove(subscription);
        //     if (delegates.Count == 0)
        //         _subscribers.Remove(typeof(T));
        // }

        public void Unsubscribe(string eventName, Action<Event> callback)
        {
            if (!_subscribers.ContainsKey(eventName)) return;
            lock(_instance)
            {
                var delegates = _subscribers[eventName];
                if (delegates.Contains(callback))
                    delegates.Remove(callback);
                if (delegates.Count == 0)
                    _subscribers.TryRemove(eventName, out _);
            }
        }

        public void UnsubscribeBefore(string eventName, Action<Event> callback)
        {
            if (!_subscribersBefore.ContainsKey(eventName)) return;
            lock (_instance)
            {
                var delegates = _subscribersBefore[eventName];
                if (delegates.Contains(callback))
                    delegates.Remove(callback);
                if (delegates.Count == 0)
                    _subscribersBefore.TryRemove(eventName, out _);
            }
        }

        // public void UnsubscribeBefore<T>(Action<MessagePayload<T>> subscription) where T : Event
        // {
        //     if (!_subscribersBefore.ContainsKey(typeof(T))) return;
        //     var delegates = _subscribersBefore[typeof(T)];
        //     if (delegates.Contains(subscription))
        //         delegates.Remove(subscription);
        //     if (delegates.Count == 0)
        //         _subscribersBefore.Remove(typeof(T));
        // }
        // public void UnsubscribeAfter<T>(Action<MessagePayload<T>> subscription) where T : Event
        // {
        //     if (!_subscribersAfter.ContainsKey(typeof(T))) return;
        //     var delegates = _subscribersAfter[typeof(T)];
        //     if (delegates.Contains(subscription))
        //         delegates.Remove(subscription);
        //     if (delegates.Count == 0)
        //         _subscribersAfter.Remove(typeof(T));
        // }

        public void StartInboundQueue(string groupName = "EventDriveStream")
        {
            var t1 = Task.Factory.StartNew(() => InboundQueue(groupName));
        }
        public void StartOutboundQueue(string groupName = "EventDriveStream")
        {
            var t1 = Task.Factory.StartNew(() => OutboundQueue(groupName));
        }

        public void UseEventStore(string connectionString)
        {
            _eventStoreConnectionString = connectionString;
        }
        public void Dispose()
        {
            lock(_instance)
            {
                _subscribers?.Clear();
            }
        }   
    }
}

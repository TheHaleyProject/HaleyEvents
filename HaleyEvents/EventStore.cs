using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Haley.Abstractions;
using System.Threading;

namespace Haley.Events
{
    public sealed class EventStore : IEventService
    {
        // Core idea is that a list of delegates are stored. During run time, the delegates are invoked.
        private ConcurrentDictionary<Type, EventBase> _event_collection = new ConcurrentDictionary<Type, EventBase>();

        public T GetEvent<T>() where T : EventBase, new()
        {
            Type _target_type = typeof(T);
            if (!_event_collection.ContainsKey(_target_type))
            {
                //Whichever thread tries to subscribe to the event with "UIThread as option" will also set their context as the synchronization context.
                _event_collection.TryAdd(_target_type, new T());
            }
            T result = (T)_event_collection[_target_type] ?? null;
            return result;
        }

        public void ClearAll()
        {
            _event_collection = new ConcurrentDictionary<Type, EventBase>(); //Clear all previously subscribed events.
        }

        /// <summary>
        /// Clear all the events with the declaring parent matching the arguments.
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        public void ClearAll<TParent>(bool include_all_groups = false)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.UnSubscribe<TParent>(include_all_groups); //This will try and remove the parents if already registered.
            }
        }

        public void ClearAll(Type parent, bool include_all_groups = false)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.UnSubscribe(parent, include_all_groups); //This will try and remove the parents if already registered.
            }
        }

        public void ClearAll(string subscription_key)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.UnSubscribe(subscription_key); //This will try and remove the parents if already registered.
            }
        }

        public void ClearGroup(string group_id)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.UnSubscribeGroup(group_id);
            }
        }

        public static EventStore Singleton = new EventStore();
        public EventStore() { }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Haley.Abstractions;

namespace Haley.Events
{
    public sealed class EventStore : IEventService
    {
        private ConcurrentDictionary<Type, EventBase> _event_collection = new ConcurrentDictionary<Type, EventBase>();

        // Core idea is that a list of delegates are stored. During run time, the delegates are invoked.

        public T GetEvent<T>() where T : EventBase, new()
        {
            Type _target_type = typeof(T);
            if (!_event_collection.ContainsKey(_target_type))
            {
                //If key is not present , add it
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
                _event.unSubscribe<TParent>(include_all_groups); //This will try and remove the parents if already registered.
            }
        }

        public void ClearAll(Type parent, bool include_all_groups = false)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.unSubscribe(parent, include_all_groups); //This will try and remove the parents if already registered.
            }
        }

        public void ClearAll(string subscription_key)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.unSubscribe(subscription_key); //This will try and remove the parents if already registered.
            }
        }

        public void ClearGroup(string group_id)
        {
            foreach (var _event in _event_collection.Values)
            {
                _event.unSubscribeGroup(group_id);
            }
        }

        public static EventStore Singleton = new EventStore();
        public EventStore() { }
    }
}

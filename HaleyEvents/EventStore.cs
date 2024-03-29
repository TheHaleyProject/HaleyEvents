﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Haley.Abstractions;
using System.Threading;
using Haley.Services;

namespace Haley.Events
{
    public sealed class EventStore
    {
        #region Static Items
        private static IEventService _instance = new EventService(); //static item.
        public static T GetEvent<T>() where T : class, IEventBase, new() {
            return _instance.GetEvent<T>();
        }

        public static void ClearAll() {
            _instance.ClearAll();
        }

        /// <summary>
        /// Clear all the events with the declaring parent matching the arguments.
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        public static void ClearAll<TParent>(bool include_all_groups = false) where TParent : class {
            _instance.ClearAll<TParent>(include_all_groups);
        }

        public static void ClearAll(Type parent, bool include_all_groups = false) {
            _instance.ClearAll(parent, include_all_groups);
        }

        public static void ClearAll(string subscription_key) {
            _instance.ClearAll(subscription_key);
        }

        public static void ClearGroup(string group_id) {
            _instance.ClearGroup(group_id);
        }

        #endregion

        [Obsolete(@"Remove the SINGLETON keyword. Replace ""EventStore.Singleton.[METHOD/PROPERTY]"" with ""EventStore.[METHOD/PROPERTY]""", true)]
        public static EventStore Singleton => null;
        private EventStore() { }
    }
}

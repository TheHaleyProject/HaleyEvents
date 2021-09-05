using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Haley.Events;
namespace Haley.Abstractions
{
    public interface IEventService
    {
        T GetEvent<T>() where T : EventBase, new();
        void ClearAll();
        void ClearAll<TParent>(bool include_all_groups = false);
        void ClearAll(Type parent, bool include_all_groups = false);
        void ClearAll(string subscription_key);
        void ClearGroup(string group_id);
    }
}

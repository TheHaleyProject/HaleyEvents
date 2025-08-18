using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Haley.Enums;
using System.Threading;

namespace Haley.Events
{

    public class HEvent : EventBase
    {
        public void Publish()
        {
            //Publish without passing arguments
            base.publish();
        }
        public string Subscribe(Action listener, bool allow_duplicate = false, string group_id = null, InvokeOption option = InvokeOption.DefaultThread)
        {
            SubscriberBase _newinfo = new SubscriberBase(listener, group_id, option:option);
            if (option == InvokeOption.UIThread)
            {
                _newinfo.SyncContext = SynchronizationContext.Current; //this gets the context of the subscribing action.
            }
            return base.subscribe(_newinfo, allow_duplicate); //Returning the subscription id
        }
    }

    public class HEvent<T> : EventBase
    {
        public void Publish(T eventArguments)
        {
            base.publish(eventArguments);
        }
        public string Subscribe(Action<T> listener, bool allow_duplicate = false, string group_id = null, InvokeOption option = InvokeOption.DefaultThread)
        {
            SubscriberBase<T> _newinfo = new SubscriberBase<T>(listener, group_id, option: option) { };
            if (option == InvokeOption.UIThread)
            {
                _newinfo.SyncContext = SynchronizationContext.Current; //this gets the context of the subscribing action.
            }
            return base.subscribe(_newinfo, allow_duplicate); //Returning the subscription id
        }
    }
}

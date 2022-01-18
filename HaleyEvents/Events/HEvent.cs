using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Haley.Events
{

    public class HEvent : EventBase
    {
        public void Publish()
        {
            //Publish without passing arguments
            base._publish();
        }
        public string Subscribe(Action listener, bool allow_duplicate = false, string group_id = null)
        {
            SubscriberBase _newinfo = new SubscriberBase(listener, group_id);
            return base._subscribe(_newinfo, allow_duplicate); //Returning the subscription id
        }
    }

    public class HEvent<T> : EventBase
    {
        public void Publish(T eventArguments)
        {
            base._publish(eventArguments);
        }
        public string Subscribe(Action<T> listener, bool allow_duplicate = false, string group_id = null)
        {
            SubscriberBase<T> _newinfo = new SubscriberBase<T>(listener, group_id);
            return base._subscribe(_newinfo, allow_duplicate); //Returning the subscription id
        }
    }
}

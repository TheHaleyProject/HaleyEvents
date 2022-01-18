using Haley.Abstractions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Events.Utils;

namespace Haley.Events
{
    public class SubscriberBase : SubscriberBase<object>
    {
        public SubscriberBase(Action _listener, string group_id) : base((nullentry) => _listener(), group_id, _listener.Method.DeclaringType, _listener.Method.Name)
        {
            //Because in above when when we are passing the arguments (listener) to the generic subscriber base, we are creating another delegate from this class. So, this breaks the values. To avoid that, rewrite the values by passing ACTUAL LISTENER'S value.
        }
    }

    public class SubscriberBase<T> : ISubscriber
    {
        public string id { get; set; }
        public string group_id { get; private set; }
        public Type declaring_type { get; }
        public string listener_method { get; set; }
        public Action<T> listener { get; set; }

        public SubscriberBase(Action<T> _listener, string _group_id, Type declaringtype = null, string listener_name = null)
        {
            listener = _listener;
            declaring_type = declaringtype ?? listener.Method.DeclaringType;
            listener_method = listener_name ?? listener.Method.Name;
            id = Guid.NewGuid().ToString();
            //By default lets keep all group id as null.
            group_id = _group_id; //This is used for grouping together a set of events and it will be easy to unsubscribe if required.
        }

        public void SendMessage(params object[] args)
        {
            if (args.Length == 0)
            {
                listener.Invoke(default(T));
            }
            else
            {
                var _incomingtype = args[0].GetType();
                var _this_generic = this.GetType().GetGenericArguments()[0];
                if (_this_generic.IsAssignableFrom(_incomingtype))
                {
                    listener.Invoke((T)args[0]);
                }
            }
        }
    }
}

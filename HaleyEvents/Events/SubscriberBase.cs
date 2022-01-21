using Haley.Abstractions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Enums;
using Haley.Events.Utils;
using System.Threading;

namespace Haley.Events
{
    public class SubscriberBase : SubscriberBase<object>
    {
        public SubscriberBase(Action _listener, string group_id, InvokeOption option = InvokeOption.DefaultThread) : base((nullentry) => _listener(), group_id, _listener.Method.DeclaringType, _listener.Method.Name,option,false)
        {
            //Because in above when when we are passing the arguments (listener) to the generic subscriber base, we are creating another delegate from this class. So, this breaks the values. To avoid that, rewrite the values by passing ACTUAL LISTENER'S value.
        }
    }

    public class SubscriberBase<T> : ISubscriber
    {
        public string Id { get; set; }
        public string GroupId { get; private set; }
        public Type DeclaringType { get; }
        public string ListenerMethod { get; set; }
        public Action<T> listener { get; set; }
        public SynchronizationContext SyncContext { get; set; }
        public InvokeOption InvokeOption { get; set; }
        public bool CanSendArgs { get; set; }

        public SubscriberBase(Action<T> _listener, string _group_id, Type declaringtype = null, string listener_name = null,InvokeOption option = InvokeOption.DefaultThread,bool canSendArgs = true,SynchronizationContext context = null)
        {
            listener = _listener;
            DeclaringType = declaringtype ?? listener.Method.DeclaringType;
            ListenerMethod = listener_name ?? listener.Method.Name;
            Id = Guid.NewGuid().ToString();
            CanSendArgs = canSendArgs;
            SyncContext = context;
            InvokeOption = option; //To be used when sending message.
            //By default lets keep all group id as null.
            GroupId = _group_id; //This is used for grouping together a set of events and it will be easy to unsubscribe if required.
        }

        public void SendMessage(params object[] args)
        {
            try
            {
                T _param = default(T);

                if (CanSendArgs && args != null && args.Length > 0 && args[0] != null)
                {
                    var _incomingtype = args[0].GetType();
                    var _this_generic = this.GetType().GetGenericArguments()[0];
                    if (_this_generic.IsAssignableFrom(_incomingtype))
                    {
                        _param = (T)args[0]; //if not assignable then we will just an empty or default object.
                    }
                }
                SendMessageInternal(_param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SendMessageInternal(T param)
        {
            try
            {
                switch (InvokeOption)
                {
                    case InvokeOption.BackgroundThread:
                        //Create a task and run this in a different thread.
                        Task.Run(()=>listener.Invoke(param)); 
                        break;
                    case InvokeOption.UIThread:
                        //Synchronization Context invoke needed to avoid cross thread calling.
                        if (SyncContext == null)
                            throw new ArgumentException($@"Synchronization context for {ListenerMethod} from {DeclaringType} is null. Cannot raise the event. Try calling the event in default thread and handle the UI synchronization.");

                        SyncContext.Post(p => listener.Invoke((T)p), param); //Same param gets sent in as "p" which is then sent to the listener.
                        break;
                    default:
                    case InvokeOption.DefaultThread:
                        listener.Invoke(param); //Run in the same calling thread.
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

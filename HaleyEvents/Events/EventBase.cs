using Haley.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Haley.Events
{
    /// <summary>
    /// Implementing a simple observer pattern
    /// </summary>
    public abstract class EventBase
    {
        //private ConcurrentBag<ISubscriber> _subscribers = new ConcurrentBag<ISubscriber>();
        public SynchronizationContext SynchronizationContext { get; set; }
        private ConcurrentDictionary<string, ISubscriber> _subscribers = new ConcurrentDictionary<string, ISubscriber>();
        #region PROTECTED METHODS
        protected void publish(params object[] arguments)
        {
            // Using params keyword, because we can have zero or more parameters
            //This should invoke all the delegates
            foreach (var _subscriber in _subscribers)
            {
                _subscriber.Value.SendMessage(arguments);
            }
        }
        protected string subscribe(ISubscriber subscriber, bool allow_duplicates = false)
        {
            //If group id is null, then it means we take on the default value.
            if (!allow_duplicates)
            {
                var _kvp = _subscribers.FirstOrDefault(sub =>
            sub.Value.ListenerMethod == subscriber.ListenerMethod &&
            sub.Value.DeclaringType == subscriber.DeclaringType &&
            sub.Value.GroupId == subscriber.GroupId);
                if (_kvp.Value != null)
                {
                    return _kvp.Value.Id;
                }
            }

            _subscribers.TryAdd(subscriber.Id, subscriber); //This subscriber can be a duplicate of same group or a new subscriber with a new group id.
            return subscriber.Id;
        }

        protected bool unSubscribe(string subscriber_id)
        {
            ISubscriber _removed_value;
            var _removed = _subscribers.TryRemove(subscriber_id, out _removed_value);
            return _removed;
        }
        protected void unSubscribeAll()
        {
            _subscribers = new ConcurrentDictionary<string, ISubscriber>();
        }


        #endregion

        #region VIRTUAL METHODS
        /// <summary>
        /// Subscribers with the input key will be unsubscribed.
        /// </summary>
        /// <param name="subscription_key"></param>
        /// <returns></returns>
        public virtual bool UnSubscribe(string subscription_key) //Only one item will be unsubscribed.
        {
            return unSubscribe(subscription_key);
        }

        public virtual bool UnSubscribeGroup(string group_id)
        {
            try
            {
                //Even though we know that default subscriptions doesn't have a group ID, we need to ensure that those doesn't get deleted by mistake.
                if (string.IsNullOrEmpty(group_id)) return false; //In case of null, we should not even check.
                List<string> _toremove = _subscribers.Where(_kvp => !(string.IsNullOrEmpty(_kvp.Value.GroupId)) && _kvp.Value.GroupId == group_id)?.Select(p => p.Key)?.ToList();
                if (_toremove == null || _toremove.Count == 0) return true;
                foreach (var item in _toremove)
                {
                    unSubscribe(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        /// <summary>
        /// All subscribers(delegates) with the declaring parent type will be unsubscribed.
        /// </summary>
        /// <param name="subscription_key"></param>
        /// <returns></returns>
        public virtual bool UnSubscribe(Type declaring_parent, bool include_all_groups = false) //Only one item will be unsubscribed.
        {
            if (declaring_parent == null) return false;
            try
            {
                List<string> _toremove = new List<string>();
                var _allMatches = _subscribers.Where(_kvp => _kvp.Value.DeclaringType == declaring_parent);
                if (include_all_groups)
                {
                    _toremove = _allMatches?.Select(p => p.Key)?.ToList(); //Get all, including the groups.
                }
                else
                {
                    var _groupsIgnoredMatches = _allMatches?.Where(_kvp => string.IsNullOrEmpty(_kvp.Value.GroupId))?.ToList();
                    _toremove = _groupsIgnoredMatches?.Select(p => p.Key)?.ToList();
                }

                if (_toremove == null || _toremove.Count == 0) return false;

                foreach (var item in _toremove)
                {
                    unSubscribe(item);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// All subscribers(delegates) with the declaring parent type will be unsubscribed.
        /// </summary>
        /// <typeparam name="TParent">Declaring Type to be removed.</typeparam>
        /// <returns></returns>
        public virtual bool UnSubscribe<TParent>(bool include_all_groups = false)
        {
            return UnSubscribe(typeof(TParent), include_all_groups);
        }

        #endregion
    }
}

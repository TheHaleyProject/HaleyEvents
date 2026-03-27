using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Haley.Abstractions;
using System.Threading;
using Haley.Services;
using Microsoft.Extensions.Logging;

namespace Haley.Events
{
    public sealed class EventStore
    {
        #region Static Items
        private static IEventService _instance = new EventService(); //static item.
        public static ILogger Logger { get; private set; }
        public static bool ThrowExceptions { get; set; } = true;

        // Receipt registry: receiptKey → completer action
        private static readonly ConcurrentDictionary<string, Action<object>> _receipts
            = new ConcurrentDictionary<string, Action<object>>();

        /// <summary>
        /// Registers a one-shot receipt token keyed by <paramref name="key"/>.
        /// Call before publishing the event. Await <see cref="IReceiptToken{T}.WaitAsync"/> to receive the result.
        /// The token auto-cancels after <paramref name="timeout"/> if the handler never fires.
        /// </summary>
        public static IReceiptToken<TResult> Expect<TResult>(string key, TimeSpan timeout) {
            Action<string> cleanup = k => {
                Action<object> removed;
                _receipts.TryRemove(k, out removed);
            };

            // Placeholder registered first so the key exists before the token's
            // timeout CancellationTokenSource can fire cleanup.
            _receipts[key] = _ => { };

            var token = new ReceiptToken<TResult>(key, timeout, cleanup);

            // Replace placeholder with the real completer that closes over the token.
            _receipts[key] = value => {
                if (value is TResult typed) {
                    token.Complete(typed);
                } else {
                    token.Complete(default(TResult));
                }
            };

            return token;
        }

        /// <summary>
        /// Called by an event handler to complete the receipt registered under <paramref name="key"/>.
        /// Returns true if a waiting receipt was found and completed; false if no receipt exists (fire-and-forget caller).
        /// </summary>
        public static bool TryComplete(string key, object value) {
            if (string.IsNullOrEmpty(key)) return false;
            Action<object> completer;
            if (_receipts.TryGetValue(key, out completer)) {
                completer(value);
                return true;
            }
            return false;
        }

        public static T Get<T>() where T : class, IEventBase, new() {
            return _instance.GetEvent<T>();
        }

        public static void ClearAll() {
            _instance.ClearAll();
        }

        public static void SetLogger(ILogger logger) {
            Logger = logger;
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
        private EventStore() { }
    }
}

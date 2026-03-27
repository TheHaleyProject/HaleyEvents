using Haley.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Haley.Events {
    internal sealed class ReceiptToken<T> : IReceiptToken<T> {
        private readonly TaskCompletionSource<T> _tcs;
        private readonly CancellationTokenSource _cts;
        private readonly string _key;
        private readonly Action<string> _cleanup;
        private int _disposed = 0;

        public bool IsCompleted => _tcs.Task.IsCompleted;

        public ReceiptToken(string key, TimeSpan timeout, Action<string> cleanup) {
            _key = key;
            _cleanup = cleanup;
            _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _cts = new CancellationTokenSource(timeout);
            _cts.Token.Register(OnTimeout);
        }

        private void OnTimeout() {
            _tcs.TrySetCanceled();
            Cleanup();
        }

        internal void Complete(T value) {
            _tcs.TrySetResult(value);
            Cleanup();
        }

        private void Cleanup() {
            _cleanup?.Invoke(_key);
        }

        public Task<T> WaitAsync(CancellationToken ct = default) {
            if (ct == default) return _tcs.Task;
            ct.Register(() => _tcs.TrySetCanceled());
            return _tcs.Task;
        }

        public void Dispose() {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _tcs.TrySetCanceled();
            Cleanup();
            _cts.Dispose();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    internal class AsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource<object>> _waiters = new Queue<TaskCompletionSource<object>>();
        private bool _signaled;

        public Task WaitOne()
        {
            lock (_waiters)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                if (_waiters.Count > 0 || !_signaled)
                {
                    _waiters.Enqueue(tcs);
                }
                else
                {
                    tcs.SetResult(null);
                    _signaled = false;
                }
                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<object> toSet = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0) toSet = _waiters.Dequeue();
                else _signaled = true;
            }
            toSet?.SetResult(null);
        }
    }
}
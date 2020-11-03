using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EuNet.Core
{
    public static class TaskExtensions
    {
        public static void DoNotAwait(this Task task)
        {

        }

        public static IEnumerator AsIEnumerator(this Task task, TimeSpan? timeOut = null)
        {
            if (timeOut != null)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                while (!task.IsCompleted)
                {
                    yield return null;

                    if (stopwatch.Elapsed >= timeOut)
                    {
                        throw new TimeoutException($"task timeout = {timeOut}");

                    }
                }
            }
            else
            {
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    throw task.Exception;
                }
            }
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}

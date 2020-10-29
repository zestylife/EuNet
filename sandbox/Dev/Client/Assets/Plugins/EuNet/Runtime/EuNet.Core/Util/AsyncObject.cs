using System;
using System.Runtime.CompilerServices;

namespace EuNet.Core
{
    public class AsyncObject<T> : INotifyCompletion
    {
        private Action _continuation = null;
        private T _result = default(T);
        private Exception _exception = null;

        public bool IsCompleted
        {
            get;
            private set;
        }

        public T GetResult()
        {
            if (_exception != null)
                throw _exception;

            return _result;
        }

        public bool TrySetResult(T result)
        {
            if (IsCompleted == false)
            {
                IsCompleted = true;
                _result = result;

                _continuation?.Invoke();

                return true;
            }
            return false;
        }

        public bool TrySetException(Exception exception)
        {
            if (IsCompleted == false)
            {
                IsCompleted = true;
                _exception = exception;

                _continuation?.Invoke();

                return true;
            }
            return false;
        }

        public void OnCompleted(Action continuation)
        {
            if (_continuation != null)
                throw new InvalidOperationException("This AsyncObject instance has already been listened");
            _continuation = continuation;
        }

        public void Reset()
        {
            _result = default(T);
            _exception = null;
            IsCompleted = false;
            _continuation = null;
        }

        public AsyncObject<T> WaitAsync()
        {
            return this;
        }

        public AsyncObject<T> GetAwaiter()
        {
            return this;
        }
    }
}

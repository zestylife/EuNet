using System;
using System.Collections.Generic;
using System.Threading;

namespace EuNet.Core
{
    public class ConcurrentCircularQueue<T> where T : class
    {
        private volatile int _lock;

        private volatile int _head;
        private volatile int _tail;
        private readonly T[] _buffer;
        private int _bufferLength;

        public int Capacity
        {
            get
            {
                return _bufferLength - 1;
            }
        }

        public int Head => _head;
        public int Tail => _tail;

        public bool IsEmpty
        {
            get
            {
                return _head == _tail;
            }
        }

        public bool IsFull
        {
            get
            {
                return _head == (_tail + 1) % _bufferLength;
            }
        }

        public int Count
        {
            get
            {
                if (IsEmpty)
                    return 0;

                if (_tail < _head)
                    return _bufferLength - _head + _tail;

                return _tail - _head;
            }
        }

        private ConcurrentCircularQueue()
        {
        }

        public ConcurrentCircularQueue(int capacity)
        {
            _buffer = new T[capacity + 1];
            _head = _tail = 0;
            _lock = 0;
            _bufferLength = capacity + 1;
        }

        public bool Enqueue(T item)
        {
            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0) ;

            try
            {
                if (IsFull)
                    return false;

                _tail = (++_tail) % _bufferLength;
                _buffer[_tail] = item;

                return true;
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                    throw new InvalidOperationException();
            }
        }

        public T Dequeue()
        {
            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0) ;

            try
            {
                if (IsEmpty)
                    return default(T);

                var index = (++_head) % _bufferLength;

                T item = _buffer[index];
                _buffer[index] = null;
                _head = index;

                return item;
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                    throw new InvalidOperationException();
            }
        }

        public bool TryDequeue(out T value)
        {
            value = Dequeue();
            return value != null;
        }

        public void Clear()
        {
            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            _head = _tail = 0;

            if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                throw new InvalidOperationException();
        }

        public T[] ToArray()
        {
            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            var length = Count;

            var array = new T[length];
            if (IsEmpty)
                return array;

            if (_head < _tail)
            {
                Array.Copy(_buffer, _head + 1, array, 0, length);
            }
            else
            {
                var firstLegnth = _bufferLength - _head - 1;

                Array.Copy(_buffer, _head + 1, array, 0, firstLegnth);
                Array.Copy(_buffer, 0, array, firstLegnth, length - firstLegnth);
            }

            if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                throw new InvalidOperationException();

            return array;
        }

        public void Enqueue(ICollection<T> items)
        {
            if (items.Count == 0)
                return;

            if (items.Count > Capacity)
                throw new InvalidOperationException("Trying to add too many items");

            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            try
            {
                if (items.Count > Capacity - Count)
                    throw new InvalidOperationException("Not enough space left");

                foreach(var item in items)
                {
                    _tail = (++_tail) % _bufferLength;
                    _buffer[_tail] = item;
                }
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                    throw new InvalidOperationException();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();

            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            try
            {
                var length = Count;

                if (array.Length - arrayIndex < length)
                    throw new ArgumentException();

                if (IsEmpty)
                    return;

                if(_head < _tail)
                {
                    Array.Copy(_buffer, _head + 1, array, arrayIndex, length);
                }
                else
                {
                    var firstLegnth = _bufferLength - _head - 1;

                    Array.Copy(_buffer, _head + 1, array, arrayIndex, firstLegnth);
                    Array.Copy(_buffer, 0, array, arrayIndex + firstLegnth, length - firstLegnth);
                }
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                    throw new InvalidOperationException();
            }
        }
    }
}
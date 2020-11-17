using System;
using System.Collections.Generic;
using System.Threading;

namespace EuNet.Core
{
    public class ConcurrentCircularQueue<T> where T : class
    {
        private int _lock;

        private volatile int _head;
        private volatile int _tail;
        private readonly T[] _buffer;

        public int Capacity
        {
            get
            {
                return _buffer.Length - 1;
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
                return _head == (_tail + 1) % _buffer.Length;
            }
        }

        public int Length
        {
            get
            {
                if (IsEmpty)
                    return 0;

                if (_tail < _head)
                    return _buffer.Length - _head + _tail;

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
        }
        
        public bool Enqueue(T item)
        {
            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            try
            {
                if (IsFull)
                    return false;

                _tail = (++_tail) % _buffer.Length;
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
            if (IsEmpty)
                return default(T);

            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            try
            {
                if (IsEmpty)
                    return default(T);

                var index = (++_head) % _buffer.Length;

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

        public void Clear()
        {
            var spin = new SpinWait();

            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                spin.SpinOnce();

            _head = _tail = 0;

            if (Interlocked.CompareExchange(ref _lock, 0, 1) != 1)
                throw new InvalidOperationException();
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
                if (items.Count > Capacity - Length)
                    throw new InvalidOperationException("Not enough space left");

                foreach(var item in items)
                {
                    _tail = (++_tail) % _buffer.Length;
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
                if (array.Length - arrayIndex < Length)
                    throw new ArgumentException();

                if (IsEmpty)
                    return;

                if(_head < _tail)
                {
                    Array.Copy(_buffer, _head + 1, array, arrayIndex, Length);
                }
                else
                {
                    var firstLegnth = _buffer.Length - _head - 1;

                    Array.Copy(_buffer, _head + 1, array, arrayIndex, firstLegnth);
                    Array.Copy(_buffer, 0, array, arrayIndex + firstLegnth, Length - firstLegnth);
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
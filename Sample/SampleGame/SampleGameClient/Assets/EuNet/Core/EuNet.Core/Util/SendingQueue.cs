using System;
using System.Collections;
using System.Collections.Generic;

namespace EuNet.Core
{
    public sealed class SendingQueue : IList<ArraySegment<byte>>
    {
        private List<ArraySegment<byte>> _globalQueue;
        private int _currentCount = 0;
        private int _beginOffset;

        public SendingQueue()
        {
            _globalQueue = new List<ArraySegment<byte>>();
        }

        public ArraySegment<byte> this[int index]
        {
            get
            {
                return _globalQueue[_beginOffset + index];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return _currentCount - _beginOffset;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public int TotalSegmentCount
        {
            get
            {
                var count = _currentCount - _beginOffset;
                var total = 0;

                for (int i = _beginOffset; i < count; i++)
                {
                    var segment = _globalQueue[i];
                    total += segment.Count;
                }

                return total;
            }
        }

        public bool Push(ArraySegment<byte> item)
        {
            if (_currentCount >= _globalQueue.Count)
            {
                _globalQueue.Add(item);
                ++_currentCount;
                return true;
            }

            _globalQueue[_currentCount] = item;
            ++_currentCount;

            return true;
        }

        public void Clear()
        {
            _currentCount = 0;
            _beginOffset = 0;
        }

        public void Add(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            for (var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            for (var i = 0; i < (_currentCount - _beginOffset); ++i)
            {
                yield return _globalQueue[_beginOffset + i];
            }
        }

        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void LeftTrim(int trimSize)
        {
            var count = _currentCount - _beginOffset;
            var subTotal = 0;

            for (int i = _beginOffset; i < count; i++)
            {
                var segment = _globalQueue[i];
                subTotal += segment.Count;

                if (subTotal <= trimSize)
                    continue;

                _beginOffset = i;

                int rest = subTotal - trimSize;
                _globalQueue[i] = new ArraySegment<byte>(segment.Array, segment.Offset + segment.Count - rest, rest);

                break;
            }
        }
    }
}

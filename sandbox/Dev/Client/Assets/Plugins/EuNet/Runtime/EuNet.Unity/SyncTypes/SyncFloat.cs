using EuNet.Unity;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    [Serializable]
    public struct SyncFloat : IEquatable<SyncFloat>, IEquatable<float>, IComparable<SyncFloat>, IComparable<float>, IComparable
    {
        [SerializeField]
        private float _startValue;

        [SerializeField]
        private float _endValue;

        [SerializeField]
        private float _elapsedTime;

        [SerializeField]
        private float _syncTime;

        public float CurrentValue
        {
            get
            {
                return Mathf.Lerp(_startValue, _endValue, _elapsedTime / SyncTime);
            }
        }

        public float SyncTime
        {
            get
            {
                return _syncTime >= 0f ? _syncTime : NetClientGlobal.Instance.DefaultSyncTime;
            }
        }

        public SyncFloat(float value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(float currentValue, float netValue, float netVelocity)
        {
            _startValue = currentValue;
            _endValue = netValue + netVelocity * SyncTime;
            _elapsedTime = 0f;
        }

        public float Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return CurrentValue;
        }

        public static implicit operator SyncFloat(float value)
        {
            return new SyncFloat(value);
        }

        public static implicit operator float(SyncFloat value)
        {
            return value.CurrentValue;
        }

        public override int GetHashCode()
        {
            return CurrentValue.GetHashCode();
        }

        public override string ToString()
        {
            return CurrentValue.ToString();
        }

        public int CompareTo(SyncFloat other)
        {
            return CurrentValue.CompareTo(other.CurrentValue);
        }

        public int CompareTo(float other)
        {
            return CurrentValue.CompareTo(other);
        }

        public int CompareTo(object obj)
        {
            return CurrentValue.CompareTo(obj);
        }

        public bool Equals(SyncFloat other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(float other)
        {
            return CurrentValue.Equals(other);
        }
    }
}
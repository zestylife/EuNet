using EuNet.Unity;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    [Serializable]
    public struct SyncVector3 : IEquatable<SyncVector3>, IEquatable<Vector3>
    {
        [SerializeField]
        private Vector3 _startValue;

        [SerializeField]
        private Vector3 _endValue;

        [SerializeField]
        private float _elapsedTime;

        [SerializeField]
        private float _syncTime;

        public Vector3 CurrentValue
        {
            get
            {
                return Vector3.Lerp(_startValue, _endValue, _elapsedTime / SyncTime);
            }
        }

        public float SyncTime
        {
            get
            {
                return _syncTime >= 0f ? _syncTime : NetClientGlobal.Instance.DefaultSyncTime;
            }
        }

        public SyncVector3(Vector3 value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Vector3 currentValue, Vector3 netValue, Vector3 netVelocity)
        {
            _startValue = currentValue;
            _endValue = netValue + netVelocity * SyncTime;
            _elapsedTime = 0f;
        }

        public Vector3 Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return CurrentValue;
        }

        public static implicit operator SyncVector3(Vector3 value)
        {
            return new SyncVector3(value);
        }

        public static implicit operator Vector3(SyncVector3 value)
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

        public bool Equals(SyncVector3 other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Vector3 other)
        {
            return CurrentValue.Equals(other);
        }
    }
}
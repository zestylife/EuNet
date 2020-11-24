using EuNet.Unity;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    [Serializable]
    public struct SyncVector2 : IEquatable<SyncVector2>, IEquatable<Vector2>
    {
        [SerializeField]
        private Vector2 _startValue;

        [SerializeField]
        private Vector2 _endValue;

        [SerializeField]
        private float _elapsedTime;

        [SerializeField]
        private float _syncTime;

        public Vector2 CurrentValue
        {
            get
            {
                return Vector2.Lerp(_startValue, _endValue, _elapsedTime / SyncTime);
            }
        }

        public float SyncTime
        {
            get
            {
                return _syncTime >= 0f ? _syncTime : NetClientGlobal.Instance.DefaultSyncTime;
            }
        }

        public SyncVector2(Vector2 value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Vector2 currentValue, Vector2 netValue, Vector2 netVelocity)
        {
            _startValue = currentValue;
            _endValue = netValue + netVelocity * SyncTime;
            _elapsedTime = 0f;
        }

        public Vector2 Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return CurrentValue;
        }

        public static implicit operator SyncVector2(Vector2 value)
        {
            return new SyncVector2(value);
        }

        public static implicit operator Vector2(SyncVector2 value)
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

        public bool Equals(SyncVector2 other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Vector2 other)
        {
            return CurrentValue.Equals(other);
        }
    }
}
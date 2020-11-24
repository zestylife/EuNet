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
        private Vector2 _value;

        [SerializeField]
        private Vector2 _netValue;

        [SerializeField]
        private Vector2 _velocity;

        [SerializeField]
        private float _elapsedTime;

        [SerializeField]
        private float _syncTime;

        public float SyncTime
        {
            get
            {
                return _syncTime > 0f ? _syncTime : _syncTime = NetClientGlobal.Instance.DefaultSyncTime;
            }
            set
            {
                _syncTime = value;
                UpdateEndValue();
                UpdateValue();
            }
        }

        public Vector2 Velocity
        {
            get { return _velocity; }
            set
            {
                _velocity = value;
                UpdateEndValue();
                UpdateValue();
            }
        }

        public float ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                _elapsedTime = value;
                UpdateValue();
            }
        }

        public SyncVector2(Vector2 value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _value = value;
            _netValue = value;
            _velocity = Vector2.zero;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Vector2 currentValue, Vector2 netValue, Vector2 netVelocity)
        {
            _startValue = currentValue;
            _netValue = netValue;
            _velocity = netVelocity;
            _value = currentValue;
            _elapsedTime = 0f;

            UpdateEndValue();
        }

        private void UpdateEndValue()
        {
            _endValue = _netValue + _velocity * SyncTime;
        }

        private Vector2 UpdateValue()
        {
            _value = Vector2.LerpUnclamped(_startValue, _endValue, _elapsedTime / SyncTime);
            return _value;
        }

        public Vector2 Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return UpdateValue();
        }

        public static implicit operator SyncVector2(Vector2 value)
        {
            return new SyncVector2(value);
        }

        public static implicit operator Vector2(SyncVector2 value)
        {
            return value._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public bool Equals(SyncVector2 other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _value.Equals(other._value) &&
                _netValue.Equals(other._netValue) &&
                _velocity.Equals(other._velocity) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Vector2 other)
        {
            return _value.Equals(other);
        }
    }
}
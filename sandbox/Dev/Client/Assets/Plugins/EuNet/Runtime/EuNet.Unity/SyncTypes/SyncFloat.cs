using EuNet.Unity;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    [Serializable]
    public struct SyncFloat : IEquatable<SyncFloat>, IEquatable<float>
    {
        [SerializeField]
        private float _startValue;

        [SerializeField]
        private float _endValue;

        [SerializeField]
        private float _value;

        [SerializeField]
        private float _netValue;

        [SerializeField]
        private float _velocity;

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

        public float Velocity
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

        public SyncFloat(float value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _value = value;
            _netValue = value;
            _velocity = 0f;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(float currentValue, float netValue, float netVelocity)
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

        private float UpdateValue()
        {
            _value = Mathf.LerpUnclamped(_startValue, _endValue, _elapsedTime / SyncTime);
            return _value;
        }

        public float Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return UpdateValue();
        }

        public static implicit operator SyncFloat(float value)
        {
            return new SyncFloat(value);
        }

        public static implicit operator float(SyncFloat value)
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

        public bool Equals(SyncFloat other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _value.Equals(other._value) &&
                _netValue.Equals(other._netValue) &&
                _velocity.Equals(other._velocity) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(float other)
        {
            return _value.Equals(other);
        }
    }
}
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
        private Vector3 _value;

        [SerializeField]
        private Vector3 _netValue;

        [SerializeField]
        private Vector3 _velocity;

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

        public Vector3 Velocity
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

        public SyncVector3(Vector3 value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _value = value;
            _netValue = value;
            _velocity = Vector3.zero;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Vector3 currentValue, Vector3 netValue, Vector3 netVelocity)
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

        private Vector3 UpdateValue()
        {
            _value = Vector3.LerpUnclamped(_startValue, _endValue, _elapsedTime / SyncTime);
            return _value;
        }

        public Vector3 Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return UpdateValue();
        }

        public static implicit operator SyncVector3(Vector3 value)
        {
            return new SyncVector3(value);
        }

        public static implicit operator Vector3(SyncVector3 value)
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

        public bool Equals(SyncVector3 other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _value.Equals(other._value) &&
                _netValue.Equals(other._netValue) &&
                _velocity.Equals(other._velocity) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Vector3 other)
        {
            return _value.Equals(other);
        }
    }
}
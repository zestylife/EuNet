using EuNet.Unity;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    [Serializable]
    public struct SyncQuaternion : IEquatable<SyncQuaternion>, IEquatable<Quaternion>
    {
        [SerializeField]
        private Quaternion _startValue;

        [SerializeField]
        private Quaternion _endValue;

        [SerializeField]
        private Quaternion _value;

        [SerializeField]
        private Quaternion _netValue;

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

        public SyncQuaternion(Quaternion value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _value = value;
            _netValue = value;
            _velocity = Vector3.zero;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Quaternion currentValue, Quaternion netValue, Vector3 netVelocity)
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
            _endValue = _netValue * Quaternion.Euler(_velocity * SyncTime);
        }

        private Quaternion UpdateValue()
        {
            _value = Quaternion.LerpUnclamped(_startValue, _endValue, _elapsedTime / SyncTime);
            return _value;
        }

        public Quaternion Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return UpdateValue();
        }

        public static implicit operator SyncQuaternion(Quaternion value)
        {
            return new SyncQuaternion(value);
        }

        public static implicit operator Quaternion(SyncQuaternion value)
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

        public bool Equals(SyncQuaternion other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _value.Equals(other._value) &&
                _netValue.Equals(other._netValue) &&
                _velocity.Equals(other._velocity) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Quaternion other)
        {
            return _value.Equals(other);
        }
    }
}

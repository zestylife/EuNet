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
        private float _elapsedTime;

        [SerializeField]
        private float _syncTime;

        public Quaternion CurrentValue
        {
            get
            {
                return Quaternion.Lerp(_startValue, _endValue, _elapsedTime / SyncTime);
            }
        }

        public float SyncTime
        {
            get
            {
                return _syncTime >= 0f ? _syncTime : NetClientGlobal.Instance.DefaultSyncTime;
            }
        }

        public SyncQuaternion(Quaternion value, float syncTime = -1f)
        {
            _startValue = value;
            _endValue = value;
            _elapsedTime = 0f;
            _syncTime = syncTime;
        }

        public void Set(Quaternion currentValue, Quaternion netValue, Quaternion netVelocity)
        {
            _startValue = currentValue;
            _endValue = netValue * Quaternion.Lerp(Quaternion.identity, netVelocity, SyncTime);
            _elapsedTime = 0f;
        }

        public Quaternion Update(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
            return CurrentValue;
        }

        public static implicit operator SyncQuaternion(Quaternion value)
        {
            return new SyncQuaternion(value);
        }

        public static implicit operator Quaternion(SyncQuaternion value)
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

        public bool Equals(SyncQuaternion other)
        {
            return _startValue.Equals(other._startValue) &&
                _endValue.Equals(other._endValue) &&
                _elapsedTime.Equals(other._elapsedTime) &&
                _syncTime.Equals(other._syncTime);
        }

        public bool Equals(Quaternion other)
        {
            return CurrentValue.Equals(other);
        }
    }
}
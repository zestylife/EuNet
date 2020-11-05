using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    public class NetViewPosRotSync : INetViewSync
    {
        private NetView _view;
        public NetView View => _view;

        private readonly float _updateInterval;
        private float _updateElapsedTime;
        private int _lastSentHash;
        private Vector3 _targetPos;
        private Quaternion _targetRot;

        public NetViewPosRotSync(NetView view, float updateInterval)
        {
            _view = view;
            _updateInterval = updateInterval;

            _lastSentHash = 0;

            _targetPos = _view.transform.localPosition;
            _targetRot = _view.transform.localRotation;
        }

        public void Update(float elapsedTime, bool isMine)
        {
            if (isMine == true)
            {
                _updateElapsedTime += elapsedTime;
                if (_updateElapsedTime >= _updateInterval)
                {
                    var writer = NetPool.DataWriterPool.Alloc();

                    try
                    {
                        var transform = _view.transform;

                        writer.Write((ushort)NetProtocol.P2pPosRotSync);
                        writer.Write(_view.ViewId);

                        writer.Write(transform.localPosition);
                        writer.Write(transform.localRotation);

                        var hash = writer.GetHashCode();
                        if (_lastSentHash != hash)
                        {
                            _lastSentHash = hash;

                            //Debug.Log($"send pos rot {hash}");
                            NetClientGlobal.Instance.SendAll(writer, DeliveryMethod.Unreliable);
                        }
                    }
                    finally
                    {
                        NetPool.DataWriterPool.Free(writer);
                    }

                    _updateElapsedTime = 0;
                }
            }
            else
            {
                var transform = _view.transform;

                if ((_targetPos - transform.localPosition).sqrMagnitude >= NetClientGlobal.Instance.PrecisionForVectorSqrtSync)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, Time.deltaTime * 10f);
                }
                else
                {
                    transform.localPosition = _targetPos;
                }

                if (Quaternion.Angle(_targetRot, transform.localRotation) >= NetClientGlobal.Instance.PrecisionForQuaternionSync)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRot, Time.deltaTime * 10f);
                }
                else
                {
                    transform.localRotation = _targetRot;
                }
            }
        }

        public void OnReceiveSync(NetDataReader reader)
        {
            _targetPos = reader.ReadVector3();
            _targetRot = reader.ReadQuaternion();

            var transform = _view.transform;

            if((_targetPos - transform.localPosition).sqrMagnitude >= NetClientGlobal.Instance.LimitForPositionSqrtSync)
                transform.localPosition = _targetPos;

            if (Quaternion.Angle(_targetRot, transform.localRotation) >= NetClientGlobal.Instance.LimitForRotationSync)
                transform.localRotation = _targetRot;
        }
    }
}

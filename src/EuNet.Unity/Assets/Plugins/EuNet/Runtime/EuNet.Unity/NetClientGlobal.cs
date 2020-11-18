using EuNet.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    [ExecutionOrder(-10000)]
    [AddComponentMenu("EuNet/NetClientGlobal")]
    public class NetClientGlobal : Singleton<NetClientGlobal>
    {
        [Header("Mode")]
        [SerializeField] private bool _isLocalMode = false;
        public bool IsLocalMode => _isLocalMode;

        [Header("Synchronization")]
        [SerializeField] private float _precisionForVectorSqrtSync = 0.0001f;
        public float PrecisionForVectorSqrtSync => _precisionForVectorSqrtSync;

        [SerializeField] private float _precisionForQuaternionSync = 0.1f;
        public float PrecisionForQuaternionSync => _precisionForQuaternionSync;

        [SerializeField] private float _limitForPositionSqrtSync = 100f;
        public float LimitForPositionSqrtSync => _limitForPositionSqrtSync;

        [SerializeField] private float _limitForRotationSync = 90f;
        public float LimitForRotationSync => _limitForRotationSync;

        private NetClientP2p _client;
        public NetClientP2p Client => _client;

        public ushort SessionId => _client.SessionId;
        public TcpChannel TcpChannel => _client.TcpChannel;
        public UdpChannel UdpChannel => _client.UdpChannel;
        public NetViews Views => _client.Views;
        public SessionState State => _client.State;

        public void SetClient(NetClientP2p client)
        {
            if (client != null && _client != null)
                Debug.LogError("[NetClientP2p] should have only one instance.");

            _client = client;
        }

        public bool RegisterView(NetView view)
        {
            return Client.RegisterView(view);
        }

        public bool UnregisterView(NetView view)
        {
            if (Client == null)
                return false;

            return Client.UnregisterView(view);
        }

        public int GenerateViewId()
        {
            return Client.GenerateViewId();
        }

        public int GenerateSceneViewId()
        {
            return Client.GenerateSceneViewId();
        }

        public void RemoveViewId(int viewId)
        {
            Client?.RemoveViewId(viewId);
        }

        public bool MasterIsMine()
        {
            if (IsLocalMode == true)
                return true;

            return Client.MasterIsMine();
        }

        public GameObject Instantiate(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Client.Instantiate(name, pos, rot, writer);
        }

        public GameObject InstantiateSceneObject(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Client.InstantiateSceneObject(name, pos, rot, writer);
        }

        public void Destroy(int viewId, NetDataWriter writer = null)
        {
            Client.Destroy(viewId, writer);
        }

        public void SendP2pMessage(INetView view, NetDataWriter writer, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod)
        {
            if (IsLocalMode == true)
                return;

            Client.SendP2pMessage(view, writer, deliveryTarget, deliveryMethod);
        }

        public void SendAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            if (IsLocalMode == true)
                return;

            Client.SendAll(writer, deliveryMethod);
        }

        public Task RequestRecovery()
        {
            if (IsLocalMode == true)
                return Task.CompletedTask;

            return Client.RequestRecovery();
        }
    }
}
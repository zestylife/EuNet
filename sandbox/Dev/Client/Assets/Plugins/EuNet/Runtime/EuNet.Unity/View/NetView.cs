using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    [ExecutionOrder(100)]
    public class NetView : MonoBehaviour, INetView
    {
        [Header("NetView")]
        [SerializeField] private int _viewId;

        // 이 오브젝트의 소유자
        [SerializeField] private ushort _ownerSessionId;

        // 이 오브젝트는 Scene 오브젝트 인가? ( true라면 소유자는 Master가 된다 )
        [SerializeField] private bool _isSceneObject;

        // 동기화 타입
        [SerializeField] private NetViewSyncType _viewSyncType;
        [SerializeField] private float _viewSyncInterval = 0.05f;

        public string PrefabPath { get; set; } = string.Empty;
        private INetViewHandler[] _viewHandlers;
        private INetViewPeriodicSync[] _viewPeriodicSyncs;
        private int[] _viewPeriodicSyncHashs;
        private IRpcInvokable[] _viewRpcInvokables;
        private INetSerializable _serializer;
        private NetViewPosRotSync _posRotSync;

        private Dictionary<Type, object> _typeRpcMap = new Dictionary<Type, object>();

        public int ViewId
        {
            get { return _viewId;  }
            set
            {
                if (_viewId == value)
                    return;

                if (_viewId != 0)
                {
                    if (Application.isPlaying)
                    {
                        NetClientGlobal.Instance.UnregisterView(this);
                        NetClientGlobal.Instance.RemoveViewId(_viewId);
                    }
                }

                if (value != 0)
                {
                    _viewId = value;

                    if (Application.isPlaying)
                        NetClientGlobal.Instance.RegisterView(this);
                }

                _viewId = value;
            }
        }

        public ushort OwnerSessionId
        {
            get { return _ownerSessionId; }
            set { _ownerSessionId = value; }
        }

        public bool IsSceneObject
        {
            get { return _isSceneObject; }
            set { _isSceneObject = value; }
        }

        protected virtual void Awake()
        {
            if (ViewId != 0)
                NetClientGlobal.Instance.RegisterView(this);

            _viewHandlers = GetComponents<INetViewHandler>();
            _viewPeriodicSyncs = GetComponents<INetViewPeriodicSync>();
            _viewRpcInvokables = GetComponents<IRpcInvokable>();

            var serializerComp = GetComponents<INetSerializable>();
            if (serializerComp.Length > 1)
            {
                Debug.LogError($"Found multiple INetSerializable in one gameobject. please delete only one left", gameObject);
            }

            if (serializerComp.Length > 0)
                _serializer = serializerComp[0];

            _viewPeriodicSyncHashs = new int[_viewPeriodicSyncs.Length];

            switch (_viewSyncType)
            {
                case NetViewSyncType.PositionAndRotation:
                    _posRotSync = new NetViewPosRotSync(this, _viewSyncInterval);
                    break;
            }
        }

        protected virtual void OnDestroy()
        {
            if (ViewId > 0)
            {
                NetClientGlobal.Instance.UnregisterView(this);
                NetClientGlobal.Instance.RemoveViewId(ViewId);
            }
        }

        public void OnUpdate(float elapsedTime)
        {
            _posRotSync?.Update(elapsedTime, IsMine());
        }

        public bool IsMine()
        {
            if (_isSceneObject == true)
            {
                if (NetClientGlobal.Instance.MasterIsMine() == true)
                    return true;
            }
            else
            {
                if (NetClientGlobal.Instance.SessionId == _ownerSessionId)
                    return true;
            }

            return false;
        }

        public void OnNetInstantiate(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewInstantiate(reader);
            }
        }

        public void OnNetDestroy(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewDestroy(reader);
            }
        }

        public void SendMessage(NetDataWriter writer, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod)
        {
            NetClientGlobal.Instance.SendP2pMessage(this, writer, deliveryTarget, deliveryMethod);
        }

        public void OnMessage(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewMessage(reader);
            }
        }

        public void OnNetSerialize(NetDataWriter writer)
        {
            writer.Write(_isSceneObject);
            writer.Write(_ownerSessionId);
            writer.Write(transform.localPosition);
            writer.Write(transform.localRotation);
            writer.Write(transform.localScale);

            _serializer?.Serialize(writer);
        }

        public void OnNetDeserialize(NetDataReader reader)
        {
            _isSceneObject = reader.ReadBoolean();
            _ownerSessionId = reader.ReadUInt16();
            transform.localPosition = reader.ReadVector3();
            transform.localRotation = reader.ReadQuaternion();
            transform.localScale = reader.ReadVector3();

            _serializer?.Deserialize(reader);
        }

        public void OnNetSync(NetProtocol procorol, NetDataReader reader)
        {
            switch(procorol)
            {
                case NetProtocol.P2pPosRotSync:
                    _posRotSync?.OnReceiveSync(reader);
                    break;
            }
        }

        public bool OnViewPeriodicSyncSerialize(NetDataWriter writer)
        {
            var firstPos = writer.Length;
            bool isExist = false;

            for(int i=0; i<_viewPeriodicSyncs.Length; ++i)
            {
                var sync = _viewPeriodicSyncs[i];
                var prevPos = writer.Length;
                writer.Write(true);

                var result = sync.OnViewPeriodicSyncSerialize(writer);

                int hash = writer.GetHashCode(prevPos, writer.Length - prevPos);
                if (result == true && _viewPeriodicSyncHashs[i] != hash)
                {
                    _viewPeriodicSyncHashs[i] = hash;
                    isExist = true;
                }
                else
                {
                    writer.Length = prevPos;
                    writer.Write(false);
                }
            }

            if (isExist == false)
                writer.Length = firstPos;

            return isExist;
        }

        public void OnViewPeriodicSyncDeserialize(NetDataReader reader)
        {
            foreach (var sync in _viewPeriodicSyncs)
            {
                var isData = reader.ReadBoolean();
                if(isData)
                    sync.OnViewPeriodicSyncDeserialize(reader);
            }
        }

        public void OnNetViewRequestReceive(ISession session, NetDataReader reader, NetDataWriter writer)
        {
            var preReaderPos = reader.Position;
            var preWriterPos = writer.Length;

            foreach (var handler in _viewRpcInvokables)
            {
                reader.Position = preReaderPos;
                writer.Length = preWriterPos;

                var result = handler.Invoke(session, reader, writer).Result;
                if (result == true)
                    return;
            }
        }

        public T FindRpcHandler<T>() where T : class
        {
            object viewRpc;
            if (_typeRpcMap.TryGetValue(typeof(T), out viewRpc))
                return viewRpc as T;

            viewRpc = gameObject.GetComponent<T>();
            if (viewRpc == null)
                throw new Exception($"Not found override [{typeof(T).Name}] interface. GameObject : {gameObject.name}");

            _typeRpcMap.Add(typeof(T), viewRpc);
            return viewRpc as T;
        }
    }
}
using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// 게임오브젝트의 동기화를 위한 NetView
    /// </summary>
    [ExecutionOrder(100)]
    [AddComponentMenu("EuNet/NetView")]
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
        
        private INetViewHandler[] _viewHandlers;
        private INetViewPeriodicSync[] _viewPeriodicSyncs;
        private int[] _viewPeriodicSyncHashs;
        private IRpcInvokable[] _viewRpcInvokables;
        private INetSerializable _serializer;
        private NetViewPosRotSync _posRotSync;

        private Dictionary<Type, object> _typeRpcMap = new Dictionary<Type, object>();

        /// <summary>
        /// 다른 유저와의 게임오브젝트 동기화를 위한 ViewId.
        /// </summary>
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

        /// <summary>
        /// 소유자 세션 아이디
        /// </summary>
        public ushort OwnerSessionId
        {
            get { return _ownerSessionId; }
            set { _ownerSessionId = value; }
        }

        /// <summary>
        /// 씬 오브젝트인지 여부. 컨트롤 소유자가 없는 예를 들어 몬스터나 미니언 등
        /// </summary>
        public bool IsSceneObject
        {
            get { return _isSceneObject; }
            set { _isSceneObject = value; }
        }

        /// <summary>
        /// 생성된 Resources 의 Prefab의 Full Path. 복구시 재생성에 사용됨.
        /// </summary>
        public string PrefabPath { get; set; } = string.Empty;

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

        /// <summary>
        /// Unity 업데이트때 호출해주어야 함
        /// </summary>
        /// <param name="elapsedTime">지난시간. Time.UnscaledDeltaTime</param>
        public void OnUpdate(float elapsedTime)
        {
            _posRotSync?.Update(elapsedTime, IsMine());
        }

        /// <summary>
        /// 본인이 소유하고 있는지 여부. Scene Object 의 경우 마스터이면 소유하고 있다고 판단함.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 네트워크로 게임오브젝트가 생성됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        public void OnNetInstantiate(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewInstantiate(reader);
            }
        }

        /// <summary>
        /// 네트워크로 게임오브젝트가 파괴됨
        /// </summary>
        /// <param name="reader">전달된 데이터</param>
        public void OnNetDestroy(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewDestroy(reader);
            }
        }

        /// <summary>
        /// P2P 유저에게 데이터 전송
        /// </summary>
        /// <param name="dataWriter">전송할 데이터</param>
        /// <param name="deliveryTarget">전송 타겟</param>
        /// <param name="deliveryMethod">전송 방식</param>
        public void SendMessage(NetDataWriter dataWriter, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod)
        {
            NetClientGlobal.Instance.SendP2pMessage(this, dataWriter, deliveryTarget, deliveryMethod);
        }

        /// <summary>
        /// 메시지가 도착함
        /// </summary>
        /// <param name="reader">전달된 메시지 데이터</param>
        public void OnNetMessage(NetDataReader reader)
        {
            var initPos = reader.Position;

            foreach (var handler in _viewHandlers)
            {
                reader.Position = initPos;
                handler.OnViewMessage(reader);
            }
        }

        /// <summary>
        /// 복구시 사용할 직렬화
        /// </summary>
        /// <param name="writer">직렬화 데이터를 담을 NetDataWriter</param>
        public void OnNetSerialize(NetDataWriter writer)
        {
            writer.Write(_isSceneObject);
            writer.Write(_ownerSessionId);
            writer.Write(transform.localPosition);
            writer.Write(transform.localRotation);
            writer.Write(transform.localScale);

            _serializer?.Serialize(writer);
        }

        /// <summary>
        /// 복구시 사용할 역직렬화
        /// </summary>
        /// <param name="reader">역직렬화시 사용할 데이터</param>
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

        /// <summary>
        /// 주기적인 동기화를 위한 직렬화
        /// </summary>
        /// <param name="dataWriter">직렬화 데이터</param>
        /// <param name="syncType">주기적 동기화 타입</param>
        /// <returns>다른 유저에게 동기화될지 여부</returns>
        public bool OnViewPeriodicSyncSerialize(NetDataWriter dataWriter, PeriodicSyncType syncType)
        {
            var firstPos = dataWriter.Length;
            bool isExist = false;

            for(int i=0; i<_viewPeriodicSyncs.Length; ++i)
            {
                var sync = _viewPeriodicSyncs[i];
                var prevPos = dataWriter.Length;
                dataWriter.Write(true);

                var result = sync.OnViewPeriodicSyncSerialize(dataWriter);

                int hash = 0;

                if(syncType == PeriodicSyncType.Changed)
                    hash = dataWriter.GetHashCode(prevPos, dataWriter.Length - prevPos);

                if(result == true && 
                    (syncType == PeriodicSyncType.Always || (syncType == PeriodicSyncType.Changed && _viewPeriodicSyncHashs[i] != hash)))
                {
                    _viewPeriodicSyncHashs[i] = hash;
                    isExist = true;
                }
                else
                {
                    dataWriter.Length = prevPos;
                    dataWriter.Write(false);
                }
            }

            if (isExist == false)
                dataWriter.Length = firstPos;

            return isExist;
        }

        /// <summary>
        /// 주기적 동기화 역직렬화
        /// </summary>
        /// <param name="reader">역직렬화할 데이터</param>
        public void OnViewPeriodicSyncDeserialize(NetDataReader reader)
        {
            foreach (var sync in _viewPeriodicSyncs)
            {
                var isData = reader.ReadBoolean();
                if(isData)
                    sync.OnViewPeriodicSyncDeserialize(reader);
            }
        }

        /// <summary>
        /// NetView 요청을 받음
        /// </summary>
        /// <param name="session">받은 세션</param>
        /// <param name="reader">받은 데이터</param>
        /// <param name="writer">응답시 전달할 데이터를 담을 객체</param>
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
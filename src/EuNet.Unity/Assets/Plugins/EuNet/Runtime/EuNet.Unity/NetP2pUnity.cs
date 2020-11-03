using EuNet.Client;
using EuNet.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    public class NetP2pUnity : NetUnity
    {
        public int UdpServerPort = 12001;
        public int PingInterval = 1000;
        public int MtuInterval = 1100;
        public int RudpDisconnectTimeout = 5000;

        [Header("Lifecycle")]
        [SerializeField] private bool _isDontDestroyOnLoad = true;

        [Header("Synchronization")]
        [SerializeField] private float _precisionForVectorSqrtSync = 0.0001f;
        public float PrecisionForVectorSqrtSync => _precisionForVectorSqrtSync;

        [SerializeField] private float _precisionForQuaternionSync = 0.1f;
        public float PrecisionForQuaternionSync => _precisionForQuaternionSync;

        [SerializeField] private float _limitForPositionSqrtSync = 100f;
        public float LimitForPositionSqrtSync => _limitForPositionSqrtSync;

        [SerializeField] private float _limitForRotationSync = 90f;
        public float LimitForRotationSync => _limitForRotationSync;

        private NetViews _views;
        public NetViews Views => _views;

        private NetDataWriter _zeroDataWriter;
        private NetDataReader _readerForSendInternal;
        private int _recoveryId;
        private TaskCompletionSource<bool> _recoveryTcs;
        private float _syncInterval = 0.05f;
        private float _syncElapsedTime;
        private List<IRpcInvokable> _rpcHandlers = new List<IRpcInvokable>();

        public ushort SessionId => _client.SessionId;

        private static NetP2pUnity s_instance;
        public static NetP2pUnity Instance
        {
            get
            {
                if (s_inited == false)
                {
                    s_inited = true;
                    s_instance = FindObjectOfType<NetP2pUnity>();
                }

                return s_instance;
            }
        }
        private static bool s_inited = false;

        protected override void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this;

                if(_isDontDestroyOnLoad)
                    DontDestroyOnLoad(this.gameObject);

                s_inited = true;
            }
            else
            {
                //Debug.LogErrorFormat(gameObject, "Already created singleton object {0}", typeof(NetP2pUnity));
                Destroy(this);
                return;
            }

            _clientOption.IsServiceUdp = true;
            _clientOption.UdpServerAddress = ServerAddress;
            _clientOption.UdpServerPort = UdpServerPort;
            _clientOption.PingInterval = PingInterval;
            _clientOption.MtuInterval = MtuInterval;
            _clientOption.RudpDisconnectTimeout = RudpDisconnectTimeout;

            base.Awake();

            _client.OnP2pReceived += OnP2pReceive;
            _client.OnViewRequestReceived = OnViewRequestReceive;

            _views = new NetViews();
            _zeroDataWriter = new NetDataWriter(false, 1);
            _readerForSendInternal = new NetDataReader();

            if (_clientOption.IsServiceUdp == false)
                Debug.LogError("[NetP2pUnity] require [IsServiceUdp] option");
        }

        protected virtual void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
                s_inited = false;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            _views.Update(Time.deltaTime);

            _syncElapsedTime += Time.deltaTime;
            if (_syncElapsedTime >= _syncInterval)
            {
                _syncElapsedTime = 0f;
                OnViewPeriodicSyncSerialize();
            }
        }

        public void AddRpcService(IRpcInvokable service)
        {
            if (_rpcHandlers.Contains(service))
                throw new Exception("Already exist IRpcInvokable in _rpcHandlers");

            _rpcHandlers.Add(service);
        }

        public bool RegisterView(NetView view)
        {
            return _views.RegisterView(view);
        }

        public bool UnregisterView(NetView view)
        {
            return _views.UnregisterView(view);
        }

        public int GenerateViewId()
        {
            return _views.GenerateViewId(_client.SessionId);
        }

        public int GenerateSceneViewId()
        {
            if (MasterIsMine() == false)
            {
                Debug.LogError("Only the master client can GenerateSceneViewId()");
                return -1;
            }

            return _views.GenerateViewId(0);
        }

        public void RemoveViewId(int viewId)
        {
            _views.RemoveViewId(viewId);
        }

        public bool MasterIsMine()
        {
            if (_client == null || 
                _client.P2pGroup == null)
                return false;

            return _client.P2pGroup.MasterIsMine();
        }

        private void SendP2pInternal(
            NetDataWriter writer,
            DeliveryTarget deliveryTarget,
            DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
            {
                Debug.LogError("not support tcp transfer in p2p");
                return;
            }

            if (deliveryTarget == DeliveryTarget.All)
            {
                _client.P2pGroup.SendAll(writer, deliveryMethod);

                _readerForSendInternal.SetSource(writer);
                OnP2pReceive(_client, _readerForSendInternal);
            }
            else if (deliveryTarget == DeliveryTarget.Others)
            {
                _client.P2pGroup.SendAll(writer, deliveryMethod);
            }
            else if (deliveryTarget == DeliveryTarget.Master)
            {
                var member = _client.P2pGroup.Find(_client.P2pGroup.MasterSessionId);

                if (member != null)
                {
                    if (member.IsMine())
                    {
                        // 내가 마스터라면 바로 호출함
                        _readerForSendInternal.SetSource(writer);
                        OnP2pReceive(_client, _readerForSendInternal);
                    }
                    else
                    {
                        member.SendAsync(writer, deliveryMethod);
                    }
                }
            }
        }

        private void SendP2pInternal(
            NetDataWriter writer,
            ushort sessionId,
            DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
            {
                Debug.LogError("not support tcp transfer in p2p");
                return;
            }

            if(sessionId == SessionId)
            {
                // 내가 받아야 한다면
                _readerForSendInternal.SetSource(writer);
                OnP2pReceive(_client, _readerForSendInternal);
            }
            else
            {
                var member = _client.P2pGroup.Find(sessionId);

                if (member != null)
                    member.SendAsync(writer, deliveryMethod);
            }
        }

        private GameObject Instantiate(string name, Vector3 pos, Quaternion rot, bool isSceneObject, NetDataWriter writer = null)
        {
            if(isSceneObject == true && MasterIsMine() == false)
            {
                Debug.LogError("Only can master client instantiate scene object.");
                return null;
            }

            writer = writer ?? _zeroDataWriter;

            GameObject prefab = (GameObject)Resources.Load(name, typeof(GameObject));

            if (prefab.GetComponent<NetView>() == null)
            {
                Debug.LogError("Failed to Instantiate prefab:" + name + ". Prefab must have a EveView component.");
                return null;
            }

            NetView[] views = prefab.GetComponentsInChildren<NetView>(true);

            List<int> viewIds = new List<int>(views.Length);
            for (int i = 0; i < views.Length; i++)
            {
                if(isSceneObject)
                    viewIds.Add(GenerateSceneViewId());
                else viewIds.Add(GenerateViewId());
            }

            var viewIdsArray = viewIds.ToArray();
            ushort ownerSessionId = _client.SessionId;

            var w = NetPool.DataWriterPool.Alloc();

            try
            {
                w.Write((ushort)NetProtocol.P2pInstantiate);
                w.Write(name);
                w.Write(pos);
                w.Write(rot);

                w.Write(ownerSessionId);
                w.Write(isSceneObject);
                w.Write(viewIdsArray);
                w.Write(writer);

                SendP2pInternal(w, DeliveryTarget.Others, DeliveryMethod.ReliableOrdered);

                var reader = new NetDataReader(writer);
                return ExecuteInstantiate(name, pos, rot, ownerSessionId, isSceneObject, viewIdsArray, reader);
            }
            finally
            {
                NetPool.DataWriterPool.Free(w);
            }
        }

        public GameObject Instantiate(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Instantiate(name, pos, rot, false, writer);
        }

        public GameObject InstantiateSceneObject(string name, Vector3 pos, Quaternion rot, NetDataWriter writer = null)
        {
            return Instantiate(name, pos, rot, true, writer);
        }

        private GameObject ExecuteInstantiate(string name, Vector3 pos, Quaternion rot, ushort ownerSessionId, bool isSceneObject, int[] viewIds, NetDataReader reader)
        {
            var res = Resources.Load(name);
            if(res == null)
            {
                Debug.LogError($"P2pInstantiate can not found prefab resource : {name}", this);
                return null;
            }

            var obj = GameObject.Instantiate(res, pos, rot) as GameObject;
            var view = obj.GetComponent<NetView>();
            if (view == null)
            {
                Debug.LogError("P2pInstantiate object is not include [NetView] component", obj);
                return null;
            }

            // 뷰와 하위 뷰까지 모두 등록해주자
            NetView[] views = obj.GetComponentsInChildren<NetView>(true);
            for (int i = 0; i < views.Length; i++)
            {
                views[i].ViewId = viewIds[i];
                views[i].OwnerSessionId = ownerSessionId;
                views[i].IsSceneObject = isSceneObject;
                views[i].PrefabPath = name;
            }

            // 콜백호출
            view.OnNetInstantiate(reader);

            return view.gameObject;
        }

        private void OnP2pInstantiate(NetDataReader reader)
        {
            var name = reader.ReadString();
            var pos = reader.ReadVector3();
            var rot = reader.ReadQuaternion();

            var ownerSessionId = reader.ReadUInt16();
            var isSceneObject = reader.ReadBoolean();
            var viewIds = reader.ReadInt32Array();

            ExecuteInstantiate(name, pos, rot, ownerSessionId, isSceneObject, viewIds, reader);
        }

        public void Destroy(int viewId, NetDataWriter writer = null)
        {
            writer = writer ?? _zeroDataWriter;

            NetPool.DataWriterPool.Use((NetDataWriter w) =>
            {
                w.Write((ushort)NetProtocol.P2pDestroy);
                w.Write(viewId);
                w.Write(writer);

                SendP2pInternal(w, DeliveryTarget.All, DeliveryMethod.ReliableOrdered);
            });
        }

        private void OnP2pDestroy(NetDataReader reader)
        {
            var viewId = reader.ReadInt32();

            var view = _views.Find(viewId);
            if (view == null)
            {
                Debug.LogError($"ViewId[{viewId}] not exist gameObject for destroy");
                return;
            }

            // 뷰와 하위 뷰까지 모두 등록해제하자
            NetView[] views = view.gameObject.GetComponentsInChildren<NetView>(true);
            for (int i = 0; i < views.Length; i++)
            {
                views[i].ViewId = 0;
            }

            // 콜백호출
            view.OnNetDestroy(reader);

            // 삭제
            Destroy(view.gameObject);
        }

        public void SendP2pMessage(INetView view, NetDataWriter writer, DeliveryTarget deliveryTarget, DeliveryMethod deliveryMethod)
        {
            NetPool.DataWriterPool.Use((NetDataWriter w) =>
            {
                w.Write((ushort)NetProtocol.P2pMessage);
                w.Write(view.ViewId);
                w.Write(writer);

                SendP2pInternal(w, deliveryTarget, deliveryMethod);
            });
        }

        public async Task RequestRecovery()
        {
            if (MasterIsMine() == true)
            {
                Debug.LogError("Only can no master client request recovery");
                throw new Exception("Only can no master client request recovery");
            }

            if(_recoveryTcs != null)
                _recoveryTcs.TrySetResult(false);

            _recoveryTcs = new TaskCompletionSource<bool>();

            int recoveryId = ++_recoveryId;

            NetPool.DataWriterPool.Use((NetDataWriter writer) =>
            {
                writer.Write((ushort)NetProtocol.P2pRequestRecovery);
                writer.Write(SessionId);
                writer.Write(recoveryId);

                SendP2pInternal(writer, DeliveryTarget.Master, DeliveryMethod.ReliableOrdered);
            });

            await _recoveryTcs.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
        }

        private void OnRequestRecovery(NetDataReader reader)
        {
            Debug.Log("OnRequestRecovery");

            ushort sessionId = reader.ReadUInt16();
            int recoveryId = reader.ReadInt32();

            NetPool.DataWriterPool.Use((NetDataWriter writer) =>
            {
                writer.Write((ushort)NetProtocol.P2pResponseRecovery);
                writer.Write(sessionId);
                writer.Write(recoveryId);
                writer.Write(_views);

                SendP2pInternal(writer, sessionId, DeliveryMethod.ReliableOrdered);
            });
        }

        private void OnResponseRecovery(NetDataReader reader)
        {
            Debug.Log("OnResponseRecovery");

            ushort sessionId = reader.ReadUInt16();
            int recoveryId = reader.ReadInt32();

            if (sessionId != SessionId || _recoveryId != recoveryId)
                return;

            try
            {
                reader.TryRead(ref _views);
                _recoveryTcs?.TrySetResult(true);
            }
            catch(Exception ex)
            {
                _recoveryTcs?.TrySetException(ex);
            }
        }

        private void OnViewPeriodicSyncSerialize()
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                foreach (var kvp in _views.Views)
                {
                    if (kvp.Value.IsMine() == false)
                        continue;

                    writer.Reset();
                    writer.Write((ushort)NetProtocol.P2pPeriodicSync);
                    writer.Write(kvp.Key);

                    if (kvp.Value.OnViewPeriodicSyncSerialize(writer))
                    {
                        SendP2pInternal(writer, DeliveryTarget.Others, DeliveryMethod.Unreliable);
                    }
                }
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        private void OnViewPeriodicSyncDeserialize(NetDataReader reader)
        {
            int viewId = reader.ReadInt32();

            var view = _views.Find(viewId);
            if (view == null)
                return;

            view.OnViewPeriodicSyncDeserialize(reader);
        }

        private Task OnP2pReceive(ISession session, NetDataReader reader)
        {
            NetProtocol protocol = (NetProtocol)reader.ReadUInt16();

            switch (protocol)
            {
                case NetProtocol.P2pInstantiate:
                    {
                        OnP2pInstantiate(reader);
                    }
                    break;
                case NetProtocol.P2pDestroy:
                    {
                        OnP2pDestroy(reader);
                    }
                    break;
                case NetProtocol.P2pMessage:
                    {
                        int viewId = reader.ReadInt32();

                        var view = _views.Find(viewId);
                        if (view == null)
                            return Task.CompletedTask;

                        view.OnMessage(reader);
                    }
                    break;
                case NetProtocol.P2pRequestRecovery:
                    {
                        OnRequestRecovery(reader);
                    }
                    break;
                case NetProtocol.P2pResponseRecovery:
                    {
                        OnResponseRecovery(reader);
                    }
                    break;
                case NetProtocol.P2pPeriodicSync:
                    {
                        OnViewPeriodicSyncDeserialize(reader);
                    }
                    break;
                default:
                    {
                        int viewId = reader.ReadInt32();

                        var view = _views.Find(viewId);
                        if (view == null)
                            return Task.CompletedTask;

                        view.OnNetSync(protocol, reader);
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        public void SendAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            _client.P2pGroup?.SendAll(writer, deliveryMethod);
        }

        internal Task OnViewRequestReceive(ISession session, NetDataReader reader, NetDataWriter writer)
        {
            var viewId = reader.ReadInt32();
            var view = _views.Find(viewId);
            if (view == null)
            {
                Debug.LogWarning($"Can not found view : {viewId} in OnNetViewRequestReceive");
                return Task.CompletedTask;
            }

            var preReaderPos = reader.Position;
            var preWriterPos = writer.Length;

            foreach (var handler in _rpcHandlers)
            {
                reader.Position = preReaderPos;
                writer.Length = preWriterPos;

                var result = handler.Invoke(view, reader, writer).Result;
                if (result == true)
                    return Task.CompletedTask;
            }

            reader.Position = preReaderPos;
            writer.Length = preWriterPos;

            view.OnNetViewRequestReceive(session, reader, writer);
            return Task.CompletedTask;
        }
    }
}

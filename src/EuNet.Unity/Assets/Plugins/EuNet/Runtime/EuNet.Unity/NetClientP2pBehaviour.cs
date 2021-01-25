using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// P2P를 포함한 CS 전용 클라이언트 컴포넌트
    /// </summary>
    [ExecutionOrder(-1000)]
    [AddComponentMenu("EuNet/NetClientP2pBehaviour")]
    public class NetClientP2pBehaviour : NetClientBehaviour
    {
        public int UdpServerPort = 12001;
        public int PingInterval = 1000;
        public int MtuInterval = 1100;
        public int RudpDisconnectTimeout = 5000;
        public PeriodicSyncType SyncType = PeriodicSyncType.None;
        public DeliveryMethod SyncSendMethod = DeliveryMethod.Unreliable;
        public float SyncInterval = 0.1f;
        
        public NetClientP2p ClientP2p => _client as NetClientP2p;

        private static NetClientP2pBehaviour s_instance;

        protected override void Awake()
        {
            if(s_instance != null)
            {
                Destroy(this);
                return;
            }

            s_instance = this;

            _isDontDestroyOnLoad = true;
            DontDestroyOnLoad(gameObject);
        }

        protected override void Start()
        {
            _clientOption.IsServiceUdp = true;
            _clientOption.UdpServerAddress = ServerAddress;
            _clientOption.UdpServerPort = UdpServerPort;
            _clientOption.PingInterval = PingInterval;
            _clientOption.MtuInterval = MtuInterval;
            _clientOption.RudpDisconnectTimeout = RudpDisconnectTimeout;
            _clientOption.TcpServerAddress = ServerAddress;
            _clientOption.TcpServerPort = TcpServerPort;
            _clientOption.IsCheckAlive = IsCheckAlive;
            _clientOption.CheckAliveInterval = CheckAliveInterval;
            _clientOption.CheckAliveTimeout = CheckAliveTimeout;

            SetClientOption(_clientOption);

            _client = new NetClientP2p(
                _clientOption,
                DefaultLoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel);
                    builder.AddUnityDebugLogger();
                }));

            ClientP2p.SyncType = SyncType;
            ClientP2p.SyncSendMethod = SyncSendMethod;
            ClientP2p.SyncInterval = SyncInterval;

            _client.OnErrored += OnError;

            NetClientGlobal.Instance.SetClient(ClientP2p);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void FixedUpdate()
        {
            ClientP2p.FixedUpdate(Time.deltaTime);
        }
    }
}
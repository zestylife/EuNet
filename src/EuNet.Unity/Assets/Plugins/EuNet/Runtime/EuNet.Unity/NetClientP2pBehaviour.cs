using EuNet.Core;
using UnityEngine;

namespace EuNet.Unity
{
    [ExecutionOrder(-1000)]
    public class NetClientP2pBehaviour : NetClientBehaviour
    {
        public int UdpServerPort = 12001;
        public int PingInterval = 1000;
        public int MtuInterval = 1100;
        public int RudpDisconnectTimeout = 5000;
        
        public NetClientP2p ClientP2p => _client as NetClientP2p;

        protected override void Awake()
        {
            _clientOption.IsServiceUdp = true;
            _clientOption.UdpServerAddress = ServerAddress;
            _clientOption.UdpServerPort = UdpServerPort;
            _clientOption.PingInterval = PingInterval;
            _clientOption.MtuInterval = MtuInterval;
            _clientOption.RudpDisconnectTimeout = RudpDisconnectTimeout;
            _clientOption.TcpServerAddress = ServerAddress;
            _clientOption.TcpServerPort = TcpServerPort;

            _client = new NetClientP2p(
                _clientOption,
                DefaultLoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel);
                    builder.AddUnityDebugLogger();
                }));

            _client.OnErrored += OnError;

            _isDontDestroyOnLoad = true;
            DontDestroyOnLoad(gameObject);

            NetClientGlobal.Instance.SetClient(ClientP2p);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetClientGlobal.Instance.SetClient(null);
        }

        protected override void FixedUpdate()
        {
            ClientP2p.FixedUpdate(Time.deltaTime);
        }
    }
}
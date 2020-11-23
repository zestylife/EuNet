using EuNet.Client;
using EuNet.Core;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// CS 전용 클라이언트 컴포넌트
    /// </summary>
    [ExecutionOrder(-1000)]
    [AddComponentMenu("EuNet/NetClientBehaviour")]
    public class NetClientBehaviour : MonoBehaviour
    {
        [Header("Lifecycle")]
        [SerializeField] protected bool _isDontDestroyOnLoad = true;

        [Header("Log Level")]
        public LogLevel LogLevel = LogLevel.Information;

        [Header("Network Infomation")]
        public string ServerAddress = "127.0.0.1";
        public int TcpServerPort = 12000;
        public bool IsCheckAlive = false;
        public long CheckAliveInterval = 30000;
        public long CheckAliveTimeout = 50000;

        protected ClientOption _clientOption = new ClientOption();
        protected NetClient _client;
        public NetClient Client => _client;

        protected virtual void Awake()
        {
            _clientOption.TcpServerAddress = ServerAddress;
            _clientOption.TcpServerPort = TcpServerPort;
            _clientOption.IsCheckAlive = IsCheckAlive;
            _clientOption.CheckAliveInterval = CheckAliveInterval;
            _clientOption.CheckAliveTimeout = CheckAliveTimeout;

            SetClientOption(_clientOption);

            _client = new NetClient(
                _clientOption,
                DefaultLoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel);
                    builder.AddUnityDebugLogger();
                }));

            _client.OnErrored += OnError;

            if (_isDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {
            _client?.Disconnect();
        }

        protected virtual void FixedUpdate()
        {
            int elapsedTime = (int)(Time.deltaTime * 1000f);
            _client.Update(elapsedTime);
        }

        private void OnApplicationQuit()
        {
            _client.Disconnect();
        }

        protected void OnError(Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        /// <summary>
        /// 이 함수를 오버라이딩하여 옵션을 수정할 수 있음.
        /// </summary>
        /// <param name="clientOption"></param>
        protected virtual void SetClientOption(ClientOption clientOption)
        {

        }
    }
}
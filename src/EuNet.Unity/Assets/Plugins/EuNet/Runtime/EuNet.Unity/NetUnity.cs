using EuNet.Client;
using EuNet.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace EuNet.Unity
{
    [ExecutionOrder(-1000)]
    public class NetUnity : MonoBehaviour
    {
        [Header("Log Level")]
        public LogLevel LogLevel = LogLevel.Information;

        [Header("Network Infomation")]
        public string ServerAddress = "192.168.0.4";
        public int TcpServerPort = 12000;

        protected ClientOption _clientOption = new ClientOption();
        protected NetClient _client;
        public NetClient Client => _client;

        protected virtual void Awake()
        {
            _clientOption.TcpServerAddress = ServerAddress;
            _clientOption.TcpServerPort = TcpServerPort;
            
            _client = new NetClient(
                _clientOption,
                DefaultLoggerFactory.Create(builder => 
                {
                    builder.SetMinimumLevel(LogLevel);
                    builder.AddUnityDebugLogger();
                }));

            //_client.OnReceived += OnReceive;
            _client.OnErrored += OnError;
        }

        protected virtual void Start()
        {

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

        public async Task<bool> ConnectAsync(TimeSpan timeout)
        {
            return await _client.ConnectAsync(timeout);
        }

        public void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            _client.SendAsync(data, offset, length, deliveryMethod);
        }

        public void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            _client.SendAsync(dataWriter, deliveryMethod);
        }

        private Task OnReceive(NetDataReader reader)
        {
            return Task.CompletedTask;
        }

        private void OnError(Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
}
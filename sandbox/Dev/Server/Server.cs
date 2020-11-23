using EuNet.Core;
using EuNet.Server;
using Common;
using System;
using System.Threading.Tasks;
using Common.Resolvers;

namespace StarterServer
{
    public class Server
    {
        private NetServer _server;
        private P2pGroup _p2pGroup;
        public P2pGroup P2pGroup => _p2pGroup;

        private static Server _instance;
        public static Server Instance => _instance;

        public Server()
        {
            _instance = this;

            // 자동으로 생성된 리졸버를 등록해줌
            CustomResolver.Register(GeneratedResolver.Instance);
        }

        public async Task Start()
        {
            // 서버 옵션을 정의
            var serverOption = new ServerOption()
            {
                Name = "StarterServer",
                TcpServerPort = 12000,
                IsServiceUdp = true,
                UdpServerPort = 12001,
                MaxSession = 1000,
                IsCheckAlive = true,
                CheckAliveInterval = 50000,
                CheckAliveTimeout = 60000,
                PacketFilter = new XorPacketFilter(1)
            };

            // 로거 팩토리를 생성
            var loggerFactory = DefaultLoggerFactory.Create(
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddConsoleLogger();
                }
            );

            var statistics = new NetStatistic();

            // UserSession 을 사용하기 위해서 팩토리를 만듬
            var sessionFactory = new DefaultSessionFactory(
                serverOption,
                loggerFactory,
                statistics,
                (createInfo) => {
                    return new UserSession(createInfo);
                });

            // 서버를 생성
            _server = new NetServer(
                serverOption,
                statistics,
                loggerFactory,
                sessionFactory);

            // 자동으로 생성된 Rpc 서비스를 등록함
            _server.AddRpcService(new LoginRpcServiceSession());
            _server.AddRpcService(new ShopRpcServiceSession());

            // P2p 그룹을 만듬 (현재는 1개만 필요하니 한개만 만듬. 여러개 생성가능)
            _p2pGroup = _server.P2pManager.CreateP2pGroup();

            // 서버를 시작함
            await _server.StartAsync();

            // 메인스레드에 키 입력을 받을 수 있게 함
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("quit");
                    break;
                }
            }

            // 서버 정지
            await _server.StopAsync();
        }
    }
}

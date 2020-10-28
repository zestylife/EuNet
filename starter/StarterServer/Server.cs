using EuNet.Core;
using EuNet.Server;
using StarterCommon;
using System;
using System.Threading.Tasks;

namespace StarterServer
{
    public class Server
    {
        private NetServer _server;

        public async Task Start()
        {
            var serverOption = new ServerOption()
            {
                Name = "StarterServer",
                TcpServerPort = 13000,
                IsServiceUdp = true,
                UdpServerPort = 13001,
                MaxSession = 1000,
            };

            var loggerFactory = new ConsoleLoggerFactory();
            var statistics = new NetStatistic();
            var sessionFactory = new DefaultSessionFactory(
                serverOption,
                loggerFactory,
                statistics,
                (sessionId, tcpChannel, udpChannel) => {
                    return new UserSession(sessionId, tcpChannel, udpChannel);
                });

            _server = new NetServer(
                serverOption,
                statistics,
                loggerFactory,
                sessionFactory);

            _server.AddRpcService(new LoginRpcServiceSession());
            _server.AddRpcService(new ShopRpcServiceSession());

            await _server.StartAsync();

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("quit");
                    break;
                }
            }

            await _server.StopAsync();
        }
    }
}

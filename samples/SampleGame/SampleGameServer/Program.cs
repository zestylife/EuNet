using EuNet.Core;
using EuNet.Server;
using SampleGameCommon;
using System;
using System.Threading.Tasks;

namespace SampleGameServer
{
    public class Program
    {
        private NetServer _server;
        private P2pGroup _p2pGroup;
        public P2pGroup P2pGroup => _p2pGroup;

        private static Program _instance;
        public static Program Instance => _instance;

        public Program()
        {
            _instance = this;
        }

        public void Run()
        {
            var serverOption = new ServerOption()
            {
                Name = "SampleGameServer",
                TcpServerPort = 12000,
                IsServiceUdp = true,
                UdpServerPort = 12001,
                MaxSession = 100,
            };

            var loggerFactory = DefaultLoggerFactory.Create(
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddConsoleLogger();
                }
            );

            var statistics = new NetStatistic();
            var sessionFactory = new DefaultSessionFactory(
                serverOption,
                loggerFactory,
                statistics,
                (createInfo) => {
                    return new UserSession(createInfo); 
                }); 

            _server = new NetServer(
                serverOption,
                statistics,
                loggerFactory,
                sessionFactory);

            _server.AddRpcService(new LoginRpcServiceSession());

            _server.StartAsync().Wait();

            _p2pGroup = _server.P2pManager.CreateP2pGroup();

            while (true)
            {
                var key = Console.ReadKey();
                if(key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("quit");
                    break;
                }
            }

            _server.StopAsync().Wait();
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }
    }
}

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EuNet.Client;
using EuNet.Core;
using EuNet.Server;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EuNet.Benchmark
{
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 1)]
    [MemoryDiagnoser]
    [RPlotExporter]
    public class Program
    {
        private NetServer _server;
        private TaskCompletionSource<bool> _tcs;
        private int _receivedCount = 0;

        private const int SendCount = 1000000;

        public Program()
        {
            _server = new NetServer(new ServerOption()
            {
                Name = "TestServer",
                TcpServerPort = 9000,
                IsServiceUdp = true,
                UdpServerPort = 9001,
                MaxSession = 10000,
            });

            _server.OnSessionReceived += (ISession session, NetDataReader reader) =>
            {
                var text = reader.ReadString();

                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    writer.Write(text);
                    session.SendAsync(writer, DeliveryMethod.Tcp);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }

                _receivedCount++;
                if (_receivedCount == SendCount)
                {
                    _tcs.TrySetResult(true);
                }

                return Task.CompletedTask;
            };

            _tcs = new TaskCompletionSource<bool>();
        }

        private async Task Begin()
        {
            Console.WriteLine("Start");
            await _server.StartAsync();
        }

        [Benchmark]
        public async Task Run()
        {
            await Begin();
            await WorkClient();
            await _tcs.Task;
            await End();

            Console.WriteLine(NetPool.PacketPool.ToString());
            Console.WriteLine(NetPool.DataWriterPool.ToString());
        }

        private async Task End()
        {
            Console.WriteLine("Stop");
            await _server.StopAsync();
        }

        public async Task WorkClient()
        {
            var tcs = new TaskCompletionSource<bool>();
            int receivedCount = 0;

            NetClient client = new NetClient(new ClientOption()
            {
                TcpServerAddress = "127.0.0.1",
                TcpServerPort = 9000,
                IsServiceUdp = true,
                UdpServerAddress = "127.0.0.1",
                UdpServerPort = 9001
            });

            client.OnReceived += (reader) =>
            {
                receivedCount++;
                if (receivedCount == SendCount)
                {
                    tcs.TrySetResult(true);
                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            };

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(100);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (client.State != SessionState.Closed)
                {
                    sw.Stop();
                    client.Update((int)sw.ElapsedMilliseconds);
                    sw.Restart();
                    await Task.Delay(30);
                }
            }).DoNotAwait();

            var connected = await client.ConnectAsync(null);

            for (int i = 0; i < SendCount; i++)
            {
                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    writer.Write("Finish");
                    client.SendAsync(writer, DeliveryMethod.Tcp);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }
            }

            await tcs.Task;

            client.Close();
        }

        static void Main(string[] args)
        {
            //Program program = new Program();
            //program.Run();

            BenchmarkRunner.Run<Program>();

        }
    }
}

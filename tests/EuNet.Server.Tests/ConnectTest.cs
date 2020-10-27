using EuNet.Client;
using EuNet.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EuNet.Server.Tests
{
    public class ConnectTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test(
            [Values(100)] int clientCount,
            [Values(100)] int sendCount,
            [Values(false, true)] bool isServiceUdp)
        {
            var serverTcs = new TaskCompletionSource<string>();
            int receivedUnreliableCount = 0;
            int receivedTcpCount = 0;

            NetServer server = new NetServer(
                new ServerOption()
                {
                    Name = "TestServer",
                    TcpServerPort = 9000,
                    IsServiceUdp = isServiceUdp,
                    UdpServerPort = 9001,
                    TcpBackLog = Math.Max(clientCount, 512),
                    MaxSession = clientCount,
                });

            server.OnSessionReceived += (ISession session, NetDataReader reader) =>
            {
                var text = reader.ReadString();

                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    if (text == "Unreliable")
                    {
                        Interlocked.Increment(ref receivedUnreliableCount);

                        writer.Write("Unreliable");

                        session.SendAsync(writer, DeliveryMethod.Unreliable);
                    }
                    else
                    {
                        Interlocked.Increment(ref receivedTcpCount);

                        if (text == "Finish")
                        {
                            writer.Write("Finish");
                        }
                        else writer.Write($"Hello Client{session.SessionId}");

                        session.SendAsync(writer, DeliveryMethod.Tcp);
                    }
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }

                return Task.CompletedTask;
            };

            server.OnSessionErrored += (ISession session, Exception ex) =>
            {
                serverTcs.TrySetException(ex);
                Assert.Fail(ex.ToString());
            };

            await server.StartAsync();

            Assert.AreEqual("Started", server.State.ToString());

            List<Task<NetClient>> taskList = new List<Task<NetClient>>();
            for (int i = 0; i < clientCount; i++)
            {
                taskList.Add(WorkClient(isServiceUdp, sendCount));
            }

            await Task.WhenAny(Task.WhenAll(taskList), serverTcs.Task);

            Assert.AreEqual(clientCount, server.SessionCount);

            foreach (var task in taskList)
                task.Result.Close();

            await Task.Delay(1000);

            Assert.AreEqual(0, server.SessionCount);

            await server.StopAsync();

            await Task.Delay(1000);

            Assert.AreEqual("Stopped", server.State.ToString());

            Console.WriteLine($"Server receivedTcpCount : {receivedTcpCount}");
            Console.WriteLine($"Server receivedUnreliableCount : {receivedUnreliableCount}");

            Console.WriteLine("****** PacketPool ******");
            Console.WriteLine(NetPool.PacketPool.ToString());
            Console.WriteLine("");

            Console.WriteLine("****** DataWriterPool ******");
            Console.WriteLine(NetPool.DataWriterPool.ToString());
            Console.WriteLine("");
            
            Console.WriteLine(server.Statistic.ToString());

            Assert.AreEqual(clientCount * sendCount, receivedTcpCount);
            Assert.AreEqual(0, NetPool.PacketPool.AllocCount);
            Assert.AreEqual(0, NetPool.DataWriterPool.AllocCount);

            Assert.Pass();
        }

        private async Task<NetClient> WorkClient(bool isServiceUdp, int sendCount)
        {
            NetClient client = new NetClient(new ClientOption()
            {
                TcpServerAddress = "127.0.0.1",
                TcpServerPort = 9000,
                IsServiceUdp = isServiceUdp,
                UdpServerAddress = "127.0.0.1",
                UdpServerPort = 9001
            });

            int receivedUnreliableCount = 0;
            var tcs = new TaskCompletionSource<string>();

            client.OnReceived += (NetDataReader reader) =>
            {
                string text = reader.ReadString();

                if (text == "Finish")
                    tcs.SetResult(text);
                else if (text == "Unreliable")
                {
                    receivedUnreliableCount++;
                }
                else
                {
                    Assert.AreEqual($"Hello Client{client.SessionId}", text);
                }

                return Task.CompletedTask;
            };

            client.OnErrored += (e) =>
            {
                tcs.SetException(e);
            };

            client.OnClosed += () =>
            {
                //tcs.SetException(new Exception("session closed"));
                tcs.TrySetException(new Exception("session closed"));
            };

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(100);
                Stopwatch sw = Stopwatch.StartNew();
                while (client.State != SessionState.Closed)
                {
                    sw.Stop();
                    client.Update((int)sw.ElapsedMilliseconds);
                    sw.Restart();
                    await Task.Delay(30);
                }
            }).DoNotAwait();

            var connected = await client.ConnectAsync(null);
            Assert.True(connected);

            for (int i = 0; i < sendCount; i++)
            {
                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    if (i < sendCount - 1)
                        writer.Write("Hello Server");
                    else writer.Write("Finish");

                    client.SendAsync(writer, DeliveryMethod.Tcp);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }

                if (isServiceUdp)
                {
                    writer = NetPool.DataWriterPool.Alloc();
                    try
                    {
                        writer.Write("Unreliable");

                        client.SendAsync(writer, DeliveryMethod.Unreliable);
                    }
                    finally
                    {
                        NetPool.DataWriterPool.Free(writer);
                    }
                }
            }

            await Task.Delay(1000);

            await tcs.Task;

            Assert.True(tcs.Task.IsCompletedSuccessfully);
            Assert.AreEqual("Finish", tcs.Task.Result);

            if (isServiceUdp)
                Console.WriteLine($"receivedUnreliableCount : {receivedUnreliableCount}");

            return client;
        }
    }
}
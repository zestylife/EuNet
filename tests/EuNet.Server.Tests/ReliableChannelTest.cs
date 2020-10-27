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
    public class ReliableChannelTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test(
            [Values(100)] int clientCount,
            [Values(1000)] int sendCount)
        {
            var serverTcs = new TaskCompletionSource<string>();
            int receivedUnreliableCount = 0;
            int receivedReliableCount = 0;
            int receivedReliableUnorderedCount = 0;

            NetServer server = new NetServer(
                new ServerOption()
                {
                    Name = "TestServer",
                    TcpServerPort = 9000,
                    IsServiceUdp = true,
                    UdpServerPort = 9001,
                    TcpBackLog = Math.Max(clientCount, 512),
                    MaxSession = clientCount + 100,
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
                    else if (text == "ReliableUnordered")
                    {
                        Interlocked.Increment(ref receivedReliableUnorderedCount);

                        writer.Write("ReliableUnordered");

                        session.SendAsync(writer, DeliveryMethod.ReliableUnordered);
                    }
                    else
                    {
                        Interlocked.Increment(ref receivedReliableCount);

                        if (text == "Finish")
                        {
                            writer.Write("Finish");
                        }
                        else writer.Write($"Hello Client{session.SessionId}");

                        session.SendAsync(writer, DeliveryMethod.ReliableOrdered);
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
                taskList.Add(WorkClient(sendCount));
            }

            await Task.WhenAny(Task.WhenAll(taskList), serverTcs.Task);

            Assert.AreEqual(clientCount, server.SessionCount);

            foreach (var task in taskList)
                task.Result.Close();

            await Task.Delay(3000);

            Assert.AreEqual(0, server.SessionCount);

            await server.StopAsync();

            await Task.Delay(1000);

            Assert.AreEqual("Stopped", server.State.ToString());

            Console.WriteLine($"Server receivedReliableCount : {receivedReliableCount}");
            Console.WriteLine($"Server receivedUnreliableCount : {receivedUnreliableCount}");
            Console.WriteLine($"Server receivedReliableUnorderedCount : {receivedReliableUnorderedCount}");

            Console.WriteLine("****** PacketPool ******");
            Console.WriteLine(NetPool.PacketPool.ToString());
            Console.WriteLine("");

            Console.WriteLine("****** DataWriterPool ******");
            Console.WriteLine(NetPool.DataWriterPool.ToString());
            Console.WriteLine("");

            Console.WriteLine(server.Statistic.ToString());

            Assert.AreEqual(clientCount * sendCount, receivedReliableCount);
            Assert.AreEqual(0, NetPool.PacketPool.AllocCount);
            Assert.AreEqual(0, NetPool.DataWriterPool.AllocCount);
            
            Assert.Pass();
        }

        private async Task<NetClient> WorkClient(int sendCount)
        {
            NetClient client = new NetClient(new ClientOption()
            {
                TcpServerAddress = "127.0.0.1",
                TcpServerPort = 9000,
                IsServiceUdp = true,
                UdpServerAddress = "127.0.0.1",
                UdpServerPort = 9001
            });

            int receivedUnreliableCount = 0;
            int receivedReliableUnorderedCount = 0;
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
                else if (text == "ReliableUnordered")
                {
                    receivedReliableUnorderedCount++;
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
                    writer.Write("ReliableUnordered");

                    client.SendAsync(writer, DeliveryMethod.ReliableUnordered);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }

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

                writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    if (i < sendCount - 1)
                        writer.Write("Hello Server");
                    else writer.Write("Finish");

                    client.SendAsync(writer, DeliveryMethod.ReliableOrdered);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }

                await Task.Delay(1);
            }

            await Task.Delay(1000);

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(60000);
                tcs.TrySetResult("Finish");
            }).DoNotAwait();

            await tcs.Task;

            Assert.True(tcs.Task.IsCompletedSuccessfully);
            Assert.AreEqual("Finish", tcs.Task.Result);

            Console.WriteLine($"receivedUnreliableCount : {receivedUnreliableCount}");
            Console.WriteLine($"receivedReliableUnorderedCount : {receivedReliableUnorderedCount}");

            return client;
        }
    }
}
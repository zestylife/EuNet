using EuNet.Client;
using EuNet.Core;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EuNet.Server.Tests
{
    public class RequestTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test(
            [Values(1)] int clientCount,
            [Values(2)] int sendCount,
            [Values(false)] bool isServiceUdp)
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

            server.OnSessionRequestReceived += (ISession session, NetDataReader reader, NetDataWriter writer) =>
            {
                receivedTcpCount++;
                var text = reader.ReadString();

                if (text == "Exception")
                {
                    throw new Exception("MyException");
                }
                else
                {
                    writer.Write("Hello Client");
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

            await server.SessionManager.InvokeAllSessionAsync(async (ISession session) =>
            {
                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    writer.Write("Bye Client");
                    using (var reader = await session.RequestAsync(writer, DeliveryMethod.Tcp, null))
                    {
                        var result = reader.ReadString();
                        Assert.AreEqual("Bye Server", result);
                    }
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }
            });

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

            Assert.AreEqual(clientCount * 2, receivedTcpCount);
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

            client.OnRequestReceived = (ISession session, NetDataReader reader, NetDataWriter writer) =>
            {
                var text = reader.ReadString();
                if(text == "Bye Client")
                {
                    writer.Write("Bye Server");
                }

                return Task.CompletedTask;
            };

            client.OnErrored += (e) =>
            {
                
            };

            client.OnClosed += () =>
            {
                
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

            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write("Hello Server");
                using (var reader = await client.RequestAsync(writer, DeliveryMethod.Tcp, null))
                {
                    Assert.AreEqual("Hello Client", reader.ReadString());
                }
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }

            writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write("Exception");
                using (var reader = await client.RequestAsync(writer, DeliveryMethod.Tcp, null))
                {
                    Assert.Fail();
                }
            }
            catch(Exception ex)
            {
                
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }

            await Task.Delay(1000);

            if (isServiceUdp)
                Console.WriteLine($"receivedUnreliableCount : {receivedUnreliableCount}");

            return client;
        }
    }
}
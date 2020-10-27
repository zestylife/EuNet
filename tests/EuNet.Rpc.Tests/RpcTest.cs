using EuNet.Client;
using EuNet.Core;
using EuNet.Rpc.Test.Interface.Resolvers;
using EuNet.Server;
using NUnit.Framework;
using Rpc.Test.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EuNet.Rpc.Tests
{
    public class GreeterService : GreeterRpcServiceAbstract
    {
        public override Task<string> Greet(string name)
        {
            Console.WriteLine(name);

            return Task<string>.FromResult("Result greet! " + name);
        }

        public override Task<DataClass> GreetClass(DataClass dataClass)
        {
            return Task.FromResult(new DataClass()
            {
                Int = dataClass.Int + 1,
                Property = dataClass.Property + 1,
                String = dataClass.String + "1",
                IgnoreInt = dataClass.IgnoreInt,
                IgnoreProperty = dataClass.IgnoreProperty
            });
        }

        public override Task<InterfaceSerializeClass> GreetInterfaceSerializeClass(InterfaceSerializeClass dataClass)
        {
            return Task.FromResult(new InterfaceSerializeClass()
            {
                Int = dataClass.Int + 1,
            });
        }

        public override Task<string> SessionParameter(ISession session)
        {
            return Task.FromResult("SessionParameter");
        }
    }

    public class RpcTest
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
            CustomResolver.Register(GeneratedResolver.Instance);

            var serverTcs = new TaskCompletionSource<string>();

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

            server.AddRpcService(new GreeterService());

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

            await server.SessionManager.InvokeAllSessionAsync(async (ISession session) =>
            {
                var rpc = new GreeterRpc(session);

                var result = await rpc.Greet("Greet client!");
                Console.WriteLine(result);

                result = await rpc.Greet("Greet client!");
                Console.WriteLine(result);
            });

            await Task.WhenAny(Task.WhenAll(taskList), serverTcs.Task);

            Assert.AreEqual(clientCount, server.SessionCount);

            foreach (var task in taskList)
                task.Result.Close();

            await Task.Delay(1000);

            Assert.AreEqual(0, server.SessionCount);

            await server.StopAsync();

            await Task.Delay(1000);

            Assert.AreEqual("Stopped", server.State.ToString());
            Console.WriteLine(server.Statistic.ToString());

            Console.WriteLine(NetPool.BufferPool.ToString());

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

            client.AddRpcService(new GreeterService());
            var rpc = new GreeterRpc(client);

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

            var result = await rpc.Greet("Greet server!");
            Assert.AreEqual("Result greet! Greet server!", result);
            Console.WriteLine(result);

            result = await rpc.Greet("Greet server!");
            Assert.AreEqual("Result greet! Greet server!", result);
            Console.WriteLine(result);

            var dataClass = new DataClass()
            {
                Int = 123,
                Property = 456,
                String = "789",
                IgnoreInt = 987,
                IgnoreProperty = 567,
            };
            var resultClass = await rpc.GreetClass(dataClass);
            Assert.AreEqual(dataClass.Int+1, resultClass.Int);
            Assert.AreEqual(dataClass.Property+1, resultClass.Property);
            Assert.AreEqual(dataClass.String+"1", resultClass.String);
            Assert.AreNotEqual(dataClass.IgnoreInt, resultClass.IgnoreInt);
            Assert.AreNotEqual(dataClass.IgnoreProperty, resultClass.IgnoreProperty);


            var interfaceSerializeClass = new InterfaceSerializeClass()
            {
                Int = 123,
            };
            var resultinterfaceSerializeClass = await rpc.GreetInterfaceSerializeClass(interfaceSerializeClass);

            Assert.AreEqual(interfaceSerializeClass.Int + 1, resultinterfaceSerializeClass.Int);


            rpc.WithNoReply().Greet("1234567890");

            await Task.Delay(1000);

            return client;
        }
    }
}
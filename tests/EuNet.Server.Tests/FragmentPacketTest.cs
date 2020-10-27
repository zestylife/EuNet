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
    public class FragmentPacketTest
    {
        //private const string testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean eu pharetra purus. Aliquam dictum rutrum consectetur. Mauris consectetur nunc in venenatis viverra. Maecenas ipsum ipsum, pulvinar et justo et, ullamcorper scelerisque augue. Nullam elementum lacus vitae aliquet auctor. Aenean quis mauris risus. Mauris aliquam consectetur tortor, at molestie metus. Nunc auctor varius urna eget vulputate. Duis eu vestibulum purus. Sed sit amet nisi a sapien aliquam gravida ac non sem. Duis quis sem pellentesque, luctus metus nec, volutpat mauris. Nulla eu rutrum libero, ut egestas urna. Aliquam dapibus et lacus ut dignissim. Nam ornare feugiat nisi, vitae finibus dui malesuada ut. Donec pharetra felis urna, vitae lobortis libero molestie non. Proin tempor odio elit, eu molestie erat aliquam quis. Ut a pellentesque erat. Curabitur blandit congue dui ut rutrum. Proin id commodo arcu, nec lacinia metus. Sed gravida ante in risus malesuada, et consequat dui volutpat. Vestibulum ut porttitor enim. In efficitur, neque a sodales tristique, nunc velit dictum sem, suscipit congue libero mi eu risus. Curabitur molestie rhoncus ligula, a vulputate quam blandit quis. Aenean diam turpis, commodo at leo ac, tristique tempus nisi. Aliquam sit amet magna nunc. Nulla congue lacus dignissim sem tempor malesuada. Cras iaculis vitae ante a faucibus. Sed porttitor lorem ut convallis consectetur. Vivamus vel tortor imperdiet, vestibulum leo a, bibendum nulla. Suspendisse potenti. Nulla facilisi. Curabitur eu suscipit augue, non tincidunt nulla. Curabitur interdum pretium lacus ut scelerisque. Vestibulum tellus odio, pellentesque nec lacus eu, tincidunt viverra arcu. Vestibulum condimentum sapien eu purus dignissim, ut pulvinar mi efficitur. Aliquam quis mi sed mi hendrerit fringilla. Praesent magna enim, bibendum quis enim vel, blandit malesuada erat. Integer bibendum metus eu augue finibus cursus. Proin molestie nibh a sapien finibus hendrerit. Donec consectetur hendrerit egestas. Duis quis orci purus. Phasellus efficitur aliquet odio sit amet gravida. Maecenas rhoncus luctus massa et aliquam. Suspendisse ultricies nibh velit, et suscipit nunc rhoncus vel. Aliquam ut justo bibendum, congue ante nec, pellentesque nisi. Praesent imperdiet fringilla dolor sed gravida. Etiam ut mauris feugiat turpis sodales iaculis sit amet ac ligula. Morbi convallis sem erat, at vulputate massa consectetur ac. Quisque arcu dui, vestibulum vitae imperdiet eget, euismod sed nisl. Vivamus et egestas risus, vel dignissim elit. Nunc sagittis luctus mi, nec dignissim lectus faucibus ut. Donec malesuada quam vitae massa pharetra, et dictum magna blandit. Fusce pharetra dui eu auctor placerat. Pellentesque euismod massa vitae dui sodales, vitae interdum metus rutrum. Proin dapibus neque quis nulla convallis, vitae gravida ipsum molestie. Curabitur interdum ultricies mi ac accumsan. Nulla facilisi. Sed sollicitudin eros in neque ultricies vestibulum. Quisque quis augue at risus dapibus blandit. Nullam maximus, purus quis dictum hendrerit, augue elit posuere leo, vel maximus justo sapien sed lectus. Suspendisse quis magna at tortor faucibus volutpat ac vitae velit. Proin bibendum finibus dolor at ultricies. Nulla in feugiat ante. Nullam ipsum massa, feugiat ac laoreet sit amet, congue sed metus. Donec feugiat congue sapien vitae efficitur. In malesuada ligula quis magna rutrum, vitae efficitur tellus sagittis. Aliquam vel elit sed ligula posuere iaculis. In hac habitasse platea dictumst. Duis tempor fermentum vestibulum. Donec ullamcorper mauris non consequat lacinia. Interdum et malesuada fames ac ante ipsum primis in faucibus. Etiam finibus, sem non pretium semper, tellus sapien laoreet ex, eu accumsan massa nulla sed diam. Nam eu purus at sem pulvinar hendrerit ut vel tellus. Proin dapibus dui in est molestie consectetur. Etiam sagittis justo lacus, non egestas mi finibus et. Nunc eros felis, imperdiet molestie leo at, accumsan gravida felis. Suspendisse potenti. Maecenas maximus venenatis nulla sed hendrerit. Integer volutpat sed dolor sit amet mollis. Ut ut posuere tellus, et mollis magna. Vestibulum ac tempor sapien. Nulla semper porttitor sem, a viverra enim varius id. Mauris eleifend ante leo, vel facilisis nisl consequat ut. Pellentesque rhoncus vestibulum tellus, sit amet faucibus ipsum viverra sit amet. Quisque placerat felis nec lacus dictum vehicula. Vivamus placerat nulla magna, eget auctor ligula volutpat euismod. In odio metus, cursus at pulvinar quis, iaculis vitae dolor. Donec ut erat eget tellus tristique aliquam eget sed justo. Duis ut aliquam purus. Sed nec arcu laoreet, finibus elit nec, egestas orci. Proin ac turpis ut est interdum volutpat vel nec magna. Donec vehicula neque id felis cursus, ut consequat est varius. Vivamus sit amet aliquam leo, vitae iaculis nisi. Duis libero nisi, tristique in hendrerit et, semper eget ligula. Suspendisse at elementum metus. Aliquam erat volutpat. Donec eget gravida massa. Donec congue ipsum eget libero posuere.";
        private const string testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean eu pharetra purus. Aliquam dictum rutrum consectetur. Mauris consectetur nunc in venenatis viverra. Maecenas ipsum ipsum, pulvinar et justo et, ullamcorper scelerisque augue. Nullam elementum lacus vitae aliquet auctor. Aenean quis mauris risus. Mauris aliquam consectetur tortor, at molestie metus. Nunc auctor varius urna eget vulputate. Duis eu vestibulum purus. Sed sit amet nisi a sapien aliquam gravida ac non sem. Duis quis sem pellentesque, luctus metus nec, volutpat mauris. Nulla eu rutrum libero, ut egestas urna. Aliquam dapibus et lacus ut dignissim. Nam ornare feugiat nisi, vitae finibus dui malesuada ut. Donec pharetra felis urna, vitae lobortis libero molestie non. Proin tempor odio elit, eu molestie erat aliquam quis. Ut a pellentesque erat. Curabitur blandit congue dui ut rutrum. Proin id commodo arcu, nec lacinia metus. Sed gravida ante in risus malesuada, et consequat dui volutpat. Vestibulum ut porttitor enim. In efficitur, neque a sodales tristique, nunc velit dictum sem, suscipit congue libero mi eu risus. Curabitur molestie rhoncus ligula, a vulputate quam blandit quis. Aenean diam turpis, commodo at leo ac, tristique tempus nisi. Aliquam sit amet magna nunc. Nulla congue lacus dignissim sem tempor malesuada. Cras iaculis vitae ante a faucibus. Sed porttitor lorem ut convallis consectetur. Vivamus vel tortor imperdiet, vestibulum leo a, bibendum nulla. Suspendisse potenti. Nulla facilisi. Curabitur eu suscipit augue, non tincidunt nulla. Curabitur interdum pretium lacus ut scelerisque. Vestibulum tellus odio, pellentesque nec lacus eu";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test()
        {
            var serverTcs = new TaskCompletionSource<string>();
            int receivedUnreliableCount = 0;
            int receivedTcpCount = 0;

            NetServer server = new NetServer(
                new ServerOption()
                {
                    Name = "TestServer",
                    TcpServerPort = 9000,
                    IsServiceUdp = true,
                    UdpServerPort = 9001,
                    MaxSession = 100,
                });

            server.OnSessionReceived += (ISession session, NetDataReader reader) =>
            {
                var text = reader.ReadString();

                var writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    Interlocked.Increment(ref receivedUnreliableCount);
                    writer.Write(text);
                    session.SendAsync(writer, DeliveryMethod.Unreliable);
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
            for (int i = 0; i < 1; i++)
            {
                taskList.Add(WorkClient());
            }

            await Task.WhenAny(Task.WhenAll(taskList), serverTcs.Task);

            Assert.AreEqual(1, server.SessionCount);

            foreach (var task in taskList)
                task.Result.Close();

            await Task.Delay(3000);

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

            Assert.AreEqual(0, NetPool.PacketPool.AllocCount);
            Assert.AreEqual(0, NetPool.DataWriterPool.AllocCount);
            
            Assert.Pass();
        }

        private async Task<NetClient> WorkClient()
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
            var tcs = new TaskCompletionSource<string>();

            client.OnReceived += (NetDataReader reader) =>
            {
                string text = reader.ReadString();

                tcs.SetResult(text);
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

            for (int i = 0; i < 1; i++)
            {
                NetDataWriter writer = NetPool.DataWriterPool.Alloc();
                try
                {
                    writer.Write(testString);
                    client.SendAsync(writer, DeliveryMethod.Unreliable);
                }
                finally
                {
                    NetPool.DataWriterPool.Free(writer);
                }
            }

            await Task.Delay(1000);

            await tcs.Task;

            Assert.True(tcs.Task.IsCompletedSuccessfully);
            Assert.AreEqual(testString, tcs.Task.Result);

            Console.WriteLine($"receivedUnreliableCount : {receivedUnreliableCount}");

            return client;
        }
    }
}
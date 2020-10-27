using EuNet.Core;
using EuNet.Rpc.Test.Interface.Resolvers;
using NUnit.Framework;
using Rpc.Test.Interface;

namespace EuNet.Rpc.Tests
{
    public class SerializerTest
    {
        [SetUp]
        public void Setup()
        {

        }

        private T TestBase<T>(T data)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                NetDataSerializer.Serialize<T>(writer, data);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }

            byte[] buffer = writer.CopyData();

            var reader = new NetDataReader(buffer);
            return NetDataSerializer.Deserialize<T>(reader);
        }

        [Test]
        public void TestNetDataObject()
        {
            // 생성된 리졸버를 등록시켜줘야 함
            CustomResolver.Register(GeneratedResolver.Instance);

            DataClass data = new DataClass()
            {
                Int = 123,
                String = "456",
                Property = 789,
                IgnoreInt = 999998,
                IgnoreProperty = 999999
            };

            var deserializeObject = TestBase<DataClass>(data);

            Assert.AreEqual(data.Int, deserializeObject.Int);
            Assert.AreEqual(data.String, deserializeObject.String);
            Assert.AreEqual(data.Property, deserializeObject.Property);

            Assert.AreNotEqual(data.IgnoreInt, deserializeObject.IgnoreInt);
            Assert.AreNotEqual(data.IgnoreProperty, deserializeObject.IgnoreProperty);

        }

        [Test]
        public void TestGenericNetDataObject()
        {
            // 생성된 리졸버를 등록시켜줘야 함
            CustomResolver.Register(GeneratedResolver.Instance);

            GenericDataClass<string> data = new GenericDataClass<string>()
            {
                GenericValue = "generic_test"
            };

            var deserializeObject = TestBase<GenericDataClass<string>>(data);

            Assert.AreEqual(data.GenericValue, deserializeObject.GenericValue);
        }
    }
}
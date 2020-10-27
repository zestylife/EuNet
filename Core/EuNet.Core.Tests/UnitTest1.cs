using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace EuNet.Core.Tests
{
    public class Tests
    {
        private AsyncObjectQueue<int> _queue = new AsyncObjectQueue<int>();
        private bool _pass = false;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task Test1()
        {
            Task t1 = WaitAsyncObject();
            await Task.Delay(100);

            _queue.Enqueue(123);

            Task t2 = WaitAsyncObject();
            await Task.Delay(100);

            _queue.TrySetException(new Exception("test"));

            await Task.Delay(100);
            Assert.True(_pass);

            Task.WaitAll(t1, t2);
        }

        public async Task WaitAsyncObject()
        {
            try
            {
                bool result = await _queue.WaitAsync();
                _queue.Reset();

                Assert.AreEqual(true, result);

                int value;
                _queue.Queue.TryDequeue(out value);
                Assert.AreEqual(123, value);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("test", ex.Message);

                if (ex.Message == "test")
                    _pass = true;
            }
        }
    }
}